using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Queries.CheckMapOwnership;

public record CheckMapOwnershipQuery(Guid MapId) : IRequest<Result<CheckMapOwnershipDto>>;

public class CheckMapOwnershipDto
{
    /// <summary>Map có tồn tại và active.</summary>
    public bool MapExists { get; set; }
    /// <summary>User hiện tại có sở hữu map (tự tạo hoặc đã mua).</summary>
    public bool IsOwned { get; set; }
    /// <summary>True nếu user là tác giả (CreatedBy); false nếu chỉ mua.</summary>
    public bool IsAuthor { get; set; }
}
