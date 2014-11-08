using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Web.Caching;
using DataRepository.DataContracts;
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
        public DataClassesPCSDataContext Db { get; set; }

        [Inject]
        public IMapper ModelMapper { get; set; }

        //SubmitChanges
        private bool SubmitChanges()
        {
            bool res = true;
            try
            {
                Db.SubmitChanges();
            }
            catch (ChangeConflictException)
            {
                try
                {
                    foreach (ObjectChangeConflict occ in Db.ChangeConflicts)
                    {
                        occ.Resolve(RefreshMode.KeepCurrentValues);
                    }
                }
                catch
                {
                    res = false;
                }
            }
            return res;
        }

        #endregion init

        #region Authentication

        //0 : successfully ; 1: bad password ; 2: bad user name or wrong email; 3: user login is disabled; 99: database operation failed
        public int CheckLoginUser(string userLogonId, string userPassword, int? attepmtTimeout = null)
        {
            int result = 99;
            ISingleResult<serviceValidateUserResult> spResult = Db.serviceValidateUser(userLogonId, userPassword, attepmtTimeout);
            foreach (var item in spResult)
            {
                result = item.result;
                break;
            }
            return result;
        }

        public int CheckSession(long? loginId)
        {
            int result = -1;

            ISingleResult<serviceCheckSessionResult> spResult = Db.serviceCheckSession(loginId);
            foreach (var item in spResult)
            {
                result = item.Column1;
                break;
            }
            return result;

            //using (SqlConnection tmpConnection = new SqlConnection(Settings.DbConnectionString))
            //{
            //    SqlCommand command = new
            //            SqlCommand("Exec serviceCheckSession @login_id", tmpConnection);
            //    command.Parameters.Add("@login_id", SqlDbType.BigInt).Value = login_id;
            //    tmpConnection.Open();
            //    return (int)command.ExecuteScalar();
            //}
        }

        #endregion Authentication

        #region User Role

        public RoleForm UserRoleGet(string userId)
        {
            int id;
            int.TryParse(userId, out id);
            role result = null;
            List<userRole> userRoles = Db.userRoles.Where(p => p.userId == id).ToList();
            //now user has only 1 role
            userRole userRole = userRoles.FirstOrDefault();
            if (userRole != null)
            {
                List<role> roles = Db.roles.Where(p => p.id == userRole.roleId).ToList();
                result = roles.FirstOrDefault();
            }

            return result != null
                       ? new RoleForm
                             {
                                 Id = result.id,
                                 Title = result.roleName,
                                 Description = result.roleDescription
                             }
                       : null;
        }

        #endregion User Role

        #region Users

        public IQueryable<user> Users
        {
            get
            {
                return Db.users;
            }
        }

        public List<UserForm> UserForms
        {
            //TODO: Tolik. First ToList() may be redundant. Check SQL Server
            get { return Users.ToList().Select(u => (UserForm)ModelMapper.Map(u, typeof(user), typeof(UserForm))).ToList(); }
        }

        public user UserGetByLogin(string userName)
        {
            return Users.FirstOrDefault(p => string.Compare(p.userName, userName, true) == 0);
        }

        public user Login(string userName, string password)
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
                user u = Users.FirstOrDefault(p => p.id == id);
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
            user user = Users.FirstOrDefault(p => p.id == userForm.Id);
            bool userCreated = false;
            if (user == null) //create
            {
                user = new user();
                user.active = true;
                Db.users.InsertOnSubmit(user);
                userCreated = true;
            }

            //TODO: Tolik. check
            //user = (user) ModelMapper.Map(userForm, typeof (UserForm), typeof (user));
            user.firstName = userForm.FirstName;
            user.lastName = userForm.LastName;
            user.middleName = userForm.MiddleName;
            user.phone = userForm.Phone;
            user.cell = userForm.Cell;
            user.salutation = userForm.Salutation;
            user.userName = userForm.UserName;
            user.email = userForm.Email;
            user.ps_user_id = !string.IsNullOrWhiteSpace(userForm.PsUserId) ? int.Parse(userForm.PsUserId) : -1;

            user.pwd = userForm.Pwd;
            user.pwdChangedBy = userId;
            user.pwdChangedDate = DateTime.UtcNow;

            SubmitChanges();

            if (userCreated)
            {
                //user role
                userRole userRole = new userRole();
                userRole.user = user;
                userRole.roleId = userForm.UserRole;
            }

            //Speciality. User support types
            if (!string.IsNullOrEmpty(userForm.UserSupportRequests))
            {
                List<userSupportType> userSupportTypes = user.userSupportTypes.ToList();
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
                        user = user,
                        supportRequestTypeId = int.Parse(userForm.UserSupportRequests)
                    };
                    Db.userSupportTypes.InsertOnSubmit(newUserSupportType);
                }
            }

            //Schools. Districts == Organizations.
            if (!string.IsNullOrWhiteSpace(userForm.Districts) || !string.IsNullOrWhiteSpace(userForm.Schools))
            {
                //remove old values
                List<userDefaultLocation> userDefaultLocations = user.userDefaultLocations.ToList(); //what are in base now
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
                                                        user = user,
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
                        userOrganization record = Db.userOrganizations.Where(p => p.user == user && p.organizationId == i).FirstOrDefault();
                        if (record == null)
                        {
                            record = new userOrganization();
                            record.organizationId = i;
                            record.user = user;
                            Db.userOrganizations.InsertOnSubmit(record);
                        }

                    }
                }
            }

            if (userForm.UserActionForm != null)
            {
                userForm.UserActionForm.UserId = user.id;
                UserActionUpdate(userForm.UserActionForm);
            }
            SubmitChanges();

            return UserGetForEdit(user.id);
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
            user user = Users.FirstOrDefault(t => t.id == id);
            if (user == null) return;
            //Db.users.DeleteOnSubmit(user);
            user.active = false;
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
                && (district == -1 || p.user.userDefaultLocations.FirstOrDefault().organizationId == district)
                && (school == -1 || p.user.userDefaultLocations.FirstOrDefault().schoolId == school)).Select(p => new TimeRecordReportForm()
            {
                Id = p.id,
                Code = p.code,
                Date = p.date,
                Note = p.notes,
                Specialist = p.user.FullName,
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
                && (district == -1 || p.user.userDefaultLocations.FirstOrDefault().organizationId == district)
                && (school == -1 || p.user.userDefaultLocations.FirstOrDefault().schoolId == school)).Select(p => new CaseRecordReportForm()
                {
                    Id = p.id,
                    District = p.user.userDefaultLocations.FirstOrDefault().organization.districtName,
                    School = p.user.userDefaultLocations.FirstOrDefault().school.schoolName,
                    Specialist = p.user.firstName + " " + p.user.lastName,
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

        #region Specialist
        public List<RoleAreaForm> RoleAreasGet(int role)
        {
            List<RoleAreaForm> result = new List<RoleAreaForm>();

            result = (from r in Db.roleAreas
                      where r.role == role
                      select new RoleAreaForm
                      {
                          Id = r.ID,
                          Href = r.href,
                          Image = r.image,
                          Text = r.Text
                      }).ToList();
            return result;
        }

        public StudentMenuFull MenuGet(int area)
        {
            List<StudentMenuItem> first = (from it in Db.menus
                                           where it.level == 1 && it.roleArea == area
                                           orderby it.oreder
                                           select new StudentMenuItem
                                           {
                                               href = it.href,
                                               level = it.level.ToString(),
                                               text = it.Text,
                                               title = it.title
                                           }).ToList();
            List<StudentMenuItem> second = (from it in Db.menus
                                            where it.level == 2 && it.roleArea == area
                                            orderby it.oreder
                                            select new StudentMenuItem
                                            {
                                                href = it.href,
                                                level = it.level.ToString(),
                                                text = it.Text,
                                                title = it.title
                                            }).ToList();
            StudentMenuFull menu = new StudentMenuFull();
            menu.first = first;
            menu.second = second;
            return menu;
        }

        private studentSupportReport CurrentSupportReportGet(int user, int student)
        {
            var data = (from it in Db.studentSupportReports
                        where it.specialistId == user && it.studentId == student && it.dateFinalized == null
                        select it).FirstOrDefault();
            return data;
        }

        public BackgroundForm BackgroundGet(int user, int student, int type, int id)
        {
            BackgroundForm data = new BackgroundForm();
            switch (type)
            {
                case 1:
                    data = (from it in Db.studentSupportReports
                            where it.specialistId == user && it.studentId == student && it.dateFinalized == null
                            select new BackgroundForm
                            {
                                background = it.background ?? "",
                                Reason = it.studentSupportRequest.reason.ToString() ?? "",
                                ReferedBy = it.studentSupportRequest.user.FullName
                            }).FirstOrDefault();
                    break;
                case 2:
                    data = Db.studentTeamConsultations.Where(p => p.id == id).Select(p => new BackgroundForm() { background = p.background, ReferedBy = "", Reason = "" }).FirstOrDefault();
                    break;
            }

            return data;
        }

        public int BackgroundSave(int user, int student, string background, int type, int id)
        {
            int row = 0;
            if (type == 1)
            {
                Db.studentSupportReports.Where(p => p.studentId == student && p.specialistId == user).FirstOrDefault().background = background;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentSupportReports.Context.SubmitChanges();
            }
            else if (type == 2)
            {
                Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault().background = background;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentTeamConsultations.Context.SubmitChanges();
            }
            //data.observations = observations;
            return row;
        }

        public string ObservationsGet(int user, int student, int type, int id)
        {
            string observations = "";
            switch (type)
            {
                case 1:
                    var data = Db.studentSupportReports.Where(p => p.specialistId == user && p.studentId == student && p.dateFinalized == null).FirstOrDefault();
                    observations = data == null ? "" : string.IsNullOrEmpty(data.observations) ? "" : data.observations;
                    break;
                case 2:
                    var data2 = Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault();
                    observations = data2 == null ? "" : string.IsNullOrEmpty(data2.observations) ? "" : data2.observations;
                    break;
            }
            return observations;
        }

        public int ObservationsSave(int user, int student, string observations, int type, int id)
        {
            int row = 0;
            if (type == 1)
            {
                Db.studentSupportReports.Where(p => p.studentId == student && p.specialistId == user).FirstOrDefault().observations = observations;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentSupportReports.Context.SubmitChanges();
            }
            else if (type == 2)
            {
                Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault().observations = observations;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentTeamConsultations.Context.SubmitChanges();
            }
            //data.observations = observations;
            return row;
        }

        public string TestGet(int user, int student, int type, int id)
        {
            string interpretations = "";
            switch (type)
            {
                case 1:
                    var data = Db.studentSupportReports.Where(p => p.specialistId == user && p.studentId == student && p.dateFinalized == null).FirstOrDefault();
                    interpretations = string.IsNullOrEmpty(data.interpretations) ? "" : data.interpretations;
                    break;
                case 2:
                    var data2 = Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault();
                    interpretations = string.IsNullOrEmpty(data2.interpretations) ? "" : data2.interpretations;
                    break;
            }
            return interpretations;
        }

        public int TestSave(int user, int student, string test, int type, int id)
        {
            int row = 0;
            if (type == 1)
            {
                Db.studentSupportReports.Where(p => p.studentId == student && p.specialistId == user).FirstOrDefault().interpretations = test;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentSupportReports.Context.SubmitChanges();
            }
            else if (type == 2)
            {
                Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault().interpretations = test;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentTeamConsultations.Context.SubmitChanges();
            }
            //data.observations = observations;
            return row;
        }

        public string GoalsGet(int user, int student, int type, int id)
        {
            string goals = "";
            switch (type)
            {
                case 1:
                    var data = Db.studentSupportReports.Where(p => p.specialistId == user && p.studentId == student && p.dateFinalized == null).FirstOrDefault();
                    goals = string.IsNullOrEmpty(data.goals) ? "" : data.goals;
                    break;
                case 2:
                    var data2 = Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault();
                    goals = string.IsNullOrEmpty(data2.goals) ? "" : data2.goals;
                    break;
            }
            return goals;
        }

        public int GoalsSave(int user, int student, string goals, int type, int id)
        {
            int row = 0;
            if (type == 1)
            {
                Db.studentSupportReports.Where(p => p.studentId == student && p.specialistId == user).FirstOrDefault().goals = goals;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentSupportReports.Context.SubmitChanges();
            }
            else if (type == 2)
            {
                Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault().goals = goals;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentTeamConsultations.Context.SubmitChanges();
            }
            //data.observations = observations;
            return row;
        }

        public string ProgressGet(int user, int student, int type, int id)
        {
            string progress = "";
            switch (type)
            {
                case 1:
                    var data = Db.studentSupportReports.Where(p => p.specialistId == user && p.studentId == student && p.dateFinalized == null).FirstOrDefault();
                    progress = string.IsNullOrEmpty(data.progress) ? "" : data.progress;
                    break;
                case 2:
                    var data2 = Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault();
                    progress = string.IsNullOrEmpty(data2.progress) ? "" : data2.progress;
                    break;
            }
            return progress;
        }

        public int ProgressSave(int user, int student, string progress, int type, int id)
        {
            int row = 0;
            if (type == 1)
            {
                Db.studentSupportReports.Where(p => p.studentId == student && p.specialistId == user).FirstOrDefault().progress = progress;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentSupportReports.Context.SubmitChanges();
            }
            else if (type == 2)
            {
                Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault().progress = progress;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentTeamConsultations.Context.SubmitChanges();
            }
            //data.observations = observations;
            return row;
        }

        public string RecomendationsGet(int user, int student, int type, int id)
        {
            string recommendations = "";
            switch (type)
            {
                case 1:
                    var data = Db.studentSupportReports.Where(p => p.specialistId == user && p.studentId == student && p.dateFinalized == null).FirstOrDefault();
                    recommendations = string.IsNullOrEmpty(data.recommendations) ? "" : data.recommendations;
                    break;
                case 2:
                    var data2 = Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault();
                    recommendations = string.IsNullOrEmpty(data2.progress) ? "" : data2.recommendations;
                    break;
            }
            return recommendations;
        }

        public int RecomendationsSave(int user, int student, string recommendations, int type, int id)
        {
            int row = 0;
            if (type == 1)
            {
                Db.studentSupportReports.Where(p => p.studentId == student && p.specialistId == user).FirstOrDefault().recommendations = recommendations;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentSupportReports.Context.SubmitChanges();
            }
            else if (type == 2)
            {
                Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault().recommendations = recommendations;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentTeamConsultations.Context.SubmitChanges();
            }
            //data.observations = observations;
            return row;
        }

        public string SummaryGet(int user, int student, int type, int id)
        {
            string summary = "";
            switch (type)
            {
                case 1:
                    var data = Db.studentSupportReports.Where(p => p.specialistId == user && p.studentId == student && p.dateFinalized == null).FirstOrDefault();
                    summary = string.IsNullOrEmpty(data.summary) ? "" : data.summary;
                    break;
                case 2:
                    var data2 = Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault();
                    summary = string.IsNullOrEmpty(data2.summary) ? "" : data2.summary;
                    break;
            }
            return summary;
        }

        public int SummarySave(int user, int student, string summary, int type, int id)
        {
            int row = 0;
            if (type == 1)
            {
                Db.studentSupportReports.Where(p => p.studentId == student && p.specialistId == user).FirstOrDefault().summary = summary;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentSupportReports.Context.SubmitChanges();
            }
            else if (type == 2)
            {
                Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault().summary = summary;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentTeamConsultations.Context.SubmitChanges();
            }
            //data.observations = observations;
            return row;
        }

        #region Attach Documents

        public List<StudentSupportReportFileShort> StudentSupportReportFilesGet(int userId, int studentId)
        {
            List<StudentSupportReportFileShort> result = new List<StudentSupportReportFileShort>();
            studentSupportReport studentSupportReport = Db.studentSupportReports.FirstOrDefault(p =>
                p.specialistId == userId
                && p.studentId == studentId
                && !p.dateFinalized.HasValue);
            if (studentSupportReport != null)
            {
                result =
                    Db.studentSupportReportFiles.Where(p => p.studentSupportReportId == studentSupportReport.id).Select(
                        p =>
                        new StudentSupportReportFileShort
                        {
                            Id = p.id,
                            DateUploaded = p.dateUploaded,
                            FileName = p.fileName,
                            StudentSupportReportId = p.studentSupportReportId,
                            UploadedBy = p.uploadedBy,
                            UploadedByName = p.user.FullName,
                            Description = p.description
                        }).ToList();
            }
            return result;
        }

        public StudentSupportReportFileShort StudentSupportReportFileUpload(int userId, int studentId, StudentSupportReportFile reportFile)
        {
            studentSupportReport studentSupportReport = Db.studentSupportReports.FirstOrDefault(p =>
                p.specialistId == userId
                && p.studentId == studentId
                && !p.dateFinalized.HasValue);

            if (studentSupportReport != null)
            {
                studentSupportReportFile record = new studentSupportReportFile
                {
                    fileName = reportFile.FileName,
                    dateUploaded = DateTime.UtcNow,
                    studentSupportReportId =
                        studentSupportReport.id,
                    fileContent = reportFile.FileContent,
                    uploadedBy = userId
                };
                Db.studentSupportReportFiles.InsertOnSubmit(record);
                SubmitChanges();
                reportFile.Id = record.id;
            }
            return reportFile;
        }

        public StudentSupportReportFile StudentSupportReportFileGet(int id)
        {
            StudentSupportReportFile result = null;

            studentSupportReportFile studentSupportReportFile = Db.studentSupportReportFiles.FirstOrDefault(p => p.id == id);
            if (studentSupportReportFile != null)
            {
                result = new StudentSupportReportFile
                {
                    Id = studentSupportReportFile.id,
                    DateUploaded = studentSupportReportFile.dateUploaded,
                    FileName = studentSupportReportFile.fileName,
                    StudentSupportReportId = studentSupportReportFile.studentSupportReportId,
                    FileContent = studentSupportReportFile.fileContent
                };
            }
            return result;
        }

        public void StudentSupportReportFileRemove(int id)
        {
            studentSupportReportFile record = Db.studentSupportReportFiles.FirstOrDefault(t => t.id == id);
            if (record == null) return;
            Db.studentSupportReportFiles.DeleteOnSubmit(record);
            SubmitChanges();
        }

        #endregion Attach Documents

        public string StatusGet(int user, int student, int type, int id)
        {
            string status = "";
            switch (type)
            {
                case 1:
                    var data = Db.studentSupportReports.Where(p => p.specialistId == user && p.studentId == student && p.dateFinalized == null).FirstOrDefault();
                    status = string.IsNullOrEmpty(data.status) ? "" : data.status;
                    break;
                case 2:
                    var data2 = Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault();
                    status = string.IsNullOrEmpty(data2.status) ? "" : data2.status;
                    break;
            }
            return status;
        }

        public int StatusSave(int user, int student, string status, int type, int id)
        {
            int row = 0;
            if (type == 1)
            {
                Db.studentSupportReports.Where(p => p.studentId == student && p.specialistId == user).FirstOrDefault().status = status;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentSupportReports.Context.SubmitChanges();
            }
            else if (type == 2)
            {
                Db.studentTeamConsultations.Where(p => p.id == id).FirstOrDefault().status = status;
                row += Db.GetChangeSet().Updates.Count();
                row += Db.GetChangeSet().Inserts.Count();
                Db.studentTeamConsultations.Context.SubmitChanges();
            }
            //data.observations = observations;
            return row;
        }

        public List<TimeRecordFrom> TimeRecordsGet(int user, int student)
        {
            var data = (from it in Db.userTimeRecords
                        where it.studentId == student && it.userId == user
                        select new TimeRecordFrom
                        {
                            Id = it.id,
                            Code = it.code,
                            Date = it.date,
                            Notes = it.notes,
                            Time = it.time
                        }).ToList();
            return data;
        }

        public TimeRecordFrom TimeRecordOneGet(int recordId)
        {
            var data = (from it in Db.userTimeRecords
                        where it.id == recordId
                        select new TimeRecordFrom
                        {
                            Code = it.code,
                            Date = it.date,
                            Id = it.id,
                            Notes = it.notes,
                            Time = it.time
                        }).FirstOrDefault();
            return data;
        }

        public int TimeRecordsSave(TimeRecordFrom form, int user, int student)
        {
            int row;
            if (form.Id == -1)
            {
                userTimeRecord record = new userTimeRecord();
                record.code = form.Code;
                record.date = form.Date;
                record.notes = form.Notes;
                record.studentId = student;
                record.time = form.Time;
                record.userId = user;
                Db.userTimeRecords.InsertOnSubmit(record);
                row = 1;
            }
            else
            {
                var record = (from item in Db.userTimeRecords
                              where item.id == form.Id
                              select item).FirstOrDefault();
                record.code = form.Code;
                record.date = form.Date;
                record.notes = form.Notes;
                record.time = form.Time;
                row = Db.GetChangeSet().Updates.Count();
            }
            Db.userTimeRecords.Context.SubmitChanges();
            return row;
        }

        public int TimeRecordsDelete(int recordId)
        {
            var record = (from item in Db.userTimeRecords
                          where item.id == recordId
                          select item).FirstOrDefault();
            Db.userTimeRecords.DeleteOnSubmit(record);
            int row = Db.GetChangeSet().Updates.Count();
            Db.userTimeRecords.Context.SubmitChanges();
            return row;
        }

        public List<TimeRecordFrom> TimeRecordsOfficeGet(DateRange range, int userId)
        {
            DateTime start, end;
            List<TimeRecordFrom> data;
            if (!string.IsNullOrEmpty(range.startDate) && !string.IsNullOrEmpty(range.endDate))
            {
                start = DateTime.ParseExact(range.startDate, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
                end = DateTime.ParseExact(range.endDate, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
                data = (from it in Db.userTimeRecords
                        where it.studentId == null && it.userId == userId && it.date >= start && it.date <= end
                        select new TimeRecordFrom
                        {
                            Id = it.id,
                            Code = it.code,
                            Date = it.date,
                            Notes = it.notes,
                            Time = it.time
                        }).ToList();
            }
            else if (!string.IsNullOrEmpty(range.startDate) && string.IsNullOrEmpty(range.endDate))
            {
                start = DateTime.ParseExact(range.startDate, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
                data = (from it in Db.userTimeRecords
                        where it.studentId == null && it.userId == userId && it.date >= start
                        select new TimeRecordFrom
                        {
                            Id = it.id,
                            Code = it.code,
                            Date = it.date,
                            Notes = it.notes,
                            Time = it.time
                        }).ToList();
            }
            else if (string.IsNullOrEmpty(range.startDate) && !string.IsNullOrEmpty(range.endDate))
            {
                end = DateTime.ParseExact(range.endDate, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
                data = (from it in Db.userTimeRecords
                        where it.studentId == null && it.userId == userId && it.date <= end
                        select new TimeRecordFrom
                        {
                            Id = it.id,
                            Code = it.code,
                            Date = it.date,
                            Notes = it.notes,
                            Time = it.time
                        }).ToList();
            }
            else
            {
                data = (from it in Db.userTimeRecords
                        where it.studentId == null && it.userId == userId
                        select new TimeRecordFrom
                        {
                            Id = it.id,
                            Code = it.code,
                            Date = it.date,
                            Notes = it.notes,
                            Time = it.time
                        }).ToList();
            }
            return data;
        }

        public int TimeRecordsOfficeSave(TimeRecordFrom form, int userId)
        {
            int row;
            if (form.Id == -1)
            {
                userTimeRecord record = new userTimeRecord();
                record.code = form.Code;
                record.date = form.Date;
                record.notes = form.Notes;
                record.studentId = null;
                record.time = form.Time;
                record.userId = userId;
                Db.userTimeRecords.InsertOnSubmit(record);
                row = 1;
            }
            else
            {
                var record = (from item in Db.userTimeRecords
                              where item.id == form.Id
                              select item).FirstOrDefault();
                record.code = form.Code;
                record.date = form.Date;
                record.notes = form.Notes;
                record.time = form.Time;
                row = Db.GetChangeSet().Updates.Count();
            }
            Db.studentSupportReports.Context.SubmitChanges();
            return row;
        }

        public List<StudentSearchForm> StudentSearchGet(int userId)
        {
            var data = (from item in Db.students
                        join req in Db.studentSupportRequests on item.id equals req.studentId
                        join assign in Db.studentSupportRequestAssignments on req.id equals assign.studentSupportRequestId
                        join sch in Db.schools on item.schoolId equals sch.id
                        where assign.specialistId == userId
                        select new StudentSearchForm
                        {
                            Id = item.id,
                            Grade = item.grade,
                            Name = (item.lastName + ", " + item.firstName),
                            Number = item.studentNumber,
                            School = sch.schoolName,
                            District = sch.organization.districtName,
                            Dob = item.dob
                        }).ToList();
            return data;
        }

        public List<StudentSearchForm> StudentSearchGetParams(string filters, int user)
        {
            List<StudentSearchForm> students = StudentSearchGet(user);
            filters = filters.ToLower();
            string[] filterSplitted = filters.Split('|');
            string field;
            string val;
            string[] buf;
            int numFilter;
            if (filterSplitted.Count() == 1 && int.TryParse(filters, out numFilter))
            {
                students = students.Where(p => p.Grade == filters
                    || p.Number == filters
                    || p.Dob.ToString("yyyy-MM-dd").Contains(filters)).ToList();
            }
            else
            {
                if (filterSplitted.Count() == 1 && filters.Split('=').Count() == 1)
                {
                    students = students.Where(p => p.District.ToLower().Contains(filters)
                        || p.Name.ToLower().Contains(filters)
                        || p.School.ToLower().Contains(filters)).ToList();
                }
                else
                {
                    foreach (string filter in filterSplitted)
                    {
                        buf = filter.Split('=');
                        if (buf.Count() != 2)
                        {
                            return new List<StudentSearchForm>();
                        }
                        else
                        {
                            field = buf[0];
                            val = buf[1];
                            switch (field)
                            {
                                case "name":
                                    students = students.Where(p => p.Name.ToLower().Contains(val)).ToList();
                                    break;
                                case "dob":
                                    students = students.Where(p => p.Dob.ToString("yyyy-MM-dd").ToLower().Contains(val)).ToList();
                                    break;
                                case "number":
                                    students = students.Where(p => p.Number.ToLower() == val).ToList();
                                    break;
                                case "grade":
                                    students = students.Where(p => p.Grade.ToLower() == val).ToList();
                                    break;
                                case "district":
                                    students = students.Where(p => p.District.ToLower().Contains(val)).ToList();
                                    break;
                                case "school":
                                    students = students.Where(p => p.School.ToLower().Contains(val)).ToList();
                                    break;
                                default:
                                    return new List<StudentSearchForm>();
                                    break;
                            }
                        }
                    }
                }
            }
            return students;
        }

        public List<UserShortInfo> OtherSpecialistsGet(int user, int student)
        {
            var data = (from it in Db.studentSupportRequestAssignments
                        where it.studentSupportRequest.studentId == student && it.specialistId != user
                        select new UserShortInfo
                        {
                            Id = it.studentSupportRequest.user.id,
                            FirstName = it.studentSupportRequest.user.firstName.ToString(),
                            LastName = it.studentSupportRequest.user.lastName.ToString(),
                            MiddleName = it.studentSupportRequest.user.middleName.ToString()
                        }).ToList();
            return data;
        }

        public void TeamConsultCreate(int user, int student, string users)
        {
            studentSupportReport report = CurrentSupportReportGet(user, student);
            int consId = (Db.studentTeamConsultations.Count() != 0 ? Db.studentTeamConsultations.Max(p => p.id) + 1 : 1);
            studentTeamConsultation consult = new studentTeamConsultation();
            consult.id = consId;
            consult.studentId = student;
            consult.dateStarted = DateTime.Today;
            consult.dateFinalized = DateTime.Today;
            /*consult.background = report.background;
            consult.observations = report.observations;
            consult.interpretations = report.interpretations;
            consult.goals = report.goals;
            consult.progress = report.progress;
            consult.recommendations = report.recommendations;
            consult.summary = report.summary;
            consult.status = report.status;*/
            Db.studentTeamConsultations.InsertOnSubmit(consult);

            Db.studentTeamConsultations.Context.SubmitChanges();

            studentTeamConsultationMembership member;
            int memberId = (Db.studentTeamConsultationMemberships.Count() != 0 ? Db.studentTeamConsultationMemberships.Max(p => p.id) + 1 : 1);
            List<int> otherUsers = users.Split('_').Select(p => int.Parse(p)).ToList();
            foreach (int spec in otherUsers)
            {
                member = new studentTeamConsultationMembership();
                member.id = memberId;
                member.studentTeamConsultationId = consId;
                member.userId = spec;
                member.finalized = false;
                member.dateFinalized = null;
                Db.studentTeamConsultationMemberships.InsertOnSubmit(member);
                memberId++;
            }

            member = new studentTeamConsultationMembership();
            member.id = memberId;
            member.studentTeamConsultationId = consId;
            member.userId = user;
            member.finalized = false;
            member.dateFinalized = null;
            Db.studentTeamConsultationMemberships.InsertOnSubmit(member);


            Db.studentTeamConsultationMemberships.Context.SubmitChanges();

        }

        public List<TabMenuItem> TabMenuGet(int user, int student)
        {
            List<TabMenuItem> result = new List<TabMenuItem>();
            var current = CurrentSupportReportGet(user, student);
            if (current != null)
            {
                result.Add(new TabMenuItem() { Id = current.id, SubItems = "", Type = 1 });
            }
            var team = Db.studentTeamConsultationMemberships.Where(p => p.userId == user && p.studentTeamConsultation.studentId == student).Select(p => p.studentTeamConsultationId).Distinct();
            TabMenuItem tab;
            List<string> sub;
            foreach (var item in team)
            {
                tab = new TabMenuItem();
                tab.Id = item;
                tab.Type = 2;
                sub = new List<string>();
                foreach (var other in Db.studentTeamConsultationMemberships.Where(p => p.studentTeamConsultationId == item && p.userId != user).ToList())
                {
                    if (other.finalized)
                    {
                        sub.Add("<span class='red'>" + other.user.firstName + " " + other.user.lastName + "</span>");
                    }
                    else
                    {
                        sub.Add(other.user.firstName + " " + other.user.lastName);
                    }
                }
                tab.SubItems = string.Join(", ", sub);
                result.Add(tab);
            }
            return result;
        }

        #endregion Specialist

        #region Organizations

        public List<OrganizationForm> OrganizationsGet()
        {
            return Db.organizations.Select(p => new OrganizationForm
            {
                Id = p.id,
                Title = p.districtName,
                Active = (bool)p.active,
                Address = p.address,
                City = p.city,
                ContactEmail = p.contactEmail,
                ContactFax = p.contactFax,
                ContactName = p.contactName,
                ContactPhone = p.contactPhone,
                PostalCode = p.postalCode,
                Province = p.province,
                SystemUserName = p.systemUserName
            }).ToList();
        }

        public OrganizationForm OrganizationGetForEdit(int id)
        {
            OrganizationForm result = OrganizationsGet().FirstOrDefault(p => p.Id == id);
            if (result != null) //get system user and password. General GET don't need it
            {
                organization org = Db.organizations.FirstOrDefault(p => p.id == id);
                if (org != null)
                {
                    result.SystemPassword = org.systemPassword;
                    result.SystemConfirmPassword = org.systemPassword;
                }
            }
            else
            {
                result = new OrganizationForm();
            }
            return result;
        }

        public OrganizationForm OrganizationUpdate(OrganizationForm organizationForm, int role, int userId)
        {
            organization org = Db.organizations.FirstOrDefault(p => p.id == organizationForm.Id);
            if (org == null) //create
            {
                org = new organization();
                org.active = true;
                Db.organizations.InsertOnSubmit(org);
            }
            org.districtName = organizationForm.Title;
            org.address = organizationForm.Address;
            org.city = organizationForm.City;
            org.province = organizationForm.Province;
            org.postalCode = organizationForm.PostalCode;
            org.contactPhone = organizationForm.ContactPhone;
            org.contactFax = organizationForm.ContactFax;
            org.contactEmail = organizationForm.ContactEmail;
            org.contactName = organizationForm.ContactName;
            org.systemUserName = organizationForm.SystemUserName;
            org.systemPassword = organizationForm.SystemPassword;

            SubmitChanges();
            organizationForm.Id = org.id;

            UserForm user = new UserForm();
            var userOrgFirst = org.userOrganizations.FirstOrDefault();
            if (userOrgFirst != null)
            {
                user = UserGetForEdit(userOrgFirst.user.id);
                //user.Organizations = organizationForm.Id.ToString();
            }
            else
            {
                user = new UserForm();
                user.Id = -1;
                user.PwdChangedDate = DateTime.Now;
                user.Active = true;
                user.UserRole = role;
                user.Organizations = organizationForm.Id.ToString();
            }
            string[] str = org.contactName.Split(' ');
            user.FirstName = str.Any() ? str[0] : string.Empty;
            user.LastName = str.Count() > 1 ? str[1] : user.FirstName;
            user.Email = org.contactEmail;
            user.UserName = org.systemUserName;
            user.Pwd = org.systemPassword;
            UserUpdate(user, userId);

            return organizationForm;
        }

        public void OrganizationRemove(int id)
        {
            organization org = Db.organizations.FirstOrDefault(t => t.id == id);
            if (org == null) return;
            //Db.users.DeleteOnSubmit(user);
            org.active = false;
            SubmitChanges();
        }

        public IQueryable<organizationSetting> OrganizationSettings
        {
            get
            {
                return Db.organizationSettings;
            }
        }

        public void OrganizationSettingsUpdate(OrganizationSettingsForm settingsForm)
        {
            organizationSetting settings = OrganizationSettings.FirstOrDefault(p => p.id == settingsForm.Id);


            settings.sync = settingsForm.Sync;
            settings.powerSchoolUrl = settingsForm.PowerSchoolUrl;
            settings.clientId = settingsForm.ClientId;
            settings.clientSecret = settingsForm.ClientSecret;

            SubmitChanges();
        }

        public OrganizationSettingsForm OrganizationSettingsCreate(int organizationId)
        {
            organizationSetting settings = new organizationSetting();
            Db.organizationSettings.InsertOnSubmit(settings);

            settings.organizationId = organizationId;
            settings.sync = false;

            SubmitChanges();

            return (OrganizationSettingsForm)ModelMapper.Map(settings, typeof(organizationSetting), typeof(OrganizationSettingsForm));


        }

        /*
         public List<UserForm> UserForms
        {
            //TODO: Tolik. First ToList() may be redundant. Check SQL Server
            get { return Users.ToList().Select(u => (UserForm) ModelMapper.Map(u, typeof (user), typeof (UserForm))).ToList(); }
        }
         
         */

        public OrganizationSettingsForm OrganizationSettingsGet(int organizationId)
        {



            return (OrganizationSettingsForm)ModelMapper.Map(OrganizationSettings.FirstOrDefault(p => p.organizationId == organizationId), typeof(organizationSetting), typeof(OrganizationSettingsForm));


        }

        #endregion Organizations

        #region Schools

        public IQueryable<school> Schools
        {
            get
            {
                return Db.schools;
            }
        }

        public List<SchoolFormShort> SchoolsGet()
        {
            return Db.schools.Select(p =>
                                     new SchoolFormShort
                                         {
                                             Id = p.id,
                                             Title =
                                                 string.Format("{0} - {1}", p.organization.districtName, p.schoolName),
                                             OrganizationId = p.organizationId
                                         })
                .ToList();
        }

        /// <summary>
        /// Get schools by district ids array
        /// </summary>
        /// <param name="districtIds">array of district ids. If parameter is empty array returns all schools</param>
        /// <returns>if parameter is empty array returns all schools</returns>
        public List<SchoolFormShort> SchoolsGet(int[] districtIds)
        {
            return SchoolsGet().Where(p => districtIds.Count() == 0 || districtIds.Contains(p.OrganizationId)).ToList();
        }

        #region School User
        public UserActionForm UserActionGet(int user)
        {
            UserActionForm form = Db.userActions.Where(p => p.userId == user).Select(p => new UserActionForm() { CanApprove = p.canApprove, CanRequest = p.canRequest, CanViewReports = p.canViewReports }).FirstOrDefault();
            return form;
        }

        public List<StudentMenuItem> SchoolUserMenuGet(int user)
        {
            int roleArea = Db.roleAreas.Where(p => p.role == 3).FirstOrDefault().ID;
            List<StudentMenuItem> menu = Db.menus.Where(p => p.roleArea == roleArea).OrderBy(p => p.oreder).Select(p => new StudentMenuItem() { href = p.href, level = p.level.ToString(), text = p.Text, title = p.title }).ToList();

            UserActionForm form = UserActionGet(user);
            if (!form.CanRequest)
                menu.Remove(menu.Where(p => p.href == Enum.GetName(typeof(ActionUsers), 1)).FirstOrDefault());
            if (!form.CanApprove)
                menu.Remove(menu.Where(p => p.href == Enum.GetName(typeof(ActionUsers), 2)).FirstOrDefault());
            if (!form.CanViewReports)
                menu.Remove(menu.Where(p => p.href == Enum.GetName(typeof(ActionUsers), 3)).FirstOrDefault());
            return menu;
        }

        public List<UserShortInfo> StudentsForCurrentSchoolOrg(int user)
        {
            userOrganization userOrg = Db.userOrganizations.Where(p => p.userId == user).FirstOrDefault();
            List<UserShortInfo> result;
            if (userOrg.schoolId != null)
            {
                result = Db.students.Where(p => p.schoolId == userOrg.schoolId).Select(p => new UserShortInfo() { Id = p.id, FirstName = p.firstName, LastName = p.lastName, MiddleName = p.middleName }).ToList();
            }
            else
            {
                List<int> schools = Db.schools.Where(p => p.organizationId == userOrg.organizationId).Select(p => p.id).ToList();
                result = Db.students.Where(p => schools.Contains(p.schoolId)).Select(p => new UserShortInfo() { Id = p.id, FirstName = p.firstName, LastName = p.lastName, MiddleName = p.middleName }).ToList();
            }
            return result;
        }

        public List<SchoolForm> SchoolsGetByAdminId(int organizationId)
        {

            List<SchoolForm> result = new List<SchoolForm>();

            result = (from s in Schools
                      where s.organizationId == organizationId
                      select new SchoolForm
                      {
                          Id = s.id,
                          OrganizationId = s.organizationId,
                          SchoolName = s.schoolName,
                          Address = s.address,
                          City = s.city,
                          Province = s.province,
                          PostalCode = s.postalCode,
                          Phone = s.phone,
                          Fax = s.fax,
                          Email = s.email,
                          Website = s.website,
                          SourceId = s.sourceId

                      }).ToList();
            return result;


        }


        private enum ActionUsers : int
        {
            RequestSupport = 1,
            ApproveSupport = 2,
            StudentReports = 3
        }

        /*public List<IdTitle> SupportRequestTypesGet()
        {
            List<IdTitle> types = Db.supportRequestTypes.Select(p => new IdTitle() { Id = p.id, Title = p.code }).ToList();
            return types;
        }*/

        public CurrentSchoolUsers CurrentDistrictSchoolUsers(int user, int role)
        {
            userOrganization userOrg = Db.userOrganizations.Where(p => p.userId == user).FirstOrDefault();
            if (userOrg == null)
                return new CurrentSchoolUsers() { districtUser = new List<UserShortInfo>(), schoolUsers = new List<UserShortInfo>() };
            CurrentSchoolUsers users = new CurrentSchoolUsers();
            users.districtUser = new List<UserShortInfo>();
            users.schoolUsers = new List<UserShortInfo>();

            var userList = Db.userOrganizations.Where(p => p.organizationId == userOrg.organizationId && p.user.userRoles.First().roleId == role && p.user.active).OrderBy(p => p.user.lastName).ThenBy(p => p.user.firstName).ThenBy(p => p.user.middleName).ToList();//.Select(p => new UserShortInfo() { Id = p.userId, FirstName = p.user.firstName, LastName = p.user.lastName, MiddleName = p.user.middleName }).ToList();

            if (userOrg.organizationId != null)
            {
                users.schoolUsers = userList.Where(p => p.organizationId == userOrg.organizationId).Select(p => new UserShortInfo() { Id = p.userId, FirstName = p.user.firstName, LastName = p.user.lastName, MiddleName = p.user.middleName }).ToList();
            }
            users.districtUser = userList.Where(p => p.organizationId == null).Select(p => new UserShortInfo() { Id = p.userId, FirstName = p.user.firstName, LastName = p.user.lastName, MiddleName = p.user.middleName }).ToList();

            return users;
        }

        public Dictionary<int, List<UserShortInfoDistrict>> AllUsersGet(List<int> types, int role)
        {
            Dictionary<int, List<UserShortInfoDistrict>> result = new Dictionary<int, List<UserShortInfoDistrict>>();
            foreach (var type in types)
            {
                List<UserShortInfoDistrict> users = new List<UserShortInfoDistrict>();
                users = Db.users.Where(p => p.userRoles.FirstOrDefault() != null && p.userRoles.FirstOrDefault().roleId == role && p.userSupportTypes.ToList().Any() && p.userSupportTypes.Where(t=>t.supportRequestTypeId == type).Any()).Select(p => new UserShortInfoDistrict()
                {
                    Id = p.id,
                    FirstName = p.FullName,
                    District =
                        p.userOrganizations.Any() ? p.userOrganizations.FirstOrDefault().organization.districtName : "",
                    School =
                        p.userOrganizations.Any()
                            ? p.userOrganizations.FirstOrDefault().schoolId != null
                                ? p.userOrganizations.FirstOrDefault().school.schoolName
                                : ""
                            : ""
                }).ToList().OrderBy(p => p.Order).ThenBy(p => p.District).ThenBy(p => p.School).ThenBy(p => p.FirstName).ToList();
                result.Add(type, users);
            }

            return result;
        }

        public StudentSupportRequestForm StudentSupportRequestSave(StudentSupportRequestForm form, int user)
        {
            bool wasSaved = true;
            studentSupportRequest request;
            request = Db.studentSupportRequests.FirstOrDefault(p => p.id == form.Id);
            int row = 0;
            if (request == null)
            {
                request = new studentSupportRequest()
                {
                    approved = false,
                    denied = false,
                    whoApproved = null,
                    dateApproved = null,
                    studentId = form.StudentId,
                    requestedBy = user,
                    dateRequested = DateTime.Today,
                    reason = form.approvalNote,
                    submitted = form.Submitted
                };
                Db.studentSupportRequests.InsertOnSubmit(request);
            }
            else
            {
                request.studentId = form.StudentId;
                request.reason = form.approvalNote;
                request.submitted = form.Submitted;
                row += 1;
            }
            row += Db.GetChangeSet().Inserts.Count();
            row += Db.GetChangeSet().Updates.Count();
            Db.studentSupportRequests.Context.SubmitChanges();

            //if (row == 0) return;
            int id = request.id;
            wasSaved = StudentSupportRequestNotificationsSave(form.Users.Split('_').ToList().Select(p => int.Parse(p)).ToList(), id);
            //if (!wasSaved) return;

            wasSaved = StudentSupportRequestTypesSave(form.Types.Split('_').ToList().Select(p => int.Parse(p)).ToList(), id);
            //if (!wasSaved) return;

            StudentSupportRequestFilesSave(form.SelectedFile, id);

            return RequestFormGetOne(request.id);
        }

        public StudentSupportRequestForm RequestFormGetOne(int id)
        {
            if (id == -1)
            {
                return new StudentSupportRequestForm() { Id = -1 };
            }
            else
            {
                studentSupportRequest data = Db.studentSupportRequests.FirstOrDefault(p => p.id == id);
                if (data == null)
                {
                    return new StudentSupportRequestForm() { Id = -1 };
                }
                //StudentSupportRequestForm form = data.Select(p => new StudentSupportRequestForm()
                //{
                //    Id = p.id,
                //    StudentId = p.studentId,
                //    Types = string.Join("_", p.studentSupportRequestTypes.Select(t => t.supportRequestTypeId.ToString()).Distinct().ToList()),
                //    Users = string.Join("_", p.studentSupportRequestNotifications.Select(t => t.userId).ToList()),
                //    Reason = p.reason,
                //    SelectedFile = p.studentSupportRequestFiles.Any() ? new UploadedFile() {Content = p.studentSupportRequestFiles.FirstOrDefault().fileContent, Name = p.studentSupportRequestFiles.FirstOrDefault().fileName} : null
                //}).FirstOrDefault();
                StudentSupportRequestForm form = new StudentSupportRequestForm();
                form.Id = data.id;
                form.StudentId = data.studentId;
                form.Types = data.studentSupportRequestTypes.Any() ? string.Join("_", data.studentSupportRequestTypes.Select(t => t.supportRequestTypeId.ToString()).Distinct().ToList()) : "";
                form.Users = data.studentSupportRequestNotifications.Any() ? string.Join("_", data.studentSupportRequestNotifications.Select(t => t.userId).Distinct().ToList()) : "";
                form.SelectedFile = data.studentSupportRequestFiles.Any()
                    ? new UploadedFile()
                    {
                        Id = data.studentSupportRequestFiles.FirstOrDefault().id,
                        Content = data.studentSupportRequestFiles.FirstOrDefault().fileContent,
                        Name = data.studentSupportRequestFiles.FirstOrDefault().fileName
                    }
                    : new UploadedFile() { Id = -1 };
                form.Reason = data.reason;
                return form;
            }
        }

        private bool StudentSupportRequestNotificationsSave(List<int> users, int request)
        {
            int rowCount = 0;
            foreach (int user in users)
            {
                List<studentSupportRequestNotification> notififcationRemove = Db.studentSupportRequestNotifications.Where(p => p.studentSupportRequestId == request).ToList();
                foreach (studentSupportRequestNotification req in notififcationRemove)
                {
                    Db.studentSupportRequestNotifications.DeleteOnSubmit(req);
                }
                SubmitChanges();
                studentSupportRequestNotification notififcation = new studentSupportRequestNotification()
                {
                    studentSupportRequestId = request,
                    userId = user
                };
                Db.studentSupportRequestNotifications.InsertOnSubmit(notififcation);
                rowCount += Db.GetChangeSet().Inserts.Count();
            }
            Db.studentSupportRequestNotifications.Context.SubmitChanges();
            return rowCount > 0;
        }

        private bool StudentSupportRequestTypesSave(List<int> types, int request)
        {
            int rowCount = 0;
            foreach (int typeId in types)
            {
                List<studentSupportRequestType> notififcationRemove = Db.studentSupportRequestTypes.Where(p => p.studentSupportRequestId == request).ToList();
                foreach (studentSupportRequestType req in notififcationRemove)
                {
                    Db.studentSupportRequestTypes.DeleteOnSubmit(req);
                }
                SubmitChanges();
                studentSupportRequestType type = new studentSupportRequestType()
                {
                    studentSupportRequestId = request,
                    supportRequestTypeId = typeId
                };
                Db.studentSupportRequestTypes.InsertOnSubmit(type);
                rowCount += Db.GetChangeSet().Inserts.Count();
            }
            Db.studentSupportRequestTypes.Context.SubmitChanges();
            return rowCount > 0;
        }

        private bool StudentSupportRequestFilesSave(UploadedFile uploadedFile, int request)
        {
            int rowCount = 0;
            if (uploadedFile == null)
            {
                studentSupportRequestFile fileRemove = Db.studentSupportRequestFiles.FirstOrDefault(p => p.studentSupportRequestId == request);
                if (fileRemove != null)
                {
                    Db.studentSupportRequestFiles.DeleteOnSubmit(fileRemove);
                    SubmitChanges();
                }
                return false;
            }
            studentSupportRequestFile file = Db.studentSupportRequestFiles.FirstOrDefault(p => p.studentSupportRequestId == request) ?? new studentSupportRequestFile();
            file.dateUploaded = DateTime.Now;
            file.fileContent = uploadedFile.Content;
            file.fileName = uploadedFile.Name;
            file.studentSupportRequestId = request;
            if (file.id == 0)
                Db.studentSupportRequestFiles.InsertOnSubmit(file);
            rowCount += Db.GetChangeSet().Inserts.Count();
            rowCount += Db.GetChangeSet().Updates.Count();
            SubmitChanges();
            return rowCount > 0;
        }

        public void StudentSupportRequestDelete(int id)
        {
            try
            {
                studentSupportRequest request = Db.studentSupportRequests.FirstOrDefault(p => p.id == id);
                if (request == null)
                {
                    return;
                }
                foreach (var item in request.studentSupportReports)
                {
                    Db.studentSupportReports.DeleteOnSubmit(item);
                }
                foreach (var item in request.studentSupportRequestAssignments)
                {
                    Db.studentSupportRequestAssignments.DeleteOnSubmit(item);
                }
                foreach (var item in request.studentSupportRequestFiles)
                {
                    Db.studentSupportRequestFiles.DeleteOnSubmit(item);
                }
                foreach (var item in request.studentSupportRequestNotifications)
                {
                    Db.studentSupportRequestNotifications.DeleteOnSubmit(item);
                }
                foreach (var item in request.studentSupportRequestTypes)
                {
                    Db.studentSupportRequestTypes.DeleteOnSubmit(item);
                }
                Db.studentSupportRequests.DeleteOnSubmit(request);
                SubmitChanges();
            }
            catch (Exception)
            {
                { }
                throw;
            }

        }

        public NewRequestEmail NewRequestEmailDataGet(int user, int student)
        {
            NewRequestEmail data = new NewRequestEmail();
            data.User = Db.users.Where(p => p.id == user).Select(p => p.lastName + " " + p.firstName).FirstOrDefault();
            data.School = Db.students.Where(p => p.id == student).Select(p => p.school.schoolName).FirstOrDefault();
            var org = Db.userOrganizations.Where(p => p.userId == user).FirstOrDefault();
            List<string> emails = new List<string>();
            if (org.schoolId != null)
            {
                emails = Db.users.Where(p => p.userOrganizations.FirstOrDefault().schoolId == org.schoolId && p.userOrganizations.FirstOrDefault().organizationId == org.organizationId
                    && p.userActions.FirstOrDefault().canApprove).Select(p => p.email).ToList();
            }
            else
            {
                emails = Db.users.Where(p => p.userOrganizations.FirstOrDefault().organizationId == org.organizationId
                    && p.userActions.FirstOrDefault().canApprove).Select(p => p.email).ToList();
            }
            data.Emails = emails;
            return data;
        }

        public List<StudentSupportRequestSchortForm> ApprovalRequestsGet(int user)
        {
            userOrganization userOrg = Db.userOrganizations.Where(p => p.userId == user).FirstOrDefault();
            List<int> users = Db.userOrganizations.Where(p => p.organizationId == userOrg.organizationId && p.organizationId == userOrg.organizationId).Select(p => p.userId).ToList();
            var data = Db.studentSupportRequests.Where(p => users.Contains(p.requestedBy) && !p.approved && !p.denied);
            List<StudentSupportRequestSchortForm> requests = data.Select(p => new StudentSupportRequestSchortForm()
            {
                Id = p.id,
                StudentName = p.student.firstName + ", " + p.student.lastName,// + (!string.IsNullOrEmpty(p.student.middleName) ? " " + p.student.middleName : ""),
                DateOfRequest = p.dateRequested,
                RequestedBy = p.user.firstName + " " + p.user.lastName,
                Specialities = string.Join(", ", p.studentSupportRequestTypes.Select(t => t.supportRequestType.code).ToList()),
                Note = p.approvalNote
            }).ToList();
            return requests;
        }

        public StudentSupportRequestSchortForm ApprovalRequestOneGet(int id)
        {
            StudentSupportRequestSchortForm requests = Db.studentSupportRequests.Where(p => p.id == id).Select(p => new StudentSupportRequestSchortForm()
            {
                Id = p.id,
                StudentName = p.student.firstName + ", " + p.student.lastName,// + (!string.IsNullOrEmpty(p.student.middleName) ? " " + p.student.middleName : ""),
                DateOfRequest = p.dateRequested,
                RequestedBy = p.user.firstName + " " + p.user.lastName,
                Specialities = string.Join(", ", p.studentSupportRequestTypes.Select(t => t.supportRequestType.code).ToList()),
                Note = p.approvalNote,
                file = p.studentSupportRequestFiles.Select(f => new UploadedFile() { Name = f.fileName, Content = f.fileContent }).FirstOrDefault(),
                Reason = p.reason
            }).FirstOrDefault();
            return requests;
        }

        public UploadedFile ApprovalRequestFileGet(int id)
        {
            UploadedFile file = Db.studentSupportRequestFiles.Where(p => p.studentSupportRequestId == id).Select(p => new UploadedFile()
            {
                Content = p.fileContent,
                Name = p.fileName
            }).FirstOrDefault();
            return file;
        }

        public bool StudentRequestDeny(int requestId)
        {
            var requerst = Db.studentSupportRequests.FirstOrDefault(p => p.id == requestId);
            if (requerst == null)
                return false;
            requerst.denied = true;
            int row = Db.GetChangeSet().Updates.Count();
            Db.studentSupportRequests.Context.SubmitChanges();
            return row > 0;
        }

        public bool StudentRequestApprove(int requestId, string Note)
        {
            var requerst = Db.studentSupportRequests.FirstOrDefault(p => p.id == requestId);
            if (requerst == null)
                return false;
            requerst.approved = true;
            requerst.approvalNote = Note;
            int row = Db.GetChangeSet().Updates.Count();
            Db.studentSupportRequests.Context.SubmitChanges();
            return row > 0;
        }

        public List<SchoolStudentsSearch> StudentOrgSchoolGet(int user)
        {
            var org = Db.userOrganizations.FirstOrDefault(p => p.userId == user);
            if (org == null)
                return new List<SchoolStudentsSearch>();
            List<SchoolStudentsSearch> students = new List<SchoolStudentsSearch>();
            if (org.schoolId != null)
            {
                students = Db.students.Where(p => p.schoolId == org.schoolId).Select(p => new SchoolStudentsSearch()
                {
                    Id = p.id,
                    School = p.school.schoolName,
                    Name = p.firstName + " " + p.lastName,
                    Dob = p.dob,
                    District = p.school.organization.districtName,
                    Grade = p.grade,
                    Gender = p.gender.ToString().ToLower() == "m" ? "Male" : "Female",
                    Services = p.studentSupportRequests.Any() ? string.Join(", ", p.studentSupportRequests.FirstOrDefault().studentSupportRequestTypes.Select(s => s.supportRequestType.code).ToList()) : ""
                }).ToList();
            }
            return students;
        }

        public List<SchoolStudentsSearch> StudentOrgSchoolGetParams(string filters, int user)
        {
            List<SchoolStudentsSearch> students = StudentOrgSchoolGet(user);
            filters = filters.ToLower();
            string[] filterSplitted = filters.Split('|');
            string field;
            string val;
            string[] buf;
            int numFilter;
            if (filterSplitted.Count() == 1 && int.TryParse(filters, out numFilter))
            {
                students = students.Where(p => p.Grade == filters
                    || p.Number == filters
                    || p.Dob.ToString("yyyy-MM-dd").Contains(filters)).ToList();
            }
            else
            {
                if (filterSplitted.Count() == 1 && filters.Split('=').Count() == 1)
                {
                    students = students.Where(p => p.District.ToLower().Contains(filters)
                        || p.Name.ToLower().Contains(filters)
                        || p.School.ToLower().Contains(filters)
                        || p.Gender.ToLower() == filters).ToList();
                }
                else
                {
                    foreach (string filter in filterSplitted)
                    {
                        buf = filter.Split('=');
                        if (buf.Count() != 2)
                        {
                            return new List<SchoolStudentsSearch>();
                        }
                        else
                        {
                            field = buf[0];
                            val = buf[1];
                            switch (field)
                            {
                                case "name":
                                    students = students.Where(p => p.Name.ToLower().Contains(val)).ToList();
                                    break;
                                case "dob":
                                    students = students.Where(p => p.Dob.ToString("yyyy-MM-dd").ToLower().Contains(val)).ToList();
                                    break;
                                case "number":
                                    students = students.Where(p => p.Number.ToLower() == val).ToList();
                                    break;
                                case "grade":
                                    students = students.Where(p => p.Grade.ToLower() == val).ToList();
                                    break;
                                case "district":
                                    students = students.Where(p => p.District.ToLower().Contains(val)).ToList();
                                    break;
                                case "school":
                                    students = students.Where(p => p.School.ToLower().Contains(val)).ToList();
                                    break;
                                default:
                                    return new List<SchoolStudentsSearch>();
                                    break;
                            }
                        }
                    }
                }
            }
            return students;
        }

        public StudentFullInfo StudentReportGet(int student, int type, int id)
        {
            StudentFullInfo info = new StudentFullInfo();
            info.MainInfo = StudentGet(student);
            StudentReport report = new StudentReport();
            List<IdTitleDescription> services = new List<IdTitleDescription>();

            switch (type)
            {
                case 1:
                    report = Db.studentSupportReports.Where(p => p.dateFinalized == null && p.studentId == student).Select(p => new StudentReport()
                    {
                        Id = p.id,
                        Background = p.background,
                        Goals = p.goals,
                        Observations = p.observations,
                        Progress = p.progress,
                        Recomendations = p.recommendations,
                        Status = p.status,
                        Summary = p.summary,
                        Test = p.interpretations,
                        ConsultNames = p.studentSupportRequest.user.FullName,

                    }).FirstOrDefault();
                    services = Db.studentSupportReports.Where(p => p.dateFinalized == null && p.studentId == student).Select(p => new IdTitleDescription()
                    {
                        Id = p.id,
                        Title = string.Join(", ", p.studentSupportRequest.studentSupportRequestTypes.Select(t => t.supportRequestType.code).Distinct().ToList()),
                        Description = p.studentSupportRequest.user.FullName
                    }).ToList();
                    break;
                case 2:
                    report = Db.studentTeamConsultations.Where(p => p.id == id).Select(p => new StudentReport()
                    {
                        Id = p.id,
                        Background = p.background,
                        Goals = p.goals,
                        Observations = p.observations,
                        Progress = p.progress,
                        Recomendations = p.recommendations,
                        Status = p.status,
                        Summary = p.summary,
                        Test = p.interpretations,
                        ConsultNames = string.Join("; ", p.studentTeamConsultationMemberships.Select(q => q.user.FullName))
                    }).FirstOrDefault();
                    services =
                        Db.studentTeamConsultationMemberships.Where(p => p.studentTeamConsultationId == id).Select(
                            p => new IdTitleDescription
                                     {
                                         Id = p.id,
                                         Title = p.user.userSupportTypes.Any() ? p.user.userSupportTypes.FirstOrDefault().supportRequestType.name : string.Empty,
                                         Description = p.user.FullName
                                     }).ToList();
                    break;
                case 3:
                    report = Db.studentSupportReports.Where(p => p.id == id).Select(p => new StudentReport()
                    {
                        Id = p.id,
                        Background = p.background,
                        Goals = p.goals,
                        Observations = p.observations,
                        Progress = p.progress,
                        Recomendations = p.recommendations,
                        Status = p.status,
                        Summary = p.summary,
                        Test = p.interpretations,
                        ConsultNames = p.studentSupportRequest.user.FullName
                    }).FirstOrDefault();
                    services = Db.studentSupportReports.Where(p => p.dateFinalized == null && p.studentId == student).Select(p => new IdTitleDescription()
                    {
                        Id = p.id,
                        Title = string.Join(", ", p.studentSupportRequest.studentSupportRequestTypes.Select(t => t.supportRequestType.code).Distinct().ToList()),
                        Description = p.studentSupportRequest.user.FullName
                    }).ToList();
                    break;
            }
            info.Background = report.Background;
            info.Goals = report.Goals;
            info.Observations = report.Observations;
            info.Progress = report.Progress;
            info.Recomendations = report.Recomendations;
            info.Summary = report.Summary;
            info.Status = report.Status;
            info.Test = report.Test;
            info.Files = StudentFilesGet(report.Id, type);
            info.Services = services;
            return info;
        }

        public List<UploadedFile> StudentFilesGet(int report, int type)
        {
            List<UploadedFile> data = new List<UploadedFile>();
            switch (type)
            {
                case 1:
                case 3:
                    data = Db.studentSupportReportFiles.Where(p => p.studentSupportReportId == report).Select(p => new UploadedFile()
                    {
                        Id = p.id,
                        Content = p.fileContent,
                        Name = p.fileName,
                        UploadedBy = Db.users.Where(u => u.id == p.studentSupportReport.specialistId).Select(u => u.firstName + " " + u.lastName).FirstOrDefault(),
                        UploadedDate = p.dateUploaded
                    }).ToList();
                    break;
                case 2:
                    data = Db.studentTeamConsultationFiles.Where(p => p.studentTeamConsultationId == report).Select(p => new UploadedFile()
                    {
                        Id = p.id,
                        Content = p.fileContent,
                        Name = p.fileName,
                        UploadedBy = "",
                        UploadedDate = p.studentTeamConsultation.dateStarted
                    }).ToList();
                    break;
            }
            return data;
        }

        public UploadedFile StudentFileGet(int id)
        {
            UploadedFile file = Db.studentSupportReportFiles.Where(p => p.id == id).Select(p => new UploadedFile()
            {
                Content = p.fileContent,
                Name = p.fileName
            }).FirstOrDefault();
            return file;
        }

        public List<TabMenuItem> StudentReportsGet(int studentId)
        {
            List<TabMenuItem> result = new List<TabMenuItem>();
            TabMenuItem item = new TabMenuItem() { Id = 0, Type = 1, SubItems = "Current" };
            List<TabMenuItem> history = Db.studentSupportReports.Where(p => p.studentId == studentId && p.dateFinalized != null).Select(p => new TabMenuItem
                {
                    Id = p.id,
                    Type = 3,
                    SubItems = ((DateTime)p.dateFinalized).ToString("MMM dd, yyyy", System.Globalization.CultureInfo.InvariantCulture)
                }).ToList();
            List<TabMenuItem> consultings = Db.studentTeamConsultations.Where(p => p.studentId == studentId && p.dateFinalized != null).Select(p => new TabMenuItem
            {
                Id = p.id,
                Type = 2,
                SubItems = /*"Team " + */((DateTime)p.dateFinalized).ToString("MMM dd, yyyy", System.Globalization.CultureInfo.InvariantCulture)// + " - "  
                //string.Join(", ", p.studentTeamConsultationMemberships.AsEnumerable().Select(u => u.user.firstName + " " + u.user.lastName).ToList())
            }).ToList();
            result.Add(item);
            result.AddRange(history);
            result.AddRange(consultings);
            return result;
        }

        #endregion School User

        public SchoolForm SchoolUpdate(SchoolForm schoolForm)
        {
            school school = Schools.FirstOrDefault(p => p.id == schoolForm.Id);
            if (school == null)
            {
                school = new school();
                Db.schools.InsertOnSubmit(school);
            }
            school.schoolName = schoolForm.SchoolName;
            school.organizationId = schoolForm.OrganizationId;
            school.address = schoolForm.Address;
            school.city = schoolForm.City;
            school.province = schoolForm.Province;
            school.postalCode = schoolForm.PostalCode;
            school.phone = schoolForm.Phone;
            school.fax = schoolForm.Fax;
            school.email = schoolForm.Email;
            school.website = schoolForm.Website;
            school.sourceId = schoolForm.SourceId;

            SubmitChanges();
            schoolForm.Id = school.id;
            return schoolForm;
        }

        #endregion Schools

        #region Support Request Types

        public List<IdTitleDescription> SupportRequestTypesGet()
        {
            return Db.supportRequestTypes.Select(p => new IdTitleDescription
                                                          {
                                                              Id = p.id,
                                                              Title = p.name,
                                                              Description = p.code
                                                          }).ToList();
        }

        #endregion Support Request Types

        #region Settings

        public List<SettingsForm> SettingsGet()
        {
            List<SettingsForm> result = Db.settings.Select(p => new SettingsForm
                                                                    {
                                                                        Id = p.Id,
                                                                        Hours = p.Hours,
                                                                        Month = p.Month,
                                                                        Year = p.Year
                                                                    }).ToList();
            return result;
        }

        public SettingsForm SettingsUpdate(SettingsForm settingsForm)
        {
            setting record = Db.settings.FirstOrDefault(t => t.Id == settingsForm.Id);
            if (record == null)
            {
                record = new setting();
                Db.settings.InsertOnSubmit(record);
            }
            record.Hours = settingsForm.Hours;
            record.Month = settingsForm.Month;
            record.Year = settingsForm.Year;
            SubmitChanges();
            settingsForm.Id = record.Id;
            return settingsForm;
        }

        public void SettingRemove(int id)
        {
            setting record = Db.settings.FirstOrDefault(p => p.Id == id);
            if (record == null) return;
            Db.settings.DeleteOnSubmit(record);
            SubmitChanges();
        }

        #endregion Settings

        #region Students

        public StudentForm StudentUpdate(StudentForm studentForm, string updatedBy)
        {
            student student = Db.students.FirstOrDefault(p => p.id == studentForm.Id);
            if (student == null)
            {
                student = new student();
                student.createdDate = DateTime.UtcNow;
                Db.students.InsertOnSubmit(student);
            }
            student.schoolId = studentForm.SchoolId;
            student.studentNumber = studentForm.StudentNumber;

            student.firstName = studentForm.FirstName;
            student.lastName = studentForm.LastName;
            student.middleName = studentForm.MiddleName;

            student.gender = studentForm.Gender;
            student.dob = studentForm.Dob;
            student.grade = studentForm.Grade;

            student.code = studentForm.Code;
            student.specialPrograms = studentForm.SpecialPrograms;
            student.homePhone = studentForm.HomePhone;

            student.mailingAddress = studentForm.MailingAddress;
            student.mailingCity = studentForm.MailingCity;
            student.mailingProvince = studentForm.MailingProvince;
            student.mailingPostalCode = studentForm.MailingPostalCode;

            student.address = studentForm.Address;
            student.city = studentForm.City;
            student.province = studentForm.Province;
            student.postalCode = studentForm.PostalCode;

            student.motherName = studentForm.MotherName;
            student.motherPhone = studentForm.MotherPhone;
            student.motherEmail = studentForm.MotherEmail;

            student.fatherName = studentForm.FatherName;
            student.fatherPhone = studentForm.FatherPhone;
            student.fatherEmail = studentForm.FatherEmail;

            student.guardianName = studentForm.GuardianName;
            student.guardianPhone = studentForm.GuardianPhone;
            student.guardianEmail = studentForm.GuardianEmail;

            student.sourceId = studentForm.SourceId;
            student.updatedDate = DateTime.UtcNow;
            student.updatedBy = updatedBy;

            student.active = studentForm.Active;

            SubmitChanges();
            studentForm.Id = student.id;
            return studentForm;
        }

        #endregion Students

        #region District Sys Admin

        public List<IdTitle> ImportSchools(string[] fileStrings, int organizationId)
        {
            List<IdTitle> result = new List<IdTitle>();
            int i = 0;
            foreach (string line in fileStrings)
            {
                try
                {
                    string[] items = line.Split('\t');
                    if (items.Count() != 10)
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Fields count doesn't match. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[0]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. School name is not provided. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[9]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Source id is not provided. Skipped.", i));
                    }
                    school schoolFromBase = Schools.FirstOrDefault(p => p.schoolName.Equals(items[0]));
                    bool created = schoolFromBase == null;
                    SchoolForm schoolForm = new SchoolForm
                                                {
                                                    Id = schoolFromBase != null ? schoolFromBase.id : -1,
                                                    SchoolName = items[0],
                                                    OrganizationId = organizationId,
                                                    Address = items[1],
                                                    City = items[2],
                                                    Province = items[3],
                                                    PostalCode = items[4],
                                                    Phone = items[5],
                                                    Fax = items[6],
                                                    Email = items[7],
                                                    Website = items[8],
                                                    SourceId = items[9]
                                                };
                    schoolForm = SchoolUpdate(schoolForm);
                    result.Add(new IdTitle { Id = i, Title = string.Format("School \"{0}\" {1}", items[0], created ? "inserted" : "updated") });
                }
                catch (ArgumentNullException exc)
                {
                    result.Add(new IdTitle { Id = i, Title = exc.Message });
                }
                catch (Exception exc)
                {
                    result.Add(new IdTitle { Id = i, Title = string.Format("Line # {0}. {1}", i, exc.Message) });
                }
                i++;
            }

            return result;
        }

        public List<IdTitle> ImportStudents(string[] fileStrings, int organizationId, string updatedBy)
        {
            List<IdTitle> result = new List<IdTitle>();
            int i = 0;
            foreach (string line in fileStrings)
            {
                try
                {
                    string[] items = line.Split('\t');
                    if (items.Count() <= 9)
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Fields count is low. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[0]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Source id is not provided. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[2]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Student Number is not provided. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[3]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. FirstName is not provided. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[4]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. LastName is not provided. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[6]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Gender is not provided. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[7]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. DOB is not provided. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[8]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Grade is not provided. Skipped.", i));
                    }
                    student studentFromBase =
                        Db.students.FirstOrDefault(
                            p =>
                            p.sourceId.Equals(items[0]) && p.firstName.Equals(items[3]) && p.lastName.Equals(items[4]) &&
                            p.dob == DateTime.Parse(items[7]));
                    bool created = studentFromBase == null;
                    school school = Schools.FirstOrDefault(p => p.sourceId.Equals(items[0]));
                    if (school == null)
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. School source id not found. Skipped.", i));
                    }
                    StudentForm studentForm = new StudentForm
                    {
                        Id = studentFromBase != null ? studentFromBase.id : -1,
                        SourceId = items[0],
                        SchoolId = school.id,
                        Active = bool.Parse(items[1]),
                        StudentNumber = items[2],
                        FirstName = items[3],
                        LastName = items[4],
                        MiddleName = items[5],
                        Gender = items[6].ToCharArray()[0],
                        Dob = DateTime.Parse(items[7]),
                        Grade = items[8],
                        Code = items[9],
                        SpecialPrograms = items.Count() > 10 ? items[10] : string.Empty,
                        HomePhone = items.Count() > 11 ? items[11] : string.Empty,

                        MailingAddress = items.Count() > 12 ? items[12] : string.Empty,
                        MailingCity = items.Count() > 13 ? items[13] : string.Empty,
                        MailingProvince = items.Count() > 14 ? items[14] : string.Empty,
                        MailingPostalCode = items.Count() > 15 ? items[15] : string.Empty,

                        Address = items.Count() > 16 ? items[16] : string.Empty,
                        City = items.Count() > 17 ? items[17] : string.Empty,
                        Province = items.Count() > 18 ? items[18] : string.Empty,
                        PostalCode = items.Count() > 19 ? items[19] : string.Empty,

                        MotherName = items.Count() > 20 ? items[20] : string.Empty,
                        MotherPhone = items.Count() > 21 ? items[21] : string.Empty,
                        MotherEmail = items.Count() > 22 ? items[22] : string.Empty,

                        FatherName = items.Count() > 23 ? items[23] : string.Empty,
                        FatherPhone = items.Count() > 24 ? items[24] : string.Empty,
                        FatherEmail = items.Count() > 25 ? items[25] : string.Empty,

                        GuardianName = items.Count() > 26 ? items[26] : string.Empty,
                        GuardianPhone = items.Count() > 27 ? items[27] : string.Empty,
                        GuardianEmail = items.Count() > 28 ? items[28] : string.Empty
                    };
                    studentForm = StudentUpdate(studentForm, updatedBy);
                    result.Add(new IdTitle { Id = i, Title = string.Format("Student \"{0}\" {1} {2}", items[4], items[3], created ? "inserted" : "updated") });
                }
                catch (ArgumentNullException exc)
                {
                    result.Add(new IdTitle { Id = i, Title = exc.Message });
                }
                catch (Exception exc)
                {
                    result.Add(new IdTitle { Id = i, Title = string.Format("Line # {0}. {1}", i, exc.Message) });
                }
                i++;
            }

            return result;
        }

        public List<IdTitle> ImportStaff(string[] fileStrings, int userRoleId, int organizationId, int currentUserId)
        {
            List<IdTitle> result = new List<IdTitle>();
            int i = 0;
            foreach (string line in fileStrings)
            {
                try
                {
                    string[] items = line.Split('\t');
                    if (items.Count() <= 8)
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Fields count is low. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[2]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. First name id is not provided. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[3]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Last name is not provided. Skipped.", i));
                    }
                    bool b;
                    if (string.IsNullOrWhiteSpace(items[8]) && !bool.TryParse(items[8], out b))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. Active is not provided or in incorrect format. Skipped.", i));
                    }
                    if (string.IsNullOrWhiteSpace(items[9]))
                    {
                        throw new ArgumentNullException(string.Format("Line # {0}. PsUserId is not provided. Skipped.", i));
                    }

                    school school = Schools.FirstOrDefault(p => p.sourceId.Equals(items[0]));
                    user userFromBase = null;
                    List<user> users = Users.ToList();
                    foreach (user user in users)
                    {
                        if (user.ps_user_id.HasValue && user.ps_user_id.ToString().Equals(items[9]))
                        {
                            userFromBase = user;
                            break;
                        }
                    }
                    bool created = userFromBase == null;

                    //schoolSourceId, email, firstName, lastName, middleName, userName, salutation, pwd, active, ps_user_id
                    // firstName, lastName, active, ps_user_id are required fields
                    userAction userAction = Db.userActions.FirstOrDefault(p => p.userId == (userFromBase != null ? userFromBase.id : -1));
                    UserForm userForm = new UserForm
                                            {
                                                Id = userFromBase != null ? userFromBase.id : -1,
                                                Email = items[1],
                                                FirstName = items[2],
                                                LastName = items[3],
                                                MiddleName = items[4],
                                                UserName = !string.IsNullOrWhiteSpace(items[5])
                                                        ? items[5]
                                                        : GetUserNameIfEmpty(items[2], items[3]),
                                                Salutation = items[6],
                                                Pwd = !string.IsNullOrWhiteSpace(items[7]) ? items[7] : GeneratePassword(),
                                                Active = bool.Parse(items[8]),
                                                PsUserId = items[9],
                                                UserRole = userRoleId,
                                                Schools = school != null ? school.id.ToString() : string.Empty,
                                                Districts = organizationId.ToString(),
                                                Organizations = organizationId.ToString(),
                                                UserActionForm = new UserActionForm
                                                                     {
                                                                         Id = userAction != null ? userAction.id : -1,
                                                                         CanApprove = userAction != null ? userAction.canApprove : false,
                                                                         CanRequest = userAction != null ? userAction.canRequest : false,
                                                                         CanViewReports = userAction != null ? userAction.canViewReports : false
                                                                     }
                                            };
                    userForm = UserUpdate(userForm, currentUserId);
                    result.Add(new IdTitle { Id = i, Title = string.Format("Staff \"{0}\" {1} {2}", items[3], items[2], created ? "inserted" : "updated") });
                }
                catch (ArgumentNullException exc)
                {
                    result.Add(new IdTitle { Id = i, Title = exc.Message });
                }
                catch (Exception exc)
                {
                    result.Add(new IdTitle { Id = i, Title = string.Format("Line # {0}. {1}", i, exc.Message) });
                }
                i++;
            }

            return result;
        }

        private string GetUserNameIfEmpty(string firstName, string lastName)
        {
            string firstLastName = string.Format("{0}.{1}", firstName, lastName);
            string result = string.Empty;
            List<string> userNames = Users.Select(p => p.userName).ToList();
            if (!userNames.Any(p => p.ToLower().Equals(firstLastName)))
            {
                return firstLastName;
            }
            int i = 0;
            foreach (string userName in userNames)
            {
                if (!userName.ToLower().Equals(string.Format("{0}{1}", firstLastName, i)))
                {
                    result = string.Format("{0}{1}", firstLastName, i);
                }
                i++;
            }
            return result;
        }

        private string GeneratePassword(int length = 8)
        {
            return System.Web.Security.Membership.GeneratePassword(length, 0);
        }

        public List<DistrictStaffs> OrganizationStaffListGet(int user)
        {
            var userOrganiztion = Db.userOrganizations.FirstOrDefault(p => p.userId == user);
            if (userOrganiztion == null)
                return new List<DistrictStaffs>();
            int userOrg = userOrganiztion.organizationId;
            //var buf = Db.userOrganizations.Where(p => p.organizationId == userOrg && p.user.userRoles.FirstOrDefault().roleId == 3).ToList();
            var data =
                Db.userOrganizations.Where(
                    p => p.organizationId == userOrg && p.user.userRoles.FirstOrDefault().roleId == 3).Select(
                        p => new DistrictStaffs
                                 {
                                     Id = p.userId,
                                     Name = p.user.firstName + " " + p.user.lastName,
                                     Email = p.user.email,
                                     Phone = p.user.phone,
                                     CanApprove =
                                         p.user.userActions.Any()
                                             ? p.user.userActions.FirstOrDefault().canApprove
                                             : false,
                                     CanRequest =
                                         p.user.userActions.Any()
                                             ? p.user.userActions.FirstOrDefault().canRequest
                                             : false,
                                     CanViewReports =
                                         p.user.userActions.Any()
                                             ? p.user.userActions.FirstOrDefault().canViewReports
                                             : false,
                                     Cell = p.user.cell,
                                     Dictrict = p.organization.districtName,
                                     School = p.school.schoolName,
                                     Salutation = p.user.salutation
                                 }).OrderBy(p => p.School).ThenBy(p => p.Name).ToList();
            return data;
        }

        public bool OrganizationStaffListSave(List<UserActionForm> actions)
        {
            //Db.studentTeamConsultationFiles.FirstOrDefault().fileContent = new byte[10];
            int rows = 0;
            foreach (var item in actions)
            {
                var buf = Db.userActions.Where(p => p.userId == item.Id).FirstOrDefault();
                buf.canApprove = item.CanApprove;
                buf.canRequest = item.CanRequest;
                buf.canViewReports = item.CanViewReports;
            }
            rows = Db.GetChangeSet().Updates.Count();
            Db.userActions.Context.SubmitChanges();
            return rows == actions.Count;
        }

        public List<IdTitle> CurrentOrganiztionsSchoolsGet(int org)
        {
            var schools = Db.schools.Where(p => p.organizationId == org).Select(p => new IdTitle() { Id = p.id, Title = p.schoolName }).ToList();
            return schools;
        }

        public StudentForm StudentGet(int id)
        {
            var student = Db.students.Where(p => p.id == id).Select(p => new StudentForm()
            {
                Id = p.id,
                Address = p.address,
                City = p.city,
                Code = p.code,
                Dob = p.dob,
                FatherEmail = p.fatherEmail,
                FatherName = p.fatherName,
                FatherPhone = p.fatherPhone,
                FirstName = p.firstName,
                //Gender = p.gender.ToString().ToLower() == "m" ? "Male" : "Female",
                Gender = p.gender,
                Grade = p.grade,
                GuardianEmail = p.guardianEmail,
                GuardianName = p.guardianName,
                GuardianPhone = p.guardianPhone,
                HomePhone = p.homePhone,
                LastName = p.lastName,
                MailingAddress = p.mailingAddress,
                MailingCity = p.mailingCity,
                MailingPostalCode = p.mailingPostalCode,
                MailingProvince = p.mailingProvince,
                MiddleName = p.middleName,
                MotherEmail = p.motherEmail,
                MotherName = p.motherName,
                MotherPhone = p.motherPhone,
                PostalCode = p.postalCode,
                Province = p.province,
                School = p.school.schoolName,
                District = p.school.organization.districtName,
                SourceId = p.sourceId,
                SpecialPrograms = p.specialPrograms,
                StudentNumber = p.studentNumber
            }).FirstOrDefault();
            return student;
        }

        public List<StudentShortForm> OrganizationStudentsGet(int orgId)
        {
            var schools = CurrentOrganiztionsSchoolsGet(orgId).Select(p => p.Id).ToList();
            var students = Db.students.Where(p => schools.Contains(p.schoolId)).Select(p => new StudentShortForm()
            {
                Id = p.id,
                Dob = p.dob,
                Grade = p.grade,
                School = p.school.schoolName,
                StudentName = p.firstName + " " + p.lastName,
                Gender = p.gender.ToString().ToLower() == "m" ? "Male" : "Female"
            }).OrderBy(p => p.School).ThenBy(p => p.StudentName).ToList();
            return students;
        }

        #endregion  District Sys Admin

        #region Manager
        public List<ManagerRequests> ManagerRequestsGet(int role, string startDate, string endDate)
        {
            List<ManagerRequests> result = Db.studentSupportRequests.Where(p => p.user.userRoles.FirstOrDefault() != null && p.user.userRoles.FirstOrDefault().roleId == role && p.approved == true).Select(p => new ManagerRequests()
            {
                Id = p.id,
                DateOfRequest = p.dateRequested,
                District = p.student.school.organization.districtName,
                School = p.student.school.schoolName,
                StudentName = p.student.lastName + ", " + p.student.firstName,
                RequestedBy = p.user.lastName + ", " + p.user.firstName,
                Specialities = string.Join(", ", p.studentSupportRequestTypes.Select(t => t.supportRequestType.code).ToList()),
                SpecialitiyIds = p.studentSupportRequestTypes.Select(t => t.supportRequestTypeId).ToList(),
                Specialists = string.Join(", ", p.studentSupportRequestAssignments.Select(a => a.user.lastName + " " + a.user.firstName).Distinct().ToList())
            }).ToList();
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime start = DateTime.ParseExact(startDate, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
                result = result.Where(p => p.DateOfRequest >= start).ToList();
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime end = DateTime.ParseExact(endDate, "MMMM dd, yyyy", CultureInfo.InvariantCulture);
                result = result.Where(p => p.DateOfRequest <= end).ToList();
            }
            return result;
        }

        public ManagerRequests ManagerRequestOneGet(int id)
        {
            ManagerRequests result = Db.studentSupportRequests.Where(p => p.id == id).Select(p => new ManagerRequests()
                {
                    Id = p.id,
                    StudentId = p.studentId,
                    StudentName = p.student.firstName + " " + p.student.lastName,
                    School = p.student.school.schoolName,
                    Grade = p.student.grade,
                    Gender = p.student.gender.ToString().ToUpper(),
                    Reason = p.reason,
                    AssignedSpecialists = p.studentSupportRequestTypes.Select(t => new RequestTypeSpecialist()
                    {
                        Type = new IdTitle()
                        {
                            Id = t.supportRequestType.id,
                            Title = t.supportRequestType.code
                        },
                        Specialist = p.studentSupportRequestAssignments.Where(a => a.supportRequestTypeId == t.supportRequestTypeId).FirstOrDefault() == null ? new IdTitle() : new IdTitle()
                        {
                            Id = p.studentSupportRequestAssignments.Where(a => a.supportRequestTypeId == t.supportRequestTypeId).Select(a => a.user.id).FirstOrDefault(),
                            Title = p.studentSupportRequestAssignments.Where(a => a.supportRequestTypeId == t.supportRequestTypeId).Select(a => a.user.lastName + ", " + a.user.firstName).FirstOrDefault()
                        }
                    }).ToList()
                }).FirstOrDefault();
            return result;
        }

        public bool MangerRequestOneSave(int id, List<RequestTypeSpecialist> items, int student)
        {
            try
            {
                foreach (var item in items)
                {
                    if (
                        Db.studentSupportRequestAssignments.Where(p => p.supportRequestTypeId == item.Type.Id && p.studentSupportRequestId == id)
                            .FirstOrDefault() != null)
                    {
                        var data =
                            Db.studentSupportRequestAssignments.Where(p => p.supportRequestTypeId == item.Type.Id && p.studentSupportRequestId == id)
                                .FirstOrDefault();
                        data.supportRequestTypeId = item.Type.Id;
                        data.specialistId = item.Specialist.Id;
                    }
                    else
                    {
                        if (item.Specialist.Id != 0)
                        {
                            studentSupportRequestAssignment assignment = new studentSupportRequestAssignment();
                            assignment.dateAssigned = DateTime.Now;
                            assignment.specialistId = item.Specialist.Id = item.Specialist.Id;
                            assignment.studentSupportRequestId = id;
                            assignment.supportRequestTypeId = item.Type.Id;
                            Db.studentSupportRequestAssignments.InsertOnSubmit(assignment);
                        }
                    }

                    if (
                        Db.studentSupportReports.Where(p => p.studentSupportRequestId == id && p.studentId == student && item.Specialist.Id != null && p.specialistId == item.Specialist.Id)
                            .FirstOrDefault() != null)
                    {
                    }
                    else
                    {
                        if (item.Specialist.Id != 0)
                        {
                            studentSupportReport report = new studentSupportReport();
                            report.studentSupportRequestId = id;
                            report.dateStarted = DateTime.Now;
                            report.specialistId = item.Specialist.Id;
                            report.studentId = student;
                            Db.studentSupportReports.InsertOnSubmit(report);
                            SubmitChanges();
                        }
                    }
                }
                SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                {
                    return false;
                }
            }
        }
        #endregion Manager
    }
}