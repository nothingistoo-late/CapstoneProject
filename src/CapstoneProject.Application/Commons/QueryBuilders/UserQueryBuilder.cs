using System.Linq.Expressions;
using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Commons.DTOs.User;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Commons.QueryBuilders;

public static class UserQueryBuilder
{
    public static Expression<Func<AppUser, bool>> BuildPredicate(this UserFilter filter)
    {
        var predicate = PredicateBuilder.True<AppUser>();
        
        if (filter.Status.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
        }
        
        if (filter.JoiningFrom.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.JoiningAt >= filter.JoiningFrom.Value);
        }

        if (filter.JoiningTo.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.JoiningAt <= filter.JoiningTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPredicate = PredicateBuilder.False<AppUser>();
            searchPredicate = searchPredicate.CombineOr(x => x.FirstName.Contains(filter.Search));
            searchPredicate = searchPredicate.CombineOr(x => x.LastName.Contains(filter.Search));
            searchPredicate = searchPredicate.CombineOr(x => x.Email != null && x.Email.Contains(filter.Search));
            searchPredicate = searchPredicate.CombineOr(x => x.PhoneNumber != null && x.PhoneNumber.Contains(filter.Search));
            predicate = predicate.CombineAnd(searchPredicate);
        }
        
        return predicate;
    }

    public static Expression<Func<AppUser, object>> BuildOrderBy(this UserFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy))
        {
            return x => x.CreatedAt!;
        }
        
        return filter.SortBy.ToLowerInvariant() switch
        {
            "firstname" => x => x.FirstName,
            "lastname" => x => x.LastName,
            "email" => x => x.Email!,
            "joiningat" => x => x.JoiningAt,
            "lastloginat" => x => x.LastLoginAt!,
            "createdat" => x => x.CreatedAt!,
            "updatedat" => x => x.UpdatedAt!,
            _ => x => x.CreatedAt!
        };
    }
}
