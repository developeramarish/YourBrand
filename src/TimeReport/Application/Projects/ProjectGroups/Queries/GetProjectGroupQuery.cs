﻿
using MediatR;

using Microsoft.EntityFrameworkCore;

using YourBrand.TimeReport.Application.Common.Interfaces;

namespace YourBrand.TimeReport.Application.Projects.ProjectGroups.Queries;

public record GetProjectGroupQuery(string ExpenseId) : IRequest<ProjectGroupDto>
{
    public class GetExpenseQueryHandler(ITimeReportContext context) : IRequestHandler<GetProjectGroupQuery, ProjectGroupDto>
    {
        public async Task<ProjectGroupDto> Handle(GetProjectGroupQuery request, CancellationToken cancellationToken)
        {
            var projectGroup = await context.ProjectGroups
               .Include(x => x.Project)
               .AsNoTracking()
               .AsSplitQuery()
               .FirstOrDefaultAsync(x => x.Id == request.ExpenseId, cancellationToken);

            if (projectGroup is null)
            {
                throw new Exception();
            }

            return projectGroup.ToDto();
        }
    }
}