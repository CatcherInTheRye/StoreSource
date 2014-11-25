using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Web.Caching;
using DataRepository.DataContracts;
using DataRepository.Mappers;
using Ninject;
using Ninject.Modules;
using PCSMvc.Mappers;
using PCS.DataRepository.DataContracts;
using System.Globalization;

namespace DataRepository
{
    //public class RepositoryNinjectModule : NinjectModule
    //{
    //    public override void Load()
    //    {
    //        this.Bind<IRepository>().To<Repository>().InSingletonScope()
    //            .WithConstructorArgument("connectionString", ConnectionString.ModelConnectionString);
    //    }
    //}

    public class Repository : IRepository
    {
        #region init

        [Inject]
        public VStoreEntities Db { get; set; }

        [Inject]
        public IMapper ModelMapper { get; set; }

        //SubmitChanges
        private EfStatus SubmitChanges()
        {
            var status = new EfStatus ();
            try
            {
                Db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                return status.SetErrors(ex.EntityValidationErrors);
            }
            catch (DbUpdateException ex)
            {
                var decodedErrors = status.TryDecodeDbUpdateException(ex);
                if (decodedErrors == null)
                    throw; //it isn't something we understand so rethrow
                return status.SetErrors(decodedErrors);
            }
            //else it isn't an exception we understand so it throws in the normal way
            return status;
        }

        #endregion init


        #region Users

        public IQueryable<User> Users
        {
            get
            {
                return Db.Users;
            }
        }

        public List<UserForm> UserForms
        {
            //TODO: Tolik. First ToList() may be redundant. Check SQL Server
            get { return Users.ToList().Select(u => (UserForm)ModelMapper.Map(u, typeof(User), typeof(UserForm))).ToList(); }
        }

        public User UserGetByLogin(string userName)
        {
            return Users.FirstOrDefault(p => string.Compare(p.Phone, userName, true) == 0);
        }

        public User Login(string userName, string password)
        {
            return Users.FirstOrDefault(p => string.Compare(p.userName, userName, true) == 0 && p.pwd == password);
        }

        public List<UserForm> UsersGetExtended()
        {
            var joinedData = (from u in UserForms
                              join ur in Db.userRoles on u.Id equals ur.userId
                              select new { u, ur }
                      ).ToList();
            foreach (var item in joinedData)
            {
                item.u.UserRole = item.ur.roleId;
            }
            List<UserForm> result = joinedData.Select(p => p.u).ToList();

            //speciality for selected users
            List<int> userIds = result.Select(p => p.Id).ToList();
            List<UserSupportRequestTypeForm> supportReqTypes = (from ust in Db.userSupportTypes
                                                                join srt in Db.supportRequestTypes on ust.supportRequestTypeId equals srt.id
                                                                where userIds.Contains(ust.userId)
                                                                select new UserSupportRequestTypeForm
                                                                {
                                                                    Id = srt.id,
                                                                    Name = srt.name,
                                                                    Code = srt.code,
                                                                    UserId = ust.userId
                                                                }).ToList();
            result.ForEach(p =>
                           p.Speciality = string.Join(", ", supportReqTypes.Where(q => q.UserId == p.Id).Select(q => q.Code))
                );
            result.ForEach(p =>
                           p.UserSupportRequests = string.Join("_", supportReqTypes.Where(q => q.UserId == p.Id).Select(q => q.Id))
                );
            //caseload for selected users
            List<studentSupportRequestAssignment> studentSupportRequestAssignments =
                Db.studentSupportRequestAssignments
                    .Where(p => userIds.Contains(p.specialistId)).ToList();
            result.ForEach(p =>
                           p.CaseLoad = studentSupportRequestAssignments.Count(q => q.specialistId == p.Id).ToString());
            //primary locations for selected users
            //schools
            List<UserSchoolForm> userSchoolForms = (from udl in Db.userDefaultLocations
                                                    join org in Db.organizations on udl.organizationId equals org.id
                                                    join s in Db.schools on udl.schoolId equals s.id into ss
                                                    from subschool in ss.DefaultIfEmpty()
                                                    where userIds.Contains(udl.userId)

                                                    select new UserSchoolForm
                                                               {
                                                                   Id = (subschool != null ? subschool.id : -1),
                                                                   Title = (subschool != null
                                                                            ? string.Format("{0} {1}", org.districtName,
                                                                                            subschool.schoolName)
                                                                            : org.districtName),
                                                                   OrganizationId = udl.organizationId,
                                                                   UserId = udl.userId
                                                               }).ToList();
            result.ForEach(p =>
                               {
                                   p.Location = string.Join(", ",
                                                            userSchoolForms.Where(q => q.UserId == p.Id).Select(
                                                                q => q.Title));
                                   p.Schools = string.Join("_", userSchoolForms.Where(q => q.UserId == p.Id).Select(
                                                                q => q.Id));
                                   p.Districts = string.Join("_", userSchoolForms.Where(q => q.UserId == p.Id).Select(
                                                                q => q.OrganizationId));
                               }
                );
            //districts


            return result;
        }

        public List<UserForm> UsersGetByRole(int roleId, bool includeInactive = false)
        {
            List<UserForm> result = UsersGetExtended().Where(p =>
                p.UserRole == roleId && (includeInactive || (!includeInactive && p.Active))).ToList();
            return result;
        }

        public List<UserForm> UsersGetByRoleFiltered(string filter, int roleId, bool includeInactive = false)
        {
            filter = filter.ToLower();
            List<UserForm> result = UsersGetByRole(roleId, includeInactive);

            string[] filterSplitted = filter.Split('|');
            if (!filter.Contains("=") && filterSplitted.Count() == 1)
            {
                result = result.Where(p => p.FullName.ToLower().Contains(filter)
                    || p.Location.ToLower().Contains(filter)
                    || p.Speciality.ToLower().Contains(filter)).ToList();
            }
            else if (filter.Contains("="))
            {
                List<KeyValuePair<string, string>> filterColumns = filterSplitted.Select(s =>
                    s.Split('=')).Select(keyValue =>
                        new KeyValuePair<string, string>(keyValue[0], keyValue.Count() > 1 ? keyValue[1] : string.Empty)).ToList();
                foreach (KeyValuePair<string, string> filterColumn in filterColumns)
                {
                    switch (filterColumn.Key)
                    {
                        case "name":
                            result = result.Where(p => p.FullName.ToLower().Contains(filterColumn.Value)).ToList();
                            break;
                        case "speciality":
                            result = result.Where(p => p.Speciality.ToLower().Contains(filterColumn.Value)).ToList();
                            break;
                        case "location":
                            result = result.Where(p => p.Location.ToLower().Contains(filterColumn.Value)).ToList();
                            break;
                        default:
                            break;
                    }
                }
            }


            return result;
        }

        public UserForm UserGetForEdit(int id)
        {
            UserForm result = UsersGetExtended().FirstOrDefault(p => p.Id == id);
            if (result != null)
            {
                User u = Users.FirstOrDefault(p => p.id == id);
                if (u != null)
                {
                    result.Pwd = u.pwd;
                    //result.ConfirmPwd = u.pwd;
                }
            }
            else
            {
                result = new UserForm();
            }
            return result;
        }

        public UserForm UserUpdate(UserForm userForm, int userId)
        {
            User User = Users.FirstOrDefault(p => p.id == userForm.Id);
            bool userCreated = false;
            if (User == null) //create
            {
                User = new User();
                User.active = true;
                Db.users.InsertOnSubmit(User);
                userCreated = true;
            }

            //TODO: Tolik. check
            //User = (User) ModelMapper.Map(userForm, typeof (UserForm), typeof (User));
            User.firstName = userForm.FirstName;
            User.lastName = userForm.LastName;
            User.middleName = userForm.MiddleName;
            User.phone = userForm.Phone;
            User.cell = userForm.Cell;
            User.salutation = userForm.Salutation;
            User.userName = userForm.UserName;
            User.email = userForm.Email;
            User.ps_user_id = !string.IsNullOrWhiteSpace(userForm.PsUserId) ? int.Parse(userForm.PsUserId) : -1;

            User.pwd = userForm.Pwd;
            User.pwdChangedBy = userId;
            User.pwdChangedDate = DateTime.UtcNow;

            SubmitChanges();

            if (userCreated)
            {
                //User role
                userRole userRole = new userRole();
                userRole.User = User;
                userRole.roleId = userForm.UserRole;
            }

            //Speciality. User support types
            if (!string.IsNullOrEmpty(userForm.UserSupportRequests))
            {
                List<userSupportType> userSupportTypes = User.userSupportTypes.ToList();
                //remove unused
                List<userSupportType> userSupportTypesForDelete =
                    userSupportTypes.Where(p => p.supportRequestTypeId != int.Parse(userForm.UserSupportRequests)).ToList();
                foreach (userSupportType userSupportType in userSupportTypesForDelete)
                {
                    Db.userSupportTypes.DeleteOnSubmit(userSupportType);
                }
                //add new
                if (!userSupportTypes.Any(p => p.supportRequestTypeId == int.Parse(userForm.UserSupportRequests)))
                {
                    userSupportType newUserSupportType = new userSupportType
                    {
                        User = User,
                        supportRequestTypeId = int.Parse(userForm.UserSupportRequests)
                    };
                    Db.userSupportTypes.InsertOnSubmit(newUserSupportType);
                }
            }

            //Schools. Districts == Organizations.
            if (!string.IsNullOrWhiteSpace(userForm.Districts) || !string.IsNullOrWhiteSpace(userForm.Schools))
            {
                //remove old values
                List<userDefaultLocation> userDefaultLocations = User.userDefaultLocations.ToList(); //what are in base now
                foreach (userDefaultLocation userDefaultLocation in userDefaultLocations)
                {
                    Db.userDefaultLocations.DeleteOnSubmit(userDefaultLocation);
                }

                //new values
                List<int> userFormOrganizations = !string.IsNullOrWhiteSpace(userForm.Districts)
                                                      ? userForm.Districts.Split('_').Select(int.Parse).ToList()
                                                      : new List<int>();
                List<int> userFormSchools = !string.IsNullOrWhiteSpace(userForm.Schools)
                                                ? userForm.Schools.Split('_').Select(int.Parse).ToList()
                                                : new List<int>();
                List<SchoolFormShort> schools = SchoolsGet();
                List<SchoolFormShort> userSchools = schools.Where(p => userFormSchools.Contains(p.Id)).ToList();
                //pure organizations without schools
                List<SchoolFormShort> changes = new List<SchoolFormShort>();
                foreach (int i in userFormOrganizations)
                {
                    if (userSchools.Count(p => p.OrganizationId == i) == 0)
                    {
                        changes.Add(new SchoolFormShort
                                        {
                                            Id = -1,
                                            OrganizationId = i
                                        });
                    }
                }

                //add schools
                changes.AddRange(userSchools);
                foreach (SchoolFormShort change in changes)
                {
                    userDefaultLocation toAdd = new userDefaultLocation()
                                                    {
                                                        User = User,
                                                        organizationId = change.OrganizationId,
                                                        schoolId = change.Id != -1 ? change.Id : (int?)null
                                                    };
                    Db.userDefaultLocations.InsertOnSubmit(toAdd);
                }
            }
            if (!string.IsNullOrWhiteSpace(userForm.Organizations))
            {
                List<int> organizations = userForm.Organizations.Split('_').ToList().Select(p => int.Parse(p)).ToList();
                if (organizations != null && organizations.Count > 0)
                {
                    foreach (int i in organizations)
                    {
                        userOrganization record = Db.userOrganizations.Where(p => p.User == User && p.organizationId == i).FirstOrDefault();
                        if (record == null)
                        {
                            record = new userOrganization();
                            record.organizationId = i;
                            record.User = User;
                            Db.userOrganizations.InsertOnSubmit(record);
                        }

                    }
                }
            }

            if (userForm.UserActionForm != null)
            {
                userForm.UserActionForm.UserId = User.id;
                UserActionUpdate(userForm.UserActionForm);
            }
            SubmitChanges();

            return UserGetForEdit(User.id);
        }

        public UserActionForm UserActionUpdate(UserActionForm userActionForm)
        {
            userAction record = Db.userActions.FirstOrDefault(t => t.id == userActionForm.Id);
            if (record == null)
            {
                record = new userAction();
                record.userId = userActionForm.UserId;
                Db.userActions.InsertOnSubmit(record);
            }
            record.canApprove = userActionForm.CanApprove;
            record.canRequest = userActionForm.CanRequest;
            record.canViewReports = userActionForm.CanViewReports;
            SubmitChanges();
            userActionForm.Id = record.id;
            return userActionForm;
        }

        public void UserRemove(int id)
        {
            User User = Users.FirstOrDefault(t => t.id == id);
            if (User == null) return;
            //Db.users.DeleteOnSubmit(User);
            User.active = false;
            SubmitChanges();
        }

        #region Reports
        public List<IdTitle> DistrictsGet()
        {
            List<IdTitle> districts = Db.organizations.Select(p => new IdTitle() { Id = p.id, Title = p.districtName }).ToList();
            return districts;
        }

        public List<TimeRecordReportForm> TimeRecordReportGet(string start, string end, int specialist, int district, int school)
        {
            DateTime startDate = string.IsNullOrEmpty(start) ? new DateTime() : DateTime.ParseExact(start, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
            DateTime endDate = string.IsNullOrEmpty(end) ? new DateTime() : DateTime.ParseExact(end, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
            List<TimeRecordReportForm> result = Db.userTimeRecords.Where(p => (string.IsNullOrEmpty(start) || p.date > startDate)
                && (string.IsNullOrEmpty(end) || p.date < endDate)
                && (specialist == -1 || p.userId == specialist)
                && (district == -1 || p.User.userDefaultLocations.FirstOrDefault().organizationId == district)
                && (school == -1 || p.User.userDefaultLocations.FirstOrDefault().schoolId == school)).Select(p => new TimeRecordReportForm()
            {
                Id = p.id,
                Code = p.code,
                Date = p.date,
                Note = p.notes,
                Specialist = p.User.FullName,
                Student = (p.studentId == null ? "Office" : p.student.firstName + " " + p.student.lastName),
                Time = p.time
            }).OrderByDescending(p => p.Date).ThenBy(p => p.Specialist).ToList();
            return result;
        }

        public List<string> TimeRecordReportTotalGet(string start, string end, int specialist)
        {
            DateTime startDate = string.IsNullOrEmpty(start) ? new DateTime() : DateTime.ParseExact(start, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
            DateTime endDate = string.IsNullOrEmpty(end) ? new DateTime() : DateTime.ParseExact(end, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
            List<string> result = new List<string>();
            var data = TimeRecordReportGet(start, end, specialist, -1, -1);
            int total = data.Sum(p => p.Time);
            result.Add(string.Format("<li>Total: {0}</li>", total));
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end) && startDate.Year == endDate.Year && startDate.Month == endDate.Month)
            {
                int fte = Db.settings.Where(p => p.Month == startDate.Month && p.Year == startDate.Year).Select(p => p.Hours).FirstOrDefault();
                result.Add(string.Format("<li>FTE: {0}</li>", fte));
                int dif = fte - total;
                result.Add(dif < 0 ? string.Format("<li>+/-: <span class='red'>{0}</span></li>", dif) : string.Format("<li>+/-: {0}</li>", dif));
            }
            return result;
        }

        public List<CaseRecordReportForm> CaseRecordReportGet(string start, string end, int specialist, int district, int school)
        {
            DateTime startDate = string.IsNullOrEmpty(start) ? new DateTime() : DateTime.ParseExact(start, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
            DateTime endDate = string.IsNullOrEmpty(end) ? new DateTime() : DateTime.ParseExact(end, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
            var data = Db.users.Where(p => p.id == 7).FirstOrDefault().userDefaultLocations.FirstOrDefault();
            List<CaseRecordReportForm> result = Db.studentSupportRequests.Where(p => (string.IsNullOrEmpty(start) || p.dateRequested > startDate)
                && (string.IsNullOrEmpty(end) || p.dateRequested < endDate)
                && (specialist == -1 || p.requestedBy == specialist)
                && (district == -1 || p.User.userDefaultLocations.FirstOrDefault().organizationId == district)
                && (school == -1 || p.User.userDefaultLocations.FirstOrDefault().schoolId == school)).Select(p => new CaseRecordReportForm()
                {
                    Id = p.id,
                    District = p.User.userDefaultLocations.FirstOrDefault().organization.districtName,
                    School = p.User.userDefaultLocations.FirstOrDefault().school.schoolName,
                    Specialist = p.User.firstName + " " + p.User.lastName,
                    Student = p.student.firstName + " " + p.student.lastName,
                    Type = string.Join(", ", p.studentSupportRequestTypes.ToList().Select(t => t.supportRequestType.code).ToList()),
                    Grade = p.student.grade,
                    Dob = p.student.dob
                }).OrderBy(p => p.Specialist).ThenBy(p => p.District).ThenBy(p => p.School).ThenBy(p => p.Student).ToList();
            return result;
        }
        #endregion Reports

        #region FTE

        public List<FteForm> FtesGetForUser(int userId)
        {
            List<FteForm> result = Db.userFteValues.ToList().Where(p => p.userId == userId).Select(p =>
                                new FteForm
                                    {
                                        Id = p.id,
                                        UserId = p.userId,
                                        Value = p.fteValue.ToString("F1"),
                                        //From = p.fteFrom != null ?  DateTime.ParseExact(p.fteFrom.GetValueOrDefault(), "yyyy.MM.dd", CultureInfo.InvariantCulture): string.Empty,
                                        From = p.fteFrom != null ? p.fteFrom.GetValueOrDefault().ToString("yyyy.MM.dd") : string.Empty,
                                        //p.fteFrom != null ? p.fteFrom.GetValueOrDefault().ToString("{0:yyyy.MM.dd}") : string.Empty,
                                        To = p.fteTo != null ? p.fteTo.GetValueOrDefault().ToString("yyyy.MM.dd") : string.Empty,
                                    }).ToList();
            return result;
        }

        public FteForm FteUpdate(FteForm fteForm)
        {
            double val;
            if (!double.TryParse(fteForm.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
            {
                throw new Exception("Value can't be empty");
            }

            userFteValue record = Db.userFteValues.FirstOrDefault(t => t.id == fteForm.Id);
            if (record == null)
            {
                record = new userFteValue();
                record.userId = fteForm.UserId;
                Db.userFteValues.InsertOnSubmit(record);
            }
            record.fteValue = double.Parse(fteForm.Value, NumberStyles.Any, CultureInfo.InvariantCulture);
            record.fteFrom = !string.IsNullOrWhiteSpace(fteForm.From) ? DateTime.ParseExact(fteForm.From, "yyyy.MM.dd", CultureInfo.InvariantCulture) : (DateTime?)null;
            record.fteTo = !string.IsNullOrWhiteSpace(fteForm.To) ? DateTime.ParseExact(fteForm.To, "yyyy.MM.dd", CultureInfo.InvariantCulture) : (DateTime?)null;
            SubmitChanges();
            fteForm.Id = record.id;
            return fteForm;
        }

        public void FteRemove(int id)
        {
            userFteValue record = Db.userFteValues.FirstOrDefault(t => t.id == id);
            if (record == null) return;
            Db.userFteValues.DeleteOnSubmit(record);
            SubmitChanges();
        }

        #endregion FTE

        #endregion Users

    }
}