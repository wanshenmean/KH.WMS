using AutoMapper;

namespace KH.WMS.Server.Profiles;

/// <summary>
/// 视图模型映射配置
/// </summary>
public class ViewModelProfile : Profile
{
    public ViewModelProfile()
    {
        // 这里配置 ViewModel 的映射关系
        // 例如：
        // CreateMap<User, UserViewModel>();
        // CreateMap<UserViewModel, User>();

        // 集合映射
        // CreateMap<List<User>, List<UserViewModel>>();

        // 嵌套对象映射
        // CreateMap<Order, OrderViewModel>()
        //     .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
        //     .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}
