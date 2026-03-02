using AutoMapper;
using CapstoneProject.Application.Commons.DTOs.User;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Commons.Mappings.Resolvers;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Commons.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // AppUser -> UserListItem
        CreateMap<AppUser, UserListItem>()
            .ForMember(dest => dest.AvatarPath, opt => opt.MapFrom<AvatarUrlResolver>())
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles will be populated separately

        // AppUser -> UserResponse
        CreateMap<AppUser, UserResponse>()
            .IncludeBase<AppUser, UserListItem>();

        // CreateUserRequest -> AppUser (audit set in handler via InitializeEntity; Status defaulted in handler)
        CreateMap<CreateUserRequest, AppUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.JoiningAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.AvatarPath, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore()) // set in handler: request.Status ?? Active
            .IgnoreIdentityFields()
            .IgnoreAuditFields();

        // UpdateUserRequest -> AppUser (audit set in handler via UpdateEntity, never from request)
        CreateMap<UpdateUserRequest, AppUser>()
            .ForMember(dest => dest.Email, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Email)))
            .ForMember(dest => dest.UserName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Email)))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.AvatarPath, opt => opt.Ignore())
            .IgnoreIdentityFields()
            .IgnoreAuditFields();
    }
}
