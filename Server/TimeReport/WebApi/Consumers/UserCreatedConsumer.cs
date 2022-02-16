﻿using System;

using MassTransit;

using MediatR;

using Skynet.IdentityService.Client;
using Skynet.IdentityService.Contracts;
using Skynet.TimeReport.Application;
using Skynet.TimeReport.Application.Common.Interfaces;
using Skynet.TimeReport.Application.Users.Commands;

namespace Skynet.TimeReport.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreated>
{
    private readonly IMediator _mediator;
    private readonly IUsersClient _usersClient;

    public UserCreatedConsumer(IMediator mediator, IUsersClient usersClient)
    {
        _mediator = mediator;
        _usersClient = usersClient;
    }

    public async Task Consume(ConsumeContext<UserCreated> context)
    {
        var message = context.Message;

        var userResponse = await _usersClient.GetUserAsync(message.UserId);

        var result = await _mediator.Send(new CreateUserCommand(userResponse.Id, userResponse.FirstName, userResponse.LastName, userResponse.DisplayName, userResponse.Ssn, userResponse.Email));
    }
}