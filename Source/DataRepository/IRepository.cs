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

        int CheckLoginUser(string userLogonId, string userPassword, int? attepmtTimeout);
        int CheckSession(long? loginId);
        
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

        #region Specialist
        List<RoleAreaForm> RoleAreasGet(int role);
        StudentMenuFull MenuGet(int area);
        BackgroundForm BackgroundGet(int user, int student, int type, int id);
        int BackgroundSave(int user, int student, string background, int type, int id);
        string ObservationsGet(int user, int student, int type, int id);
        int ObservationsSave(int user, int student, string observations, int type, int id);

        string TestGet(int user, int student, int type, int id);
        int TestSave(int user, int student, string test, int type, int id);

        string GoalsGet(int user, int student, int type, int id);
        int GoalsSave(int user, int student, string golas, int type, int id);

        string ProgressGet(int user, int student, int type, int id);
        int ProgressSave(int user, int student, string progress, int type, int id);

        string RecomendationsGet(int user, int student, int type, int id);
        int RecomendationsSave(int user, int student, string recomendations, int type, int id);

        string SummaryGet(int user, int student, int type, int id);
        int SummarySave(int user, int student, string summary, int type, int id);

        #region Attach Documents

        List<StudentSupportReportFileShort> StudentSupportReportFilesGet(int userId, int studentId);
        StudentSupportReportFileShort StudentSupportReportFileUpload(int userId, int studentId, StudentSupportReportFile reportFile);
        StudentSupportReportFile StudentSupportReportFileGet(int id);
        void StudentSupportReportFileRemove(int id);

        #endregion Attach Documents

        string StatusGet(int user, int student, int type, int id);
        int StatusSave(int user, int student, string summary, int type, int id);

        List<TimeRecordFrom> TimeRecordsGet(int user, int student);

        TimeRecordFrom TimeRecordOneGet(int recordId);
        int TimeRecordsSave(TimeRecordFrom form, int user, int student);
        int TimeRecordsDelete(int recordId);

        List<TimeRecordFrom> TimeRecordsOfficeGet(DateRange range, int userId);

        int TimeRecordsOfficeSave(TimeRecordFrom form, int userId);

        List<StudentSearchForm> StudentSearchGet(int userId);

        List<StudentSearchForm> StudentSearchGetParams(string filters, int user);

        List<UserShortInfo> OtherSpecialistsGet(int user, int student);

        void TeamConsultCreate(int user, int student, string users);
        List<TabMenuItem> TabMenuGet(int user, int student);

        UserActionForm UserActionGet(int user);

        List<StudentMenuItem> SchoolUserMenuGet(int user);

        List<UserShortInfo> StudentsForCurrentSchoolOrg(int user);

        //List<IdTitle> SupportRequestTypesGet();

        CurrentSchoolUsers CurrentDistrictSchoolUsers(int user, int role);

        Dictionary<int, List<UserShortInfoDistrict>> AllUsersGet(List<int> types, int role);

        StudentSupportRequestForm StudentSupportRequestSave(StudentSupportRequestForm form, int user);

        List<StudentSupportRequestSchortForm> ApprovalRequestsGet(int user);

        StudentSupportRequestSchortForm ApprovalRequestOneGet(int id);
        UploadedFile ApprovalRequestFileGet(int id);

        void StudentSupportRequestDelete(int id);
        StudentSupportRequestForm RequestFormGetOne(int id);


        bool StudentRequestDeny(int requestId);
        bool StudentRequestApprove(int requestId, string Note);

        List<SchoolStudentsSearch> StudentOrgSchoolGet(int user);

        List<SchoolStudentsSearch> StudentOrgSchoolGetParams(string filters, int user);
        UploadedFile StudentFileGet(int id);
        List<TabMenuItem> StudentReportsGet(int student);
        #endregion

        #region Organizations

        List<OrganizationForm> OrganizationsGet();
        OrganizationForm OrganizationGetForEdit(int id);
        OrganizationForm OrganizationUpdate(OrganizationForm organizationForm, int role, int userId);
        void OrganizationRemove(int id);

        void OrganizationSettingsUpdate(OrganizationSettingsForm settingsForm);
        OrganizationSettingsForm OrganizationSettingsCreate(int organizationId);
        OrganizationSettingsForm OrganizationSettingsGet(int organizationId);

        #endregion Organizations

        #region Schools

        List<SchoolFormShort> SchoolsGet();
        List<SchoolFormShort> SchoolsGet(int[] districtIds);
        SchoolForm SchoolUpdate(SchoolForm schoolForm);
        List<SchoolForm> SchoolsGetByAdminId(int organizationId);

        StudentFullInfo StudentReportGet(int student, int type, int id);

        List<UploadedFile> StudentFilesGet(int report, int type);

        #endregion Schools

        #region Support Request Types

        List<IdTitleDescription> SupportRequestTypesGet();

        #endregion Support Request Types

        #region Settings

        List<SettingsForm> SettingsGet();
        SettingsForm SettingsUpdate(SettingsForm settingsForm);
        void SettingRemove(int id);

        #endregion Settings

        #region Students

        StudentForm StudentUpdate(StudentForm studentForm, string updatedBy);

        #endregion Students

        #region District Sys Admin

        List<IdTitle> ImportSchools(string[] fileStrings, int organizationId);
        List<IdTitle> ImportStudents(string[] fileStrings, int organizationId, string updatedBy);
        List<IdTitle> ImportStaff(string[] fileStrings, int userRoleId, int organizationId, int currentUserId);

        List<DistrictStaffs> OrganizationStaffListGet(int user);
        bool OrganizationStaffListSave(List<UserActionForm> actions);
        List<StudentShortForm> OrganizationStudentsGet(int orgId);
        StudentForm StudentGet(int id);
        List<IdTitle> CurrentOrganiztionsSchoolsGet(int org);

        #endregion  District Sys Admin

        #region Emails
        
        NewRequestEmail NewRequestEmailDataGet(int user, int student);
        
        #endregion Emails


        #region Manager
        List<ManagerRequests> ManagerRequestsGet(int role, string startDate, string endDate);
        ManagerRequests ManagerRequestOneGet(int id);
        bool MangerRequestOneSave(int id, List<RequestTypeSpecialist> items, int student);

        #endregion Mangaer
    }
}