using MassTransit;

using YourBrand.Tenancy;

namespace YourBrand.Integration;

public class ReadTenantIdConsumeFilter<T>(ISettableTenantContext tenantContext) :
    IFilter<ConsumeContext<T>>
    where T : class
{
    public void Probe(ProbeContext context)
    {
    }

    public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var tenantId = context.Headers.Get<string>(Constants.TenantId);

        tenantContext.SetTenantId(tenantId!);

        return next.Send(context);
    }
}