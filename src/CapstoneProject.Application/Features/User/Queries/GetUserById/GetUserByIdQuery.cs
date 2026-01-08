using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.User;
using MediatR;

namespace CapstoneProject.Application.Features.User.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserResponse>>;
