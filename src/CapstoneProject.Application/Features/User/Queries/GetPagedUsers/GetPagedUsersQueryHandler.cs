using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MediatR;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.User;
using CapstoneProject.Application.Commons.QueryBuilders;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.User.Queries.GetPagedUsers;

public class GetPagedUsersQueryHandler : IRequestHandler<GetPagedUsersQuery, PaginationResult<UserListItem>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public GetPagedUsersQueryHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<PaginationResult<UserListItem>> Handle(GetPagedUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();

            // Get paged users
            var (users, totalCount) = await _unitOfWork.Repository<AppUser>().GetPagedAsync(
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                predicate: predicate,
                orderBy: orderBy,
                isAscending: request.Filter.IsAscending ?? false
            );

            var userListItems = _mapper.Map<List<UserListItem>>(users);

            // Get roles for each user (if filter by role is specified, filter in-memory)
            foreach (var userItem in userListItems)
            {
                var user = users.FirstOrDefault(u => u.Id == userItem.Id);
                if (user != null)
                {
                    var rolesResult = await _identityService.GetUserRolesAsync(user);
                    if (rolesResult.IsSuccess && rolesResult.Data != null)
                    {
                        userItem.Roles = rolesResult.Data.Select(r => Enum.Parse<RoleEnum>(r)).ToList();
                    }
                }
            }

            // Filter by role if specified
            if (!string.IsNullOrEmpty(request.Filter.Role))
            {
                userListItems = userListItems
                    .Where(x => x.Roles.Contains(Enum.Parse<RoleEnum>(request.Filter.Role)))
                    .ToList();
                totalCount = userListItems.Count;
            }

            return PaginationResult<UserListItem>.Success(
                items: userListItems,
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                totalItems: totalCount
            );
        }
        catch
        {
            throw;
        }
    }
}
