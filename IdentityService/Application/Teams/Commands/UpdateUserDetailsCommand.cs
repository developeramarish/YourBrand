﻿using MediatR;

using Microsoft.EntityFrameworkCore;

using YourBrand.IdentityService.Application.Common.Interfaces;
using YourBrand.IdentityService.Contracts;
using YourBrand.IdentityService.Domain.Exceptions;

namespace YourBrand.IdentityService.Application.Teams.Commands;

public record UpdateTeamCommand(string TeamId, string Name, string Description) : IRequest<TeamDto>
{
    public class Handler : IRequestHandler<UpdateTeamCommand, TeamDto>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TeamDto> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
        {
            var team = await _context.Teams
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == request.TeamId, cancellationToken);

            if (team is null)
            {
                throw new UserNotFoundException(request.TeamId);
            }

            team.UpdateName(request.Name);
            team.UpdateDescription(request.Description);

            await _context.SaveChangesAsync(cancellationToken);

            return team.ToDto();
        }
    }
}
