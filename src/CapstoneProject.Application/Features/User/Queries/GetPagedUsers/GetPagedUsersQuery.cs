using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.User;
using MediatR;

namespace CapstoneProject.Application.Features.User.Queries.GetPagedUsers;

public record GetPagedUsersQuery(UserFilter Filter) : IRequest<PaginationResult<UserListItem>>;
