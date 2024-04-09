using Microsoft.EntityFrameworkCore;

using YourBrand.Payments.Application.Common.Interfaces;
using YourBrand.Payments.Domain;
using YourBrand.Payments.Domain.Events;
using YourBrand.Payments.Hubs;

namespace YourBrand.Payments.Application.Events;

public class PaymentCreatedHandler(IPaymentsContext context, IPaymentsHubClient paymentsHubClient) : IDomainEventHandler<PaymentCreated>
{
    public async Task Handle(PaymentCreated notification, CancellationToken cancellationToken)
    {
        var payment = await context
            .Payments
            .FirstOrDefaultAsync(i => i.Id == notification.PaymentId);

        if (payment is not null)
        {

        }
    }
}