using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.AddMapToMyMaps;

/// <summary>
/// Thêm map free vào bộ sưu tập của user (bảng MyMap). Chỉ áp dụng cho map có giá = 0 hoặc null và đã published.
/// </summary>
public record AddMapToMyMapsCommand(Guid MapId) : IRequest<Result>;
