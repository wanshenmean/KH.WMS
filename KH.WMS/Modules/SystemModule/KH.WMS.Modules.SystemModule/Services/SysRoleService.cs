using System.Linq.Expressions;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Helpers;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Services;
using KH.WMS.Core.UserProvide;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using SqlSugar;

namespace KH.WMS.Modules.SystemModule.Services
{
    /// <summary>
    /// 系统角色服务实现
    /// </summary>
    [RegisteredService(ServiceType = typeof(ISysRoleService))]
    public class SysRoleService(
        IRepository<SysRole, long> roleRepository,
        ISqlSugarClient db,
        IRepository<SysUserRole, long> userRoleRepository,
        IRepository<SysRolePermission, long> rolePermissionRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<SysRole>(roleRepository, unitOfWork, detailSaveService), ISysRoleService
    {
        private readonly ISqlSugarClient _db = db;
        private readonly IRepository<SysRole, long> _roleRepository = roleRepository;
        private readonly IRepository<SysUserRole, long> _userRoleRepository = userRoleRepository;
        private readonly IRepository<SysRolePermission, long> _rolePermissionRepository = rolePermissionRepository;
        private readonly IUserContext _userContext = userContext;

        /// <summary>
        /// 检查目标角色是否在当前用户的操作权限范围内。
        /// 超级管理员可以操作所有角色，非超级管理员只能操作下级角色（不能操作自身）。
        /// </summary>
        private async Task<(bool allowed, ApiResponse? failResponse)> CheckRolePermission(long targetRoleId, string operationName)
        {
            if (_userContext.IsSuperAdmin)
                return (true, null);

            var currentRoleId = _userContext.RoleId ?? 0;

            // 不能操作自身角色
            if (currentRoleId == targetRoleId)
                return (false, ApiResponse.Fail(ResponseCode.FORBIDDEN, $"不允许{operationName}当前登录用户所属的角色"));

            // 目标角色必须是当前角色的子孙角色
            var descendantIds = await GetDescendantRoleIdsAsync(currentRoleId);
            if (!descendantIds.Contains(targetRoleId))
                return (false, ApiResponse.Fail(ResponseCode.FORBIDDEN, $"无权{operationName}该角色，该角色不在当前角色的管辖范围内"));

            return (true, null);
        }


        public async Task<ApiResponse> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetListAsync(r => r.Status == BizConstants.BoolFlag.YES);

            // 非超级管理员只能看到当前角色及其下级角色
            if (!_userContext.IsSuperAdmin)
            {
                var currentRoleId = _userContext.RoleId ?? 0;
                var descendantIds = await GetDescendantRoleIdsAsync(currentRoleId);
                roles = roles.Where(r => descendantIds.Contains(r.Id)).ToList();
            }

            var result = roles.OrderBy(r => r.SortNo).Select(r => new
            {
                r.Id,
                r.RoleCode,
                r.RoleName,
                r.ParentId
            });
            return ApiResponse.Ok(result);
        }

        public override async Task<ApiResponse> GetByIdAsync(long roleId)
        {
            var (allowed, failResponse) = await CheckRolePermission(roleId, "查看");
            if (!allowed) return failResponse!;

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                return ApiResponse.NotFound("角色不存在");

            // 获取角色关联的权限ID列表
            var permissions = await _rolePermissionRepository
                .GetListAsync(rp => rp.RoleId == roleId);
            var permissionIds = permissions.Select(rp => rp.PermissionId).ToList();

            return ApiResponse.Ok(new
            {
                role.Id,
                role.RoleCode,
                role.RoleName,
                role.ParentId,
                role.SortNo,
                role.Status,
                role.IsSystem,
                role.DataScope,
                role.Remark,
                PermissionIds = permissionIds
            });
        }

        public override async Task<ApiResponse> GetPagedListAsync(AdvancedQueryRequestDto query)
        {
            var expression = BuildQueryExpression(query);
            var filterExpression = ExpressionHelper.BuildFilter<SysRole>(query.Filters);
            var combinedExpression = expression.And(filterExpression);

            ISugarQueryable<SysRole> queryable = _db.Queryable<SysRole>().Where(combinedExpression);

            // 非超级管理员只能查看当前角色及其下级角色
            if (!_userContext.IsSuperAdmin)
            {
                var currentRoleId = _userContext.RoleId ?? 0;
                var descendantIds = await GetDescendantRoleIdsAsync(currentRoleId);
                queryable = queryable.Where(r => descendantIds.Contains(r.Id));
            }

            queryable = ApplyAdditionalQuery(queryable, query);
            queryable = ApplySorting(queryable, query.SortConditions);

            RefAsync<int> total = 0;
            var items = await queryable.ToPageListAsync(query.PageIndex, query.PageSize, total);

            items = await AfterQueryAsync(query, items);

            return ApiResponse.Ok(new { items, total = total.Value });
        }

        public override async Task<ApiResponse> DeleteAsync(long roleId)
        {
            var (allowed, failResponse) = await CheckRolePermission(roleId, "删除");
            if (!allowed) return failResponse!;

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                return ApiResponse.NotFound("角色不存在");

            if (role.IsSystem == 1)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "系统角色不允许删除");

            // 检查是否有用户关联
            var hasUsers = await _userRoleRepository.ExistsAsync(ur => ur.RoleId == roleId);
            if (hasUsers)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "该角色已分配给用户，无法删除");

            // 子角色的 ParentId 置为被删角色的父级，避免孤儿角色
            var childRoles = await _roleRepository.GetListAsync(r => r.ParentId == roleId);

            // "子角色 ParentId 改写 + 删除角色 + 删除角色权限关联"需在同一事务，
            // 否则中途失败会留下孤儿角色或残留权限关联。
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (childRoles.Count > 0)
                {
                    foreach (var child in childRoles)
                    {
                        child.ParentId = role.ParentId;
                    }
                    await _roleRepository.UpdateAsync(childRoles);
                }

                await _roleRepository.DeleteAsync(roleId);
                // 同步删除角色权限关联
                await _rolePermissionRepository.DeleteAsync(rp => rp.RoleId == roleId);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return ApiResponse.Ok(message: "删除成功");
        }

        public async Task<ApiResponse> SaveRoleAsync(SaveRoleDto dto)
        {
            // 非 SUPER_ADMIN 角色必须选择上级角色
            if (!_userContext.IsSuperAdmin && dto.ParentId == 0)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "必须选择上级角色");

            // 编码唯一性检查
            var exists = await _roleRepository.ExistsAsync(
                r => r.RoleCode == dto.RoleCode &&
                     (dto.Id == null || r.Id != dto.Id.Value));
            if (exists)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "角色编码已存在");

            if (dto.Id == null)
            {
                var entity = new SysRole
                {
                    RoleCode = dto.RoleCode,
                    RoleName = dto.RoleName,
                    ParentId = dto.ParentId,
                    SortNo = dto.SortNo,
                    Status = dto.Status,
                    DataScope = dto.DataScope,
                    Remark = dto.Remark
                };
                var id = await _roleRepository.AddAsync(entity);
                return ApiResponse.Ok(id, "新增成功");
            }
            else
            {
                var (allowed, failResponse) = await CheckRolePermission(dto.Id.Value, "编辑");
                if (!allowed) return failResponse!;

                var entity = await _roleRepository.GetByIdAsync(dto.Id.Value);
                if (entity == null)
                    return ApiResponse.NotFound("角色不存在");

                if (entity.IsSystem == 1)
                    return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "系统角色不允许编辑");

                entity.RoleCode = dto.RoleCode;
                entity.RoleName = dto.RoleName;
                entity.ParentId = dto.ParentId;
                entity.SortNo = dto.SortNo;
                entity.Status = dto.Status;
                entity.DataScope = dto.DataScope;
                entity.Remark = dto.Remark;

                await _roleRepository.UpdateAsync(entity);
                return ApiResponse.Ok(message: "更新成功");
            }
        }

        public async Task<ApiResponse> AssignUsersAsync(AssignRoleUserDto dto)
        {
            var (allowed, failResponse) = await CheckRolePermission(dto.RoleId, "分配用户");
            if (!allowed) return failResponse!;

            // "清空旧关联 + 批量新增"为重写操作，必须包裹在单一事务中，
            // 否则清空后新增失败会导致角色丢失所有用户。
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 清空旧关联
                await _userRoleRepository.DeleteAsync(ur => ur.RoleId == dto.RoleId);

                // 批量新增
                if (dto.UserIds.Count > 0)
                {
                    var entities = dto.UserIds.Select(uid => new SysUserRole
                    {
                        UserId = uid,
                        RoleId = dto.RoleId
                    }).ToList();
                    await _userRoleRepository.AddAsync(entities);
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return ApiResponse.Ok(message: "分配成功");
        }

        public async Task<ApiResponse> GetRoleUsersAsync(long roleId)
        {
            var (allowed, failResponse) = await CheckRolePermission(roleId, "查看用户");
            if (!allowed) return failResponse!;

            var userRoles = await _userRoleRepository.GetListAsync(ur => ur.RoleId == roleId);
            var userIds = userRoles.Select(ur => ur.UserId).ToList();

            return ApiResponse.Ok(new { userIds });
        }

        public async Task<List<long>> GetDescendantRoleIdsAsync(long roleId)
        {
            var allRoles = await _roleRepository.GetAllAsync();
            var roleIds = new List<long> { roleId };
            var queue = new Queue<long>();
            queue.Enqueue(roleId);

            while (queue.Count > 0)
            {
                var parentId = queue.Dequeue();
                var children = allRoles.Where(r => r.ParentId == parentId).Select(r => r.Id).ToList();
                foreach (var childId in children)
                {
                    if (!roleIds.Contains(childId))
                    {
                        roleIds.Add(childId);
                        queue.Enqueue(childId);
                    }
                }
            }

            return roleIds;
        }
    }
}
