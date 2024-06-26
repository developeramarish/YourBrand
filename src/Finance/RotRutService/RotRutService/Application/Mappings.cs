using YourBrand.RotRutService.Domain.Entities;
using YourBrand.RotRutService.Domain.Enums;

namespace YourBrand.RotRutService.Application;

public static class Mappings
{
    public static RotRutCaseDto ToDto(this RotRutCase @case)
    {
        return new RotRutCaseDto((DomesticServiceKind)@case.Kind, @case.Status,
            @case.Buyer, @case.PaymentDate, @case.LaborCost, @case.PaidAmount,
            @case.RequestedAmount, @case.InvoiceNo, @case.OtherCosts,
            @case.Hours, @case.MaterialCost, @case.ReceivedAmount, @case.Kind == DomesticServiceKind.HomeRepairAndMaintenanceServiceType ? @case.Rot?.ToDto() : null, @case.Kind == DomesticServiceKind.HouseholdService ? @case.Rut?.ToDto() : null);
    }

    public static RotDto ToDto(this Rot rot)
    {
        return new RotDto(rot.ServiceType.GetValueOrDefault(), rot.PropertyDesignation!, rot.ApartmentNo, rot.OrganizationNo);
    }

    public static RutDto ToDto(this Rut rut)
    {
        return new RutDto(rut.ServiceType.GetValueOrDefault());
    }
}