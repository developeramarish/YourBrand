using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using YourBrand.TimeReport.Application.Activities;
using YourBrand.TimeReport.Application.Projects.Expenses;
using YourBrand.TimeReport.Application.Projects;
using YourBrand.TimeReport.Application.TimeSheets;
using YourBrand.TimeReport.Application.Users;
using YourBrand.TimeReport.Application.Users.Absence;
using YourBrand.TimeReport.Domain.Entities;
using YourBrand.TimeReport.Application.Activities.ActivityTypes;
using YourBrand.TimeReport.Application.Projects.Expenses.ExpenseTypes;
using YourBrand.TimeReport.Application.Projects.ProjectGroups;

namespace YourBrand.TimeReport.Application;

public static class Mapper
{
    public static UserDto ToDto(this Domain.Entities.User user)
    {
        return new (user.Id, user.FirstName, user.LastName, user.DisplayName, user.SSN, user.Email, user.Created, user.Deleted);
    }

    public static ProjectDto ToDto(this Domain.Entities.Project project)
    {
        return new (project.Id, project.Name, project.Description);
    }

    public static ProjectGroupDto ToDto(this Domain.Entities.ProjectGroup projectGroup)
    {
        return new (projectGroup.Id, projectGroup.Name, projectGroup.Description, projectGroup.Project?.ToDto());
    }

    public static ActivityDto ToDto(this Domain.Entities.Activity activity)
    {
        return new (activity.Id, activity.Name, activity.ActivityType.ToDto(), activity.Description, activity.HourlyRate, activity.Project.ToDto());
    }

    public static ActivityTypeDto ToDto(this Domain.Entities.ActivityType activityType)
    {
        return new (activityType.Id, activityType.Name, activityType.Description, activityType.ExcludeHours, activityType.Project?.ToDto());
    }

    public static ExpenseDto ToDto(this Domain.Entities.Expense expense)
    {
        return new (expense.Id, expense.Date.ToDateTime(TimeOnly.Parse("01:00")), expense.Amount, expense.Description, expense.Attachment, expense.Project.ToDto());
    }

    public static ExpenseTypeDto ToDto(this Domain.Entities.ExpenseType expenseType)
    {
        return new (expenseType.Id, expenseType.Name, expenseType.Description, expenseType.Project?.ToDto());
    }

    public static TimeSheetDto ToDto(this Domain.Entities.TimeSheet timeSheet, IEnumerable<MonthEntryGroup> monthInfo)
    {
        var activities = timeSheet.Activities
            .OrderBy(e => e.Created)
            .Select(a => a.ToDto())
            .ToArray();

        return new (timeSheet.Id, timeSheet.Year, timeSheet.Week, timeSheet.From, timeSheet.To, (TimeSheetStatusDto)timeSheet.Status, timeSheet.User.ToDto(),
                activities, monthInfo.Select(x => new MonthInfoDto(x.Month, x.Status == EntryStatus.Locked)));
    }

    public static TimeSheetActivityDto ToDto(this Domain.Entities.TimeSheetActivity activity)
    {
        return new (activity.Activity.Id, activity.Activity.Name, activity.Activity.Description, activity.Project.ToDto(),
                    activity.Entries.OrderBy(e => e.Date).Select(e => e.ToTimeSheetEntryDto()));
    }

    public static TimeSheetEntryDto ToTimeSheetEntryDto(this Domain.Entities.Entry entry)
    {
        return new (entry.Id, entry.Date.ToDateTime(TimeOnly.Parse("01:00")), entry.Hours, entry.Description, (EntryStatusDto)entry.MonthGroup.Status);
    }

    public static EntryDto ToDto(this Domain.Entities.Entry entry)
    {
        return new (entry.Id, new ProjectDto(entry.Project.Id, entry.Project.Name, entry.Project.Description), new ActivityDto(entry.Activity.Id, entry.Activity.Name, entry.Activity.ActivityType.ToDto(), entry.Activity.Description, entry.Activity.HourlyRate, new ProjectDto(entry.Activity.Project.Id, entry.Activity.Project.Name, entry.Activity.Project.Description)), entry.Date.ToDateTime(TimeOnly.Parse("01:00")), entry.Hours, entry.Description, (EntryStatusDto)entry.MonthGroup.Status);
    }

    public static AbsenceDto ToDto(this Domain.Entities.Absence absence)
    {
        return new (absence.Id, absence.Date.GetValueOrDefault().ToDateTime(TimeOnly.Parse("00:01")), absence.Note, new ProjectDto(absence.Project.Id, absence.Project.Name, absence.Project.Description));
    }
}