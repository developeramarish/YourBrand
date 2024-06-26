namespace YourBrand.Invoicing.Domain;

public static class Errors
{
    public static class ProductPriceLists
    {
        public static readonly Error ProductPriceListNotFound = new Error(nameof(ProductPriceListNotFound), "ProductPriceListNotFound not found", string.Empty);
    }

    public static class ProductPrice
    {
        public static readonly Error ProductPriceNotFound = new Error(nameof(ProductPriceNotFound), "ProductPrice not found", string.Empty);
    }

    public static class Invoices
    {
        public static readonly Error InvoiceNotFound = new Error(nameof(InvoiceNotFound), "Invoice not found", string.Empty);

        public static readonly Error InvoiceItemNotFound = new Error(nameof(InvoiceItemNotFound), "Invoice item not found", string.Empty);
    }

    public static class Users
    {
        public static readonly Error UserNotFound = new Error(nameof(UserNotFound), "User not found", string.Empty);
    }
}