using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.User;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.User.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Repository<AppUser>()
            .GetFirstOrDefaultAsync(x => x.Id == request.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException("Không tìm thấy người dùng");
            }

            var userResponse = _mapper.Map<UserResponse>(user);

            // Get user roles
            var rolesResult = await _identityService.GetUserRolesAsync(user);
            if (rolesResult.IsSuccess && rolesResult.Data != null)
            {
                userResponse.Roles = rolesResult.Data.Select(r => Enum.Parse<RoleEnum>(r)).ToList();
            }

            return Result<UserResponse>.Success(userResponse, "Nhận người dùng thành công");
        }
        catch
        {
            throw;
        }
    }
}
