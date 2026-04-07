using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.AddMapGalleryMedia;

public class AddMapGalleryMediaCommandHandler : IRequestHandler<AddMapGalleryMediaCommand, Result<List<MapMediaItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICloudinaryService _cloudinaryService;

    public AddMapGalleryMediaCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<List<MapMediaItemDto>>> Handle(AddMapGalleryMediaCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<List<MapMediaItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result<List<MapMediaItemDto>>.Failure("Bản đồ không được tìm thấy.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (map.CreatedBy != userId && !isAdminOrMod)
            return Result<List<MapMediaItemDto>>.Failure("Bạn không có quyền cập nhật bản đồ này.", ErrorCodeEnum.Forbidden);

        var staged = await MapGalleryMediaHelper.StageGalleryMediaAsync(
            request.MapId,
            userId.Value,
            request.Files,
            _unitOfWork,
            _cloudinaryService,
            requireAtLeastOneFile: true,
            cancellationToken);
        if (!staged.IsSuccess)
            return staged;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<List<MapMediaItemDto>>.Success(staged.Data ?? new List<MapMediaItemDto>(), "Gallery media added.");
    }
}
