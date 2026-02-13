using System.Linq.Expressions;
using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.ApplicationUsers.Queries;

public sealed record GetApplicationUsersPagedQuery(
    string? SearchFilter,
    int StartIndex,
    int Count,
    string? SortBy,
    bool SortAscending
) : IQuery<GetApplicationUsersPagedResponse>;

public sealed record GetApplicationUsersPagedResponse(
    ICollection<ApplicationUser> Items,
    int TotalCount
);

internal sealed class GetApplicationUsersPagedQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetApplicationUsersPagedQuery, GetApplicationUsersPagedResponse>
{
    public async ValueTask<Result<GetApplicationUsersPagedResponse>> Handle(GetApplicationUsersPagedQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = unitOfWork.Repository<ApplicationUser>().QueryAsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchFilter))
        {
            dbQuery = dbQuery.Where(user =>
                (user.Email != null && user.Email.Contains(query.SearchFilter)) ||
                (user.UserName != null && user.UserName.Contains(query.SearchFilter)));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var sortBy = string.IsNullOrWhiteSpace(query.SortBy)
            ? "UserName"
            : query.SortBy;

        dbQuery = ApplySorting(dbQuery, sortBy, query.SortAscending);

        var items = await dbQuery
            .Skip(query.StartIndex)
            .Take(query.Count)
            .ToListAsync(cancellationToken);

        return new GetApplicationUsersPagedResponse(items, totalCount);
    }

    private static IQueryable<ApplicationUser> ApplySorting(IQueryable<ApplicationUser> query, string sortBy, bool ascending)
    {
        Expression<Func<ApplicationUser, object>> keySelector = sortBy.ToLowerInvariant() switch
        {
            "email" => x => x.Email ?? string.Empty,
            "emailconfirmed" => x => x.EmailConfirmed,
            "phonenumber" => x => x.PhoneNumber ?? string.Empty,
            "phonenumberconfirmed" => x => x.PhoneNumberConfirmed,
            "twofactorenabled" => x => x.TwoFactorEnabled,
            "lockoutenabled" => x => x.LockoutEnabled,
            "lockoutend" => x => x.LockoutEnd ?? DateTimeOffset.MinValue,
            "accessfailedcount" => x => x.AccessFailedCount,
            _ => x => x.UserName ?? string.Empty
        };

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }
}