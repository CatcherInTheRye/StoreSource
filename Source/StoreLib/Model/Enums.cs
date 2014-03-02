namespace StoreLib.Model
{
    public enum UserTypes : byte
    {
        Undefined = 0,
        Root = 1,
        Admin = 2,
        ContentManager = 3,
        SalesPerson = 4,
        Buyer = 5,
        Seller = 6,
        SellerBuyer = 7
    }

    public enum Statuses : byte
    {
        Undefined = 0,
        Pending = 1,
        Active = 2,
        Inactive = 3,
        Locked = 4
    }

    public enum LoginStatus: byte
    {
        Undefined = 0,
        OK = 1,
        UsernameRequired = 2,
        PasswordRequired = 3,
        WrongPassword = 4,
        Locked = 5,
        Pending = 6
    }
}