﻿using Microsoft.Extensions.DependencyInjection;
using Skynet.IdentityService.Client;

const string ApiKey = "foobar";

var services = BuildServiceProvider();

var usersClient = services.GetRequiredService<IUsersClient>();

await usersClient.CreateUserAsync(new CreateUserDto
{
    FirstName = "Foo",
    LastName = "Bar",
    DisplayName = "Barry",
    Ssn = "123456",
    Email = "barry@email.com"
});

Console.WriteLine("Created user");

static IServiceProvider BuildServiceProvider()
{
    ServiceCollection services = new();

    services.AddHttpClient(nameof(IUsersClient), (sp, http) =>
    {
        http.BaseAddress = new Uri($"https://identity.local/");
        http.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
    })
    .AddTypedClient<IUsersClient>((http, sp) => new UsersClient(http));

    return services.BuildServiceProvider();
}