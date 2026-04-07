using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints.Commands.DeleteComplaintPolicyRuleConfig;

public class DeleteComplaintPolicyRuleConfigCommandHandler : IRequestHandler<DeleteComplaintPolicyRuleConfigCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteComplaintPolicyRuleConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteComplaintPolicyRuleConfigCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("Chỉ Quản trị viên/Người điều hành mới có thể xóa cấu hình quy tắc chính sách khiếu nại.", ErrorCodeEnum.Forbidden);

        if (string.IsNullOrWhiteSpace(request.CategoryKey) || string.IsNullOrWhiteSpace(request.RuleKey))
            return Result.Failure("CategoryKey và RuleKey là bắt buộc.", ErrorCodeEnum.ValidationFailed);

        var row = await _unitOfWork.Repository<ComplaintPolicyRuleConfig>().GetQueryable()
            .FirstOrDefaultAsync(x => !x.IsDeleted
                                      && x.CategoryKey == request.CategoryKey.Trim()
                                      && x.RuleKey == request.RuleKey.Trim(),
                cancellationToken);
        if (row == null)
            return Result.Failure("Không tìm thấy cấu hình quy tắc chính sách khiếu nại.", ErrorCodeEnum.NotFound);

        row.SoftDeleteEntity(userIdNullable.Value);
        row.UpdateEntity(userIdNullable.Value);
        _unitOfWork.Repository<ComplaintPolicyRuleConfig>().Update(row);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Đã xóa cấu hình quy tắc chính sách khiếu nại.");
    }
}
