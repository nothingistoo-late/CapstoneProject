using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Auth.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<ProfileResponse>>
{
    private readonly ILogger<GetProfileQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetProfileQueryHandler(ILogger<GetProfileQueryHandler> logger, IMapper mapper, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProfileResponse>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId, roles) = await _currentUserService.ValidateUserWithRolesAsync();
            if (!isValid || userId == null)
            {
                return Result<ProfileResponse>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);
            }

            var user = await _unitOfWork.Repository<AppUser>().GetFirstOrDefaultAsync(
                u => u.Id == userId);
            
            return Result<ProfileResponse>.Success(_mapper.Map<ProfileResponse>(user), "Hồ sơ được truy xuất thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting profile for user {UserId}", _currentUserService.UserId);
            return Result<ProfileResponse>.Failure("Đã xảy ra lỗi khi truy xuất hồ sơ", ErrorCodeEnum.InternalError);
        }
    }
}