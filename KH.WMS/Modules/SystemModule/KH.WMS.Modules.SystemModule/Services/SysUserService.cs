using System.Linq.Expressions;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Authentication;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Helpers;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Security.Encryption;
using KH.WMS.Core.Security.Hashing;
using KH.WMS.Core.Services;
using KH.WMS.Core.UserProvide;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.Extensions.Logging;
using SqlSugar;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Modules.SystemModule.Services
{
    [RegisteredService(ServiceType = typeof(ISysUserService))]
    public class SysUserService(
        IRepository<SysUser, long> repository,
        ISqlSugarClient db,
        ICacheService cacheService,
        IJwtTokenService jwtTokenService,
        IRepository<SysUserRole, long> userRoleRepository,
        IRepository<SysRole, long> roleRepository,
        ISysRoleService roleService,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        IHashService hashService,
        IRsaCryptoService rsaCryptoService) : CrudService<SysUser>(repository, unitOfWork, detailSaveService), ISysUserService
    {
        private readonly ISqlSugarClient _db = db;
        private readonly IRepository<SysUser, long> _userRepository = repository;
        private readonly ICacheService _cacheService = cacheService;
        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
        private readonly IRepository<SysUserRole, long> _userRoleRepository = userRoleRepository;
        private readonly IRepository<SysRole, long> _roleRepository = roleRepository;
        private readonly ISysRoleService _roleService = roleService;
        private readonly IUserContext _userContext = userContext;
        private readonly IHashService _hashService = hashService;
        private readonly IRsaCryptoService _rsaCryptoService = rsaCryptoService;

        /// <summary>
        /// 获取当前用户角色及其所有子角色ID集合（包含自身角色）
        /// </summary>
        private async Task<List<long>> GetCurrentUserAndDescendantRoleIds()
        {
            if (_userContext.IsSuperAdmin)
                return (await _roleRepository.GetAllAsync()).Select(r => r.Id).ToList();

            return await _roleService.GetDescendantRoleIdsAsync(_userContext.RoleId ?? 0);
        }

        private const string DEFAULT_PASSWORD_PARAM_CODE = BizConstants.ParamCodes.DEFAULT_PASSWORD;

        /// <summary>
        /// 获取系统默认密码
        /// </summary>
        private async Task<string> GetDefaultPasswordAsync()
        {
            var param = await _db.Queryable<SysParameter>()
                .Where(p => p.ParamCode == DEFAULT_PASSWORD_PARAM_CODE)
                .FirstAsync();
            return param?.ParamValue ?? BizConstants.ParamCodes.DEFAULT_PASSWORD_VALUE;
        }

        /// <summary>
        /// 获取 Token 过期时间（分钟），从系统参数缓存读取，缺省取默认值
        /// </summary>
        private int GetTokenExpireMinutes()
        {
            var value = _cacheService.Get<string>(CacheConstants.SysParam.GetKey(SysParamConstants.TOKEN_EXPIRE_MIN));
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var minutes) && minutes > 0)
                return minutes;
            return int.Parse(SysParamConstants.TOKEN_EXPIRE_MIN_DEFAULT);
        }

        /// <summary>
        /// 检查目标用户是否在当前用户的角色管辖范围内
        /// 超级管理员可以操作任何人，非超级管理员只能操作角色子树下的用户 + 当前用户本人
        /// </summary>
        private async Task<(bool allowed, ApiResponse? failResponse)> CheckUserRolePermission(long targetUserId, string operationName)
        {
            // 当前用户操作自己，始终允许
            if (_userContext.UserId == targetUserId)
                return (true, null);

            // 超级管理员可以操作所有人
            if (_userContext.IsSuperAdmin)
                return (true, null);

            var allowedRoleIds = await GetCurrentUserAndDescendantRoleIds();
            var targetUserRoles = await _userRoleRepository.GetListAsync(ur => ur.UserId == targetUserId);

            if (targetUserRoles.Count == 0)
                return (false, ApiResponse.Fail(ResponseCode.FORBIDDEN, $"无权{operationName}该用户，该用户未分配角色"));

            // 目标用户的角色是否在允许的角色范围内
            if (!targetUserRoles.Any(ur => allowedRoleIds.Contains(ur.RoleId)))
                return (false, ApiResponse.Fail(ResponseCode.FORBIDDEN, $"无权{operationName}该用户，该用户角色不在当前角色的管辖范围内"));

            // 目标用户是否拥有比当前用户更高级的角色（角色的祖先不在允许范围内）
            var targetRoleIds = targetUserRoles.Select(ur => ur.RoleId).ToList();
            var allRoles = await _roleRepository.GetAllAsync();
            foreach (var roleId in targetRoleIds)
            {
                // 向上查找该角色的所有祖先
                var current = allRoles.FirstOrDefault(r => r.Id == roleId);
                while (current != null && current.ParentId != 0)
                {
                    if (!allowedRoleIds.Contains(current.ParentId))
                        return (false, ApiResponse.Fail(ResponseCode.FORBIDDEN, $"无权{operationName}该用户，该用户角色级别高于当前角色"));
                    current = allRoles.FirstOrDefault(r => r.Id == current.ParentId);
                }
            }

            return (true, null);
        }

        public async Task<ApiResponse> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.UserName == loginDTO.UserName);
            if (user == null)
                return ApiResponse.Fail(ResponseCode.UNAUTHORIZED, "用户名或密码错误");

            // RSA 解密前端加密的密码
            string password;
            try
            {
                password = _rsaCryptoService.Decrypt(loginDTO.Password);
            }
            catch
            {
                return ApiResponse.Fail(ResponseCode.UNAUTHORIZED, "用户名或密码错误");
            }

            if (!_hashService.Verify(password, user.Password))
                return ApiResponse.Fail(ResponseCode.UNAUTHORIZED, "用户名或密码错误");

            // 账号被禁用则拒绝登录（Status: 1=启用，其它=禁用；与 CrudService.SetStatusAsync 语义一致）
            if (user.Status != 1)
                return ApiResponse.Fail(ResponseCode.UNAUTHORIZED, "该账号已被禁用，请联系管理员");

            var userRole = await _userRoleRepository.GetFirstOrDefaultAsync(u => u.UserId == user.Id);
            var roleId = userRole?.RoleId ?? 0;

            string token = _jwtTokenService.GenerateAccessToken(user.Id, user.UserName, roleId);

            // token 与用户信息缓存的过期时间与 JWT 保持一致，避免缓存永驻、单登录态混乱
            var expiration = TimeSpan.FromMinutes(GetTokenExpireMinutes());
            _cacheService.SetOrCreate(CacheConstants.Token.PREFIX + user.Id, token, expiration);
            _cacheService.SetOrCreate(CacheConstants.User.GetUserInfoKey(user.Id), user, expiration);

            return ApiResponse.Ok(new { userId = user.Id, token, roleId, userName = user.UserName, name = user.RealName }, "登录成功");
        }

        public async Task<ApiResponse> LogoutAsync()
        {
            var userId = _userContext.UserId ?? 0;
            var userIdStr = userId.ToString();

            _cacheService.Remove(CacheConstants.Token.PREFIX + userIdStr);
            _cacheService.Remove(CacheConstants.User.GetUserInfoKey(userId));
            _cacheService.Remove(CacheConstants.User.GetUserPermissionsKey(userId));
            _cacheService.Remove(CacheConstants.User.GetUserRolesKey(userId));
            _cacheService.Remove(CacheConstants.User.GetUserMenusKey(userId));

            return await Task.FromResult(ApiResponse.Ok(message: "登出成功"));
        }

        public override async Task<ApiResponse> GetPagedListAsync(AdvancedQueryRequestDto query)
        {
            Expression<Func<SysUser, bool>> expression = BuildQueryExpression(query);
            Expression<Func<SysUser, bool>> filterExpression = ExpressionHelper.BuildFilter<SysUser>(query.Filters);
            Expression<Func<SysUser, bool>> combinedExpression = expression.And(filterExpression);

            ISugarQueryable<SysUser> queryable = _db.Queryable<SysUser>().Where(combinedExpression);

            // 角色权限过滤：只能查看当前登录用户 + 当前角色及其子角色下的用户
            if (!_userContext.IsSuperAdmin)
            {
                var allowedRoleIds = await GetCurrentUserAndDescendantRoleIds();
                var currentUserId = _userContext.UserId ?? 0;

                // 先查询分页前的总数和结果，再过滤角色
                // 使用子查询：用户ID在允许角色关联中 或 是当前用户本人
                queryable = queryable.Where(u =>
                    u.Id == currentUserId ||
                    SqlFunc.Subqueryable<SysUserRole>()
                        .Where(ur => ur.UserId == u.Id && allowedRoleIds.Contains(ur.RoleId))
                        .Any()
                );
            }

            queryable = ApplyAdditionalQuery(queryable, query);
            queryable = ApplySorting(queryable, query.SortConditions);

            int total = await queryable.CountAsync();

            List<SysUser> items = await queryable.ToPageListAsync(query.PageIndex, query.PageSize);

            var userIds = items.Select(u => u.Id).ToList();

            List<SysUserRole> userRoles = items.Count > 0
                ? await _userRoleRepository.GetListAsync(ur => userIds.Contains(ur.UserId))
                : new List<SysUserRole>();

            List<SysRole> allRoles = await _roleRepository.GetAllAsync();
            Dictionary<long, SysRole> roleDict = allRoles.ToDictionary(r => r.Id);

            object result = new
            {
                items = items.Select(u =>
                {
                    var roles = userRoles.Where(ur => ur.UserId == u.Id).ToList();
                    return new UserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        RealName = u.RealName,
                        Avatar = u.Avatar,
                        DepartmentId = u.DepartmentId,
                        Status = u.Status,
                        IsSystem = u.IsSystem,
                        LoginIp = u.LoginIp,
                        LoginTime = u.LoginTime,
                        Remark = u.Remark,
                        RoleIds = roles.Select(r => r.RoleId).ToList(),
                        RoleNames = roles.Select(r => roleDict.GetValueOrDefault(r.RoleId)?.RoleName ?? "").ToList()
                    };
                }),
                total
            };
            return ApiResponse.Ok(result);
        }

        public async Task<ApiResponse> GetUserDetailAsync(long userId)
        {
            var (allowed, failResponse) = await CheckUserRolePermission(userId, "查看");
            if (!allowed) return failResponse!;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse.NotFound("用户不存在");

            var userRoles = await _userRoleRepository.GetListAsync(ur => ur.UserId == userId);
            var allRoles = await _roleRepository.GetAllAsync();
            var roleDict = allRoles.ToDictionary(r => r.Id);

            var dto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                RealName = user.RealName,
                Avatar = user.Avatar,
                DepartmentId = user.DepartmentId,
                Status = user.Status,
                IsSystem = user.IsSystem,
                LoginIp = user.LoginIp,
                LoginTime = user.LoginTime,
                Remark = user.Remark,
                RoleIds = userRoles.Select(r => r.RoleId).ToList(),
                RoleNames = userRoles.Select(r => roleDict.GetValueOrDefault(r.RoleId)?.RoleName ?? "").ToList()
            };
            return ApiResponse.Ok(dto);
        }

        public async Task<ApiResponse> SaveUserAsync(SaveUserDto dto)
        {
            if (dto.Id == null)
            {
                // 新增
                var exists = await _userRepository.ExistsAsync(u => u.UserName == dto.UserName);
                if (exists)
                    return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "用户名已存在");

                var defaultPassword = await GetDefaultPasswordAsync();
                var entity = new SysUser
                {
                    UserName = dto.UserName,
                    Password = _hashService.Hash(dto.Password ?? defaultPassword),
                    RealName = dto.RealName,
                    Avatar = dto.Avatar,
                    DepartmentId = dto.DepartmentId,
                    Status = dto.Status,
                    Remark = dto.Remark
                };
                var id = await _userRepository.AddAsync(entity);
                return ApiResponse.Ok(id, "新增成功");
            }
            else
            {
                // 编辑
                var (allowed, failResponse) = await CheckUserRolePermission(dto.Id.Value, "编辑");
                if (!allowed) return failResponse!;

                var entity = await _userRepository.GetByIdAsync(dto.Id.Value);
                if (entity == null)
                    return ApiResponse.NotFound("用户不存在");

                // 用户名不允许修改
                // entity.UserName = dto.UserName;
                if (!string.IsNullOrWhiteSpace(dto.Password))
                    entity.Password = _hashService.Hash(dto.Password);
                entity.RealName = dto.RealName;
                entity.Avatar = dto.Avatar;
                entity.DepartmentId = dto.DepartmentId;
                entity.Status = dto.Status;
                entity.Remark = dto.Remark;

                await _userRepository.UpdateAsync(entity);
                _cacheService.Remove(CacheConstants.User.GetUserInfoKey(entity.Id));
                return ApiResponse.Ok(message: "更新成功");
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(long userId)
        {
            if (_userContext.UserId == userId)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "不允许删除当前登录用户");

            var (allowed, failResponse) = await CheckUserRolePermission(userId, "删除");
            if (!allowed) return failResponse!;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse.NotFound("用户不存在");

            if (user.IsSystem == 1)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "系统用户不允许删除");

            // 删除用户与角色关联需在同一事务，避免只删其一
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _userRepository.DeleteAsync(userId);
                await _userRoleRepository.DeleteAsync(ur => ur.UserId == userId);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            _cacheService.Remove(CacheConstants.User.GetUserInfoKey(userId));

            return ApiResponse.Ok(message: "删除成功");
        }

        public async Task<ApiResponse> AssignRolesAsync(AssignUserRoleDto dto)
        {
            var (allowed, failResponse) = await CheckUserRolePermission(dto.UserId, "分配角色");
            if (!allowed) return failResponse!;

            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                return ApiResponse.NotFound("用户不存在");

            // "清空旧关联 + 批量新增"为重写操作，必须包裹在单一事务中，
            // 否则清空后新增失败会导致用户丢失所有角色。
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 清空旧关联
                await _userRoleRepository.DeleteAsync(ur => ur.UserId == dto.UserId);

                // 批量新增
                if (dto.RoleIds.Count > 0)
                {
                    var entities = dto.RoleIds.Select(rid => new SysUserRole
                    {
                        UserId = dto.UserId,
                        RoleId = rid
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

        public async Task<ApiResponse> ResetPasswordAsync(long userId)
        {
            if (_userContext.UserId == userId)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "不允许重置当前登录用户的密码");

            var (allowed, failResponse) = await CheckUserRolePermission(userId, "重置密码");
            if (!allowed) return failResponse!;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse.NotFound("用户不存在");

            var defaultPassword = await GetDefaultPasswordAsync();
            user.Password = _hashService.Hash(defaultPassword);
            user.PasswordUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);

            return ApiResponse.Ok(message: $"密码已重置为 {defaultPassword}");
        }

        public async Task<ApiResponse> ToggleStatusAsync(long userId)
        {
            var (allowed, failResponse) = await CheckUserRolePermission(userId, "切换状态");
            if (!allowed) return failResponse!;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse.NotFound("用户不存在");

            if (user.IsSystem == 1)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "系统用户不允许禁用");

            user.Status = user.Status == BizConstants.BoolFlag.YES ? BizConstants.BoolFlag.NO : BizConstants.BoolFlag.YES;
            await _userRepository.UpdateAsync(user);

            return ApiResponse.Ok(message: user.Status == BizConstants.BoolFlag.YES ? "已启用" : "已禁用");
        }
    }
}
