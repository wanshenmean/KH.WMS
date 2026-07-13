using AutoMapper;

namespace KH.WMS.Server.Profiles;

/// <summary>
/// 实体映射配置
/// </summary>
public class EntityProfile : Profile
{
    public EntityProfile()
    {
        // 这里配置实体之间的映射关系
        // 例如：
        // CreateMap<User, UserDto>();
        // CreateMap<UserDto, User>();

        // 反向映射
        // CreateMap<User, UserDto>().ReverseMap();

        // 自定义映射
        // CreateMap<User, UserDto>()
        //     .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        // 忽略某些属性
        // CreateMap<User, UserDto>()
        //     .ForMember(dest => dest.Password, opt => opt.Ignore());
    }
}
