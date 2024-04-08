using MediatR;

using Microsoft.EntityFrameworkCore;

using YourBrand.Identity;
using YourBrand.IdentityManagement.Application.Common.Interfaces;
using YourBrand.IdentityManagement.Contracts;

namespace YourBrand.IdentityManagement.Application.Users.Commands;

public record SyncDataCommand() : IRequest
{
    public class SyncDataCommandHandler : IRequestHandler<SyncDataCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUserContext _userContext;

        public SyncDataCommandHandler(IApplicationDbContext context, IEventPublisher eventPublisher, IUserContext userContext)
        {
            _context = context;
            _eventPublisher = eventPublisher;
            _userContext = userContext;
        }

        public async Task Handle(SyncDataCommand request, CancellationToken cancellationToken)
        {
            var organizations = await _context.Organizations
                .Include(p => p.Tenant)
                .OrderBy(p => p.Created)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            foreach (var organization in organizations)
            {
                await _eventPublisher.PublishEvent(new OrganizationCreated(organization.Id, organization.Tenant.Id, organization.Name));
            }

            var users = await _context.Users
                .Include(p => p.Tenant)
                .Include(p => p.Organizations)
                .OrderBy(p => p.Created)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                await _eventPublisher.PublishEvent(new UserCreated(user.Id, user.Tenant!.Id, user.Organization!.Id));
            }

            var organizationUsers = await _context.OrganizationUsers
                //.OrderBy(p => p.Created)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            foreach (var organizationUser in organizationUsers)
            {
                await _eventPublisher.PublishEvent(new OrganizationUserAdded(organizationUser.TenantId, organizationUser.OrganizationId, organizationUser.UserId));
            }
        }
    }
}