using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Core.UserProvide;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.System;
using KH.WMS.Entities.System.Enums;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Newtonsoft.Json;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Modules.SystemModule.Services
{
    /// <summary>
    /// 系统菜单权限服务实现
    /// </summary>
    [RegisteredService(ServiceType = typeof(ISysPermissionService))]
    public class SysPermissionService(
        IRepository<SysPermission, long> permissionRepository,
        IRepository<SysRolePermission, long> rolePermissionRepository,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        IUserContext userContext) : CrudService<SysPermission>(permissionRepository, unitOfWork, detailSaveService), ISysPermissionService
    {
        private readonly IRepository<SysPermission, long> _permissionRepository = permissionRepository;
        private readonly IRepository<SysRolePermission, long> _rolePermissionRepository = rolePermissionRepository;
        private readonly ICacheService _cacheService = cacheService;
        private readonly IUserContext _userContext = userContext;

        /// <inheritdoc />
        public async Task<ApiResponse> GetMenuTreeAsync()
        {
            var allMenus = await GetActiveMenusAsync();
            var tree = BuildMenuTree(allMenus, 0);
            return ApiResponse.Ok(tree);
        }

        /// <inheritdoc />
        public async Task<ApiResponse> GetMenuTreeByRoleIdAsync(long roleId)
        {
            // 超级管理员返回所有菜单（含禁用），便于在菜单管理中维护禁用项；其他角色仅返回启用的菜单
            if (roleId == RoleConstants.SUPER_ADMIN_ROLE_ID)
            {
                var allMenus = await GetAllMenusAsync();
                var treeData = BuildMenuTree(allMenus, 0);
                return ApiResponse.Ok(treeData);
            }

            var activeMenus = await GetActiveMenusAsync();

            // 获取角色关联的权限（含按钮粒度）
            var rolePermissions = await _rolePermissionRepository
                .GetListAsync(rp => rp.RoleId == roleId);

            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            // 构建每个菜单允许的按钮映射：PermissionId → 允许的 permKey 集合（null 表示全部允许）
            var buttonAllowMap = rolePermissions
                .Where(rp => !string.IsNullOrEmpty(rp.AllowedButtons))
                .ToDictionary(
                    rp => rp.PermissionId,
                    rp => JsonConvert.DeserializeObject<List<string>>(rp.AllowedButtons!).ToHashSet());

            // 筛选角色有权限的菜单，并补全父级目录
            var authorizedMenus = activeMenus.Where(m => permissionIds.Contains(m.Id)).ToList();
            var allIds = new HashSet<long>();
            CollectWithParents(activeMenus, authorizedMenus, allIds);

            var roleMenus = activeMenus.Where(m => allIds.Contains(m.Id)).ToList();
            var tree = BuildMenuTree(roleMenus, 0, buttonAllowMap);
            return ApiResponse.Ok(tree);
        }

        /// <inheritdoc />
        public async Task<ApiResponse> GetDirectoriesAsync()
        {
            var directories = await _permissionRepository.GetListAsync(
                p => p.MenuType == (byte)MenuTypeEnum.Directory && p.Status == BizConstants.BoolFlag.YES);
            var result = directories.OrderBy(d => d.SortNo).Select(d => new MenuTreeNodeDto
            {
                Id = d.Id,
                PermissionCode = d.PermissionCode,
                PermissionName = d.PermissionName,
                ParentId = d.ParentId,
                MenuType = d.MenuType,
                Icon = d.Icon,
                SortNo = d.SortNo,
                IsVisible = d.IsVisible,
                Status = d.Status,
                IsExternal = d.IsExternal,
                IsCache = d.IsCache,
                Remark = d.Remark
            }).ToList();
            return ApiResponse.Ok(result);
        }

        /// <inheritdoc />
        public async Task<ApiResponse> GetMenusByDirectoryIdAsync(long directoryId)
        {
            var menus = await _permissionRepository.GetListAsync(
                p => p.ParentId == directoryId && p.MenuType == (byte)MenuTypeEnum.Menu && p.Status == BizConstants.BoolFlag.YES);
            var result = menus.OrderBy(m => m.SortNo).Select(p => MapToTreeNode(p)).ToList();
            return ApiResponse.Ok(result);
        }

        /// <inheritdoc />
        public async Task<ApiResponse> SaveMenuAsync(SaveMenuDto dto)
        {
            // 验证菜单类型与父级ID的关系
            if (dto.MenuType == (byte)MenuTypeEnum.Directory && dto.ParentId != 0)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "目录的 ParentId 必须为 0");
            if (dto.MenuType == (byte)MenuTypeEnum.Menu && dto.ParentId == 0)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "菜单的 ParentId 不能为 0");

            // 如果是菜单类型，验证父级目录存在
            if (dto.MenuType == (byte)MenuTypeEnum.Menu)
            {
                var parent = await _permissionRepository.GetByIdAsync(dto.ParentId);
                if (parent == null)
                    return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "父级目录不存在");
                if (parent.MenuType != (byte)MenuTypeEnum.Directory)
                    return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "父级必须是目录类型");
            }

            // 编码唯一性检查
            var exists = await _permissionRepository.ExistsAsync(
                p => p.PermissionCode == dto.PermissionCode &&
                     (dto.Id == null || p.Id != dto.Id.Value));
            if (exists)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "权限编码已存在");

            if (dto.Id == null)
            {
                // 新增
                var entity = new SysPermission
                {
                    PermissionCode = dto.PermissionCode,
                    PermissionName = dto.PermissionName,
                    ParentId = dto.ParentId,
                    MenuType = dto.MenuType,
                    Path = dto.Path,
                    Component = dto.Component,
                    Icon = dto.Icon,
                    SortNo = dto.SortNo,
                    IsVisible = dto.IsVisible,
                    Status = dto.Status,
                    Buttons = JsonConvert.SerializeObject(dto.Buttons),
                    IsExternal = dto.IsExternal,
                    IsCache = dto.IsCache,
                    Remark = dto.Remark
                };
                var id = await _permissionRepository.AddAsync(entity);
                ClearMenuCache();
                return ApiResponse.Ok(id, "新增成功");
            }
            else
            {
                // 编辑
                var entity = await _permissionRepository.GetByIdAsync(dto.Id.Value);
                if (entity == null)
                    return ApiResponse.NotFound("菜单不存在");

                entity.PermissionCode = dto.PermissionCode;
                entity.PermissionName = dto.PermissionName;
                entity.ParentId = dto.ParentId;
                entity.MenuType = dto.MenuType;
                entity.Path = dto.Path;
                entity.Component = dto.Component;
                entity.Icon = dto.Icon;
                entity.SortNo = dto.SortNo;
                entity.IsVisible = dto.IsVisible;
                entity.Status = dto.Status;
                entity.Buttons = JsonConvert.SerializeObject(dto.Buttons);
                entity.IsExternal = dto.IsExternal;
                entity.IsCache = dto.IsCache;
                entity.Remark = dto.Remark;

                await _permissionRepository.UpdateAsync(entity);
                ClearMenuCache();
                return ApiResponse.Ok(message: "更新成功");
            }
        }

        /// <inheritdoc />
        public override async Task<ApiResponse> DeleteAsync(long menuId)
        {
            // 检查是否有子菜单
            var hasChildren = await _permissionRepository.ExistsAsync(p => p.ParentId == menuId);
            if (hasChildren)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "存在子菜单，无法删除");

            // "删除菜单 + 同步删除角色权限关联"需在同一事务
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var deleted = await _permissionRepository.DeleteAsync(menuId);
                if (!deleted)
                {
                    await _unitOfWork.RollbackAsync();
                    return ApiResponse.NotFound("菜单不存在");
                }

                // 同步删除角色关联
                await _rolePermissionRepository.DeleteAsync(rp => rp.PermissionId == menuId);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            ClearMenuCache();
            return ApiResponse.Ok(message: "删除成功");
        }

        /// <inheritdoc />
        public async Task<ApiResponse> AssignRolePermissionsAsync(AssignPermissionDto dto)
        {
            // 非超级管理员不能修改自己所属角色的权限
            if (!_userContext.IsSuperAdmin && dto.RoleId == (_userContext.RoleId ?? 0))
                return ApiResponse.Fail(ResponseCode.FORBIDDEN, "不允许修改当前登录用户所属角色的权限");

            // "删除旧权限关联 + 批量新增"为重写操作，必须包裹在单一事务中，
            // 否则清空后新增失败会导致角色丢失所有权限。
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 删除该角色现有的所有权限关联
                await _rolePermissionRepository.DeleteAsync(rp => rp.RoleId == dto.RoleId);

                // 批量新增新的权限关联（含按钮粒度控制）
                var entities = dto.Menus.Select(item =>
                {
                    string? allowedButtonsJson = null;
                    if (item.AllowedButtons != null && item.AllowedButtons.Count > 0)
                    {
                        // 如果传入的已经是JSON字符串（只有一个元素且以 [ 开头），直接使用，避免双重序列化
                        var first = item.AllowedButtons[0];
                        if (item.AllowedButtons.Count == 1 && first != null && first.TrimStart().StartsWith("["))
                            allowedButtonsJson = first;
                        else
                            allowedButtonsJson = JsonConvert.SerializeObject(item.AllowedButtons.ToArray());
                    }

                    return new SysRolePermission
                    {
                        RoleId = dto.RoleId,
                        PermissionId = item.PermissionId,
                        AllowedButtons = allowedButtonsJson,
                    };
                }).ToList();

                if (entities.Count > 0)
                {
                    await _rolePermissionRepository.AddAsync(entities);
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return ApiResponse.Ok(message: "权限分配成功");
        }

        #region Private Methods

        private async Task<List<SysPermission>> GetActiveMenusAsync()
        {
            return await _permissionRepository.GetListAsync(p => p.Status == BizConstants.BoolFlag.YES);
        }

        /// <summary>获取全部菜单（含禁用），供超级管理员菜单管理使用</summary>
        private async Task<List<SysPermission>> GetAllMenusAsync()
        {
            return await _permissionRepository.GetAllAsync();
        }

        private List<MenuTreeNodeDto> BuildMenuTree(List<SysPermission> all, long parentId,
            Dictionary<long, HashSet<string>>? buttonAllowMap = null)
        {
            return all
                .Where(p => p.ParentId == parentId)
                .OrderBy(p => p.SortNo)
                .Select(p => MapToTreeNode(p, buttonAllowMap))
                .Select(node =>
                {
                    node.Children = BuildMenuTree(all, node.Id, buttonAllowMap);
                    return node;
                }).ToList();
        }

        private void CollectWithParents(List<SysPermission> all, List<SysPermission> authorized, HashSet<long> ids)
        {
            foreach (var menu in authorized)
            {
                ids.Add(menu.Id);
                // 递归收集所有父级
                var current = menu;
                while (current.ParentId != 0)
                {
                    ids.Add(current.ParentId);
                    current = all.FirstOrDefault(m => m.Id == current.ParentId)!;
                    if (current == null) break;
                }
            }
        }

        private static MenuTreeNodeDto MapToTreeNode(SysPermission p,
            Dictionary<long, HashSet<string>>? buttonAllowMap = null)
        {
            var buttons = string.IsNullOrEmpty(p.Buttons)
                ? null
                : JsonConvert.DeserializeObject<List<MenuButtonDto>>(p.Buttons);

            // 如果有按钮过滤映射，则只保留允许的按钮
            if (buttons != null && buttonAllowMap != null && buttonAllowMap.TryGetValue(p.Id, out var allowedKeys))
            {
                buttons = buttons.Where(b => allowedKeys.Contains(b.PermKey)).ToList();
            }

            return new MenuTreeNodeDto
            {
                Id = p.Id,
                PermissionCode = p.PermissionCode,
                PermissionName = p.PermissionName,
                ParentId = p.ParentId,
                MenuType = p.MenuType,
                Path = p.Path,
                Component = p.Component,
                Icon = p.Icon,
                SortNo = p.SortNo,
                IsVisible = p.IsVisible,
                Status = p.Status,
                IsExternal = p.IsExternal,
                IsCache = p.IsCache,
                Buttons = buttons,
                Remark = p.Remark
            };
        }

        private void ClearMenuCache()
        {
            _cacheService.Remove(CacheConstants.Data.GetListKey<SysPermission>());
        }

        #endregion
    }
}
