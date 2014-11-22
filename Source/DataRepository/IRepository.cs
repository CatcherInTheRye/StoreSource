using System.Collections.Generic;
using System.Linq;
using DataRepository.DataContracts;
using System.Web.UI.WebControls;
using PCS.DataRepository.DataContracts;

namespace DataRepository
{
    public interface IRepository
    {
        #region Authentication

        
        #endregion Authentication

        #region User Role

        RoleForm UserRoleGet(string userId);

        #endregion User Role

        #region Users

        IQueryable<user> Users { get; }
        user UserGetByLogin(string userName);
        user Login(string userName, string password);

        List<UserForm> UsersGetExtended();
        List<UserForm> UsersGetByRole(int roleId, bool includeInactive = false);
        List<UserForm> UsersGetByRoleFiltered(string filter, int roleId, bool includeInactive = false);

        UserForm UserGetForEdit(int id);
        UserForm UserUpdate(UserForm userForm, int userId);
        UserActionForm UserActionUpdate(UserActionForm userActionForm);
        void UserRemove(int id);

        #region Reports

        List<IdTitle> DistrictsGet();
        List<TimeRecordReportForm> TimeRecordReportGet(string start, string end, int specialist, int district, int school);
        List<string> TimeRecordReportTotalGet(string start, string end, int specialist);
        List<CaseRecordReportForm> CaseRecordReportGet(string start, string end, int specialist, int district, int school);
        #endregion Reports

        #region FTE

        List<FteForm> FtesGetForUser(int userId);
        FteForm FteUpdate(FteForm fteForm);
        void FteRemove(int id);

        #endregion FTE

        #endregion Users

    }
}