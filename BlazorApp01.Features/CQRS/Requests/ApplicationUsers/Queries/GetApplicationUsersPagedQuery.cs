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
    public async ValueTask<Result<GetApplicationUsersPagedResponse>> Handle(GetApplicationUsersPagedQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.QueryRepository<ApplicationUser>().QueryAsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchFilter))
        {
            query = query.Where(applicationUser =>
                (applicationUser.Email != null && applicationUser.Email.Contains(request.SearchFilter)) ||
                (applicationUser.UserName != null && applicationUser.UserName.Contains(request.SearchFilter)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy)
            ? "UserName"
            : request.SortBy;

        query = ApplySorting(query, sortBy, request.SortAscending);

        var applicationUsers = await query
            .Skip(request.StartIndex)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        return new GetApplicationUsersPagedResponse(applicationUsers, totalCount);
    }

    private static IQueryable<ApplicationUser> ApplySorting(IQueryable<ApplicationUser> query, string sortBy, bool ascending)
    {
        Expression<Func<ApplicationUser, object>> keySelector = sortBy.ToLowerInvariant() switch
        {
            "email" => applicationUser => applicationUser.Email ?? string.Empty,
            "emailconfirmed" => applicationUser => applicationUser.EmailConfirmed,
            "phonenumber" => applicationUser => applicationUser.PhoneNumber ?? string.Empty,
            "phonenumberconfirmed" => applicationUser => applicationUser.PhoneNumberConfirmed,
            "twofactorenabled" => applicationUser => applicationUser.TwoFactorEnabled,
            "lockoutenabled" => applicationUser => applicationUser.LockoutEnabled,
            "lockoutend" => applicationUser => applicationUser.LockoutEnd ?? DateTimeOffset.MinValue,
            "accessfailedcount" => applicationUser => applicationUser.AccessFailedCount,
            _ => applicationUser => applicationUser.UserName ?? string.Empty
        };

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }
}