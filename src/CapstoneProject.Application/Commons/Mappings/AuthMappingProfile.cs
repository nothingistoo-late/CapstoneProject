using AutoMapper;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Commons.Mappings.Resolvers;
using CapstoneProject.Application.Features.Auth.Queries.GetProfile;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Common.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<RegisterRequest, AppUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EntityStatusEnum.Active))
            .ForMember(dest => dest.JoiningAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.AvatarPath, opt => opt.Ignore())
            .IgnoreIdentityFields()
            .IgnoreAuditFields();

        CreateMap<AppUser, ProfileResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AvatarPath, opt => opt.MapFrom<AvatarUrlResolver>())
            .ForMember(dest => dest.Gender, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
            .ForMember(dest => dest.HireDate, opt => opt.Ignore())
            .ForMember(dest => dest.Salary, opt => opt.Ignore())
            .ForMember(dest => dest.LearnerCode, opt => opt.Ignore())
            .ForMember(dest => dest.TeacherCode, opt => opt.Ignore());
    }
}