using Ardalis.Result;
using BlazorApp01.DataAccess.Repositories;
using BlazorApp01.Domain.Models;
using BlazorApp01.Features.CQRS.MediatorFacade.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp01.Features.CQRS.Requests.ApplicationUsers.Queries;

public sealed record GetApplicationUserByIdQuery(string UserId) : IQuery<ApplicationUser?>;

internal sealed class GetApplicationUserByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetApplicationUserByIdQuery, ApplicationUser?>
{
    public async ValueTask<Result<ApplicationUser?>> Handle(GetApplicationUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<ApplicationUser>()
            .QueryAsNoTracking()
            .FirstOrDefaultAsync(applicationUser => applicationUser.Id == request.UserId, cancellationToken);
    }
}