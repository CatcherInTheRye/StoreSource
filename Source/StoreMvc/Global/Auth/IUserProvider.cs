using DataRepository;

namespace PCSMvc.Global.Auth
{
    public interface IUserProvider
    {
        user User { get; set; }
    }
}
