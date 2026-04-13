using CapstoneProject.Application.Commons.Models.Complaints;

namespace CapstoneProject.Application.Common.Interfaces;

public interface IComplaintPolicyService
{
    Task<ComplaintCreatePolicyResult> ValidateCreateAsync(ComplaintCreatePolicyInput input, CancellationToken cancellationToken);

    Task<ComplaintRefundPolicyResult> ValidateRefundAsync(ComplaintRefundPolicyInput input, CancellationToken cancellationToken);
}
