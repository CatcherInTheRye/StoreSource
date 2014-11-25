using System.Collections.Generic;
using System.Linq;
using DataRepository.DataContracts;
using System.Web.UI.WebControls;
using PCS.DataRepository.DataContracts;

namespace DataRepository
{
    public interface IRepository
    {
        
        #region Users

        IQueryable<User> Users { get; }
        User UserGetByLogin(string userName);
        User Login(string userName, string password);

        UserForm UserUpdate(UserForm userForm, int userId);
        void UserRemove(int id);

        #endregion Users

    }
}