using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.AddMapToMyGames;

/// <summary>
/// Thêm game free vào bộ sưu tập của user (bảng MyGame). Chỉ áp dụng cho game có giá = 0 hoặc null và đã published.
/// </summary>
public record AddMapToMyGamesCommand(Guid GameId) : IRequest<Result>;
