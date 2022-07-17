using YourBrand.Marketing.Application.Addresses;
using YourBrand.Marketing.Domain.Entities;
using YourBrand.Marketing.Application.Contacts;

namespace YourBrand.Marketing.Application;

public static class Mappings 
{
    public static ContactDto ToDto(this Contact person) 
    {
        return new ContactDto(person.Id, person.FirstName, person.LastName, person.Ssn, person.PhoneHome, person.PhoneMobile, person.Email);
    }

    public static AddressDto ToDto(this Address address) 
    {
        return new AddressDto(address.Id, address.Type, address.Thoroughfare, address.Premises, 
            address.SubPremises, address.PostalCode, address.Locality, 
            address.SubAdministrativeArea, address.AdministrativeArea, address.Country);
    }
}