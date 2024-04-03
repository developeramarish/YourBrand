﻿using MassTransit;

using MediatR;

using YourBrand.Identity;
using YourBrand.IdentityManagement.Contracts;
using YourBrand.Showroom.Application.Users.Commands;
using YourBrand.Tenancy;
namespace YourBrand.Showroom.Consumers;

public class ShowroomUserCreatedConsumer(IMediator mediator, ITenantService tenantService, ICurrentUserService currentUserService, IRequestClient<GetUser> requestClient, ILogger<ShowroomUserCreatedConsumer> logger) : IConsumer<UserCreated>
{
    public async Task Consume(ConsumeContext<UserCreated> context)
    {
        var message = context.Message;

        tenantService.SetTenantId(message.TenantId);
        currentUserService.SetCurrentUser(message.CreatedById);

        var messageR = await requestClient.GetResponse<GetUserResponse>(new GetUser(message.UserId, (message.CreatedById)));
        var message2 = messageR.Message;

        var result = await mediator.Send(new CreateUserCommand(message2.UserId, message2.FirstName, message2.LastName, message2.DisplayName, "SSN", message2.Email));
    }
}

public class ShowroomUserDeletedConsumer(IMediator mediator, ITenantService tenantService, ICurrentUserService currentUserService) : IConsumer<UserDeleted>
{
    public async Task Consume(ConsumeContext<UserDeleted> context)
    {
        var message = context.Message;

        //tenantService.SetTenantId(message.TenantId);
        currentUserService.SetCurrentUser(message.DeletedById);

        await mediator.Send(new DeleteUserCommand(message.UserId));
    }
}

public class ShowroomUserUpdatedConsumer(IMediator mediator, IRequestClient<GetUser> requestClient, ITenantService tenantService, ICurrentUserService currentUserService) : IConsumer<UserUpdated>
{
    public async Task Consume(ConsumeContext<UserUpdated> context)
    {
        var message = context.Message;

        currentUserService.SetCurrentUser(message.UpdatedById);

        var messageR = await requestClient.GetResponse<GetUserResponse>(new GetUser(message.UserId, message.UpdatedById));
        var message2 = messageR.Message;

        tenantService.SetTenantId(message2.TenantId);

        var result = await mediator.Send(new UpdateUserCommand(message2.UserId, message2.FirstName, message2.LastName, message2.DisplayName, "SSN", message2.Email));
    }
}