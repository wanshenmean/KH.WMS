using AutoMapper;

namespace KH.WMS.Server.Profiles;

/// <summary>
/// DTO 映射配置
/// </summary>
public class DtoProfile : Profile
{
    public DtoProfile()
    {
        // 这里配置 DTO 之间的映射关系
        // 例如：
        // CreateMap<CreateUserRequest, UserDto>();
        // CreateMap<UpdateUserRequest, UserDto>();

        // 带条件映射
        // CreateMap<UserDto, User>()
        //     .ForMember(dest => dest.UpdatedAt, opt => opt.Condition(src => src.UpdatedAt != default));

        // 类型转换
        // CreateMap<string, DateTime>().ConvertUsing(s => DateTime.Parse(s));
        // CreateMap<DateTime, string>().ConvertUsing(d => d.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}
