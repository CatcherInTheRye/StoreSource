namespace DataRepository.Common
{
    public enum DictionaryTypes : int
    {
        Gender = 1,
        Status = 2,
        UserType = 3
    }

    public enum UserType: int
    {
        Undefined = 20,
        Root = 21,
        Admin = 22,
        ContentManager = 23,
        SalesPerson = 24,
        Buyer = 25,
        Seller = 26,
        SellerBuyer = 27
    }
}