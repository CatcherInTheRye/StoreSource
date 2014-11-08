using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DataDynamics.ActiveReports;
using DataDynamics.ActiveReports.Export.Pdf;
using DataDynamics.ActiveReports.Export.Xls;
using DataRepository;
using DataRepository.DataContracts;
using PCS.Reports;
using PCSMvc.Global.Auth;
using PCSMvc.Models;
using PCSMvc.Models.ViewModels;
using PCS.DataRepository.DataContracts;

namespace PCSMvc.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (CurrentUser != null)
            {
                switch (CurrentUser.UserRole)
                {
                    case (int)UserRoleEnum.PCSManager:
                        return RedirectToAction("PCSManager");
                        break;
                    case (int)UserRoleEnum.Specialist:
                        return RedirectToAction("Specialist");
                        break;
                    case (int)UserRoleEnum.SchoolUser:
                        return RedirectToAction("SchoolUser");
                        break;
                    case (int)UserRoleEnum.DistrictSysAdmin:
                        return RedirectToAction("DistrictSysAdmin");
                        break;
                    default:
                        return RedirectToAction("Login");
                        break;
                }
            }

            return RedirectToAction("Login");
        }

        #region Login

        public ActionResult UserLogin()
        {
            return View(CurrentUser);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginView());
        }

        [HttpPost]
        public ActionResult Login(LoginView loginView)
        {
            if (ModelState.IsValid)
            {
                var user = Auth.Login(loginView.UserName, loginView.Password, loginView.IsPersistent);
                if (user != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState["Password"].Errors.Add("Passwords doesn't match");
            }
            return View(loginView);
        }

        public ActionResult Logout()
        {
            Auth.LogOut();
            return RedirectToAction("Login", "Home");
        }

        //AccessDenied
        public ActionResult AccessDenied()
        {
            return View();
        }

        #endregion Login

        #region PCS Manager

        private void MenuCurrentSet(PCSManagerIconsMenu menuItem)
        {
            ViewBag.SelectedMenuItem = menuItem.ToString();
        }

        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult PCSManager()
        {
            return View();
        }

        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult PCSManagerSpecialists()
        {
            MenuCurrentSet(PCSManagerIconsMenu.Specialist);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult PCSManagerSpecialistsGet(string filter, int roleId, bool includeInactive)
        {
            List<UserForm> result = Repository.UsersGetByRoleFiltered(filter, roleId, includeInactive);
            return JSON(new JsonTableResult<UserForm>
            {
                Data = result
            });
        }

        private void UserUpdateSetViewBag()
        {
            List<OrganizationForm> organizations = Repository.OrganizationsGet();
            ViewBag.Organizations = organizations;
            List<SchoolFormShort> schools = Repository.SchoolsGet();
            ViewBag.Schools = schools;
            List<IdTitleDescription> supportRequestTypes = Repository.SupportRequestTypesGet();
            ViewBag.SupportRequestTypes = supportRequestTypes;
            MenuCurrentSet(PCSManagerIconsMenu.Specialist);
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult UserUpdate(int? id)
        {
            UserUpdateSetViewBag();
            UserForm result = Repository.UserGetForEdit(id.GetValueOrDefault(-1));
            return View(result);
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult UserUpdate(UserForm user)
        {
            UserForm u;
            try
            {
                if (user.Id == -1)
                {
                    user.UserRole = (int)UserRoleEnum.Specialist;
                }
                user.Organizations = user.Districts;
                u = Repository.UserUpdate(user, CurrentUser.id);
            }
            catch (Exception ex)
            {
                UserUpdateSetViewBag();
                ViewBag.Error = ex.Message;
                return View(user);
            }
            return RedirectToAction("UserUpdate", new { id = u.Id });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult UserRemove(int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "User was removed successfully", "USER REMOVE");
            try
            {
                Repository.UserRemove(id);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "USER REMOVE");
            }
            return JSON(
              new JsonTableResult<UserForm>
              {
                  Total = 0,
                  Data = new List<UserForm>(),
                  Result = result
              });
        }

        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult SpecialistRequestSupport()
        {
            //CurrentSchoolUsers users = Repository.CurrentDistrictSchoolUsers(CurrentUser.id, (int)UserRoleEnum.Specialist);
            //List<UserShortInfoDistrict> users = Repository.AllUsersGet((int) UserRoleEnum.Specialist);
            //ViewBag.users = users;
            MenuCurrentSet(PCSManagerIconsMenu.Specialist);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult SupportRequestsGet(int role, string startDate, string endDate)
        {
            List<ManagerRequests> data = new List<ManagerRequests>();
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "User was removed successfully", "USER REMOVE");
            try
            {
                data = Repository.ManagerRequestsGet(role, startDate, endDate);
                Session["selectedTypes"] =
                    data.Select(p => p.SpecialitiyIds).ToList().Distinct().SelectMany(x => x).ToList();

            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "USER REMOVE");
            }
            return JSON(
              new JsonTableResult<ManagerRequests>
              {
                  Total = 0,
                  Data = data,
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult TypeUserSelects()
        {
            List<int> ids = Session["selectedTypes"] as List<int> ?? new List<int>();
            Dictionary<int, List<UserShortInfoDistrict>> users = Repository.AllUsersGet(ids, (int)UserRoleEnum.Specialist);
            ViewBag.users = users;
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult SupportRequestOneGet(int id)
        {
            ManagerRequests result = Repository.ManagerRequestOneGet(id);
            return JSON(new JsonTableResult<ManagerRequests>
            {
                Data = new List<ManagerRequests>() { result }
            });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult SupportRequestOneSave(int id, List<RequestTypeSpecialist> items, int student)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "User was removed successfully", "USER REMOVE");
            try
            {
                if (!Repository.MangerRequestOneSave(id, items, student))
                    throw new Exception("Save error.");
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "USER REMOVE");
            }
            return JSON(
              new JsonTableResult<ManagerRequests>
              {
                  Total = 0,
                  Data = new List<ManagerRequests>(),
                  Result = result
              });
        }

        #region FTE

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult FteValuesGet(int userId)
        {
            List<FteForm> result = Repository.FtesGetForUser(userId);
            return JSON(new JsonTableResult<FteForm>
            {
                Data = result
            });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult FteUpdate(FteForm fteForm)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            try
            {
                fteForm = Repository.FteUpdate(fteForm);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<FteForm>
              {
                  Total = 1,
                  Data = new List<FteForm> { fteForm },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult FteRemove(int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                Repository.FteRemove(id);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<FteForm>
              {
                  Total = 0,
                  Data = new List<FteForm>(),
                  Result = result
              });
        }

        #endregion FTE

        #region Settings

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult PCSManagerSettings()
        {
            MenuCurrentSet(PCSManagerIconsMenu.Settings);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult SettingsGet()
        {
            List<SettingsForm> result = Repository.SettingsGet();
            return JSON(new JsonTableResult<SettingsForm>
            {
                Data = result
            });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult SettingsUpdate(SettingsForm setForm)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            try
            {
                setForm = Repository.SettingsUpdate(setForm);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<SettingsForm>
              {
                  Total = 1,
                  Data = new List<SettingsForm> { setForm },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult SettingsRemove(int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "Settings was removed successfully", "SETTINGS REMOVE");
            try
            {
                Repository.SettingRemove(id);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "SETTINGS REMOVE");
            }
            return JSON(
              new JsonTableResult<SettingsForm>
              {
                  Total = 0,
                  Data = new List<SettingsForm>(),
                  Result = result
              });
        }

        #endregion Settings

        #region Reports
        
        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult PCSManagerReportTime()
        {
            ViewBag.specialists = Repository.UsersGetByRole((int)UserRoleEnum.Specialist, false);
            ViewBag.districts = Repository.DistrictsGet();
            ViewBag.schools = Repository.SchoolsGet();
            MenuCurrentSet(PCSManagerIconsMenu.Reports);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult PCSManagerReportTimeGet(string start, string end, int specialist, int district, int school)
        {
            List<TimeRecordReportForm> data = new List<TimeRecordReportForm>();
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "User was removed successfully", "USER REMOVE");
            try
            {
                data = Repository.TimeRecordReportGet(start, end, specialist, district, school);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "USER REMOVE");
            }
            return JSON(
              new JsonTableResult<TimeRecordReportForm>
              {
                  Total = 0,
                  Data = data,
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult PCSManagerReportTimeTotalGet(string start, string end, int specialist)
        {
            List<string> data = new List<string>();
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "User was removed successfully", "USER REMOVE");
            try
            {
                data = Repository.TimeRecordReportTotalGet(start, end, specialist);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "USER REMOVE");
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 0,
                  Data = data,
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult PCSManagerReportTimeReport(string reportType, string start, string end, int specialist, int district, int school)
        {
            List<TimeRecordReportForm> data = Repository.TimeRecordReportGet(start, end, specialist, district, school);

            ActiveReport rpt = new ManagerReportTimePdf();

            DataTable dt = ReportHelper.ToDataTable(data);
            rpt.DataSource = dt;

            rpt.Document.Printer.PrinterName = "";
            rpt.PageSettings.Margins.Left = 0.5f;
            rpt.PageSettings.Margins.Right = 0.5f;
            rpt.PageSettings.Margins.Top = 0.5f;
            rpt.PageSettings.Margins.Bottom = 0.5f;
            rpt.Document.Printer.Landscape = false;
            rpt.Document.Printer.PaperKind = System.Drawing.Printing.PaperKind.Letter;
            rpt.Run(false);

            string fileName = string.Format("Manager time report {0}.{1}", DateTime.UtcNow.Date.ToShortDateString(), reportType);
            if (reportType.Equals(ReportType.pdf.ToString()))
            {
                PdfExport pdf = new PdfExport();
                pdf.ConvertMetaToPng = true;
                pdf.Version = PdfVersion.Pdf13;
                pdf.ImageQuality = ImageQuality.Highest;
                pdf.ImageResolution = 200;
                using (pdf)
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        pdf.Export(rpt.Document, memStream);
                        return File(memStream.ToArray(), "application/octet-stream", fileName);
                    }
                }
            }
            else
            {
                XlsExport xls = new XlsExport();
                using (xls)
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        xls.Export(rpt.Document, memStream);
                        return File(memStream.ToArray(), "application/octet-stream", fileName);
                    }
                }
            }
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult PCSManagerReportCase()
        {
            ViewBag.specialists = Repository.UsersGetByRole((int)UserRoleEnum.Specialist, false);
            ViewBag.districts = Repository.DistrictsGet();
            ViewBag.schools = Repository.SchoolsGet();
            MenuCurrentSet(PCSManagerIconsMenu.Reports);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult PCSManagerReportCaseGet(string start, string end, int specialist, int district, int school)
        {
            List<CaseRecordReportForm> data = new List<CaseRecordReportForm>();
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "User was removed successfully", "USER REMOVE");
            try
            {
                data = Repository.CaseRecordReportGet(start, end, specialist, district, school);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "USER REMOVE");
            }
            return JSON(
              new JsonTableResult<CaseRecordReportForm>
              {
                  Total = 0,
                  Data = data,
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult PCSManagerReportCaseReport(string reportType, string start, string end, int specialist, int district, int school)
        {
            List<CaseRecordReportForm> data = Repository.CaseRecordReportGet(start, end, specialist, district, school);

            ActiveReport rpt = new ManagerReportCasePdf();

            DataTable dt = ReportHelper.ToDataTable(data);
            rpt.DataSource = dt;

            rpt.Document.Printer.PrinterName = "";
            rpt.PageSettings.Margins.Left = 0.5f;
            rpt.PageSettings.Margins.Right = 0.5f;
            rpt.PageSettings.Margins.Top = 0.5f;
            rpt.PageSettings.Margins.Bottom = 0.5f;
            rpt.Document.Printer.Landscape = false;
            rpt.Document.Printer.PaperKind = System.Drawing.Printing.PaperKind.Letter;
            rpt.Run(false);

            string fileName = string.Format("Manager case report {0}.{1}", DateTime.UtcNow.Date.ToShortDateString(), reportType);
            if (reportType.Equals(ReportType.pdf.ToString()))
            {
                PdfExport pdf = new PdfExport();
                pdf.ConvertMetaToPng = true;
                pdf.Version = PdfVersion.Pdf13;
                pdf.ImageQuality = ImageQuality.Highest;
                pdf.ImageResolution = 200;
                using (pdf)
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        pdf.Export(rpt.Document, memStream);
                        return File(memStream.ToArray(), "application/octet-stream", fileName);
                    }
                }
            }
            else
            {
                XlsExport xls = new XlsExport();
                using (xls)
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        xls.Export(rpt.Document, memStream);
                        return File(memStream.ToArray(), "application/octet-stream", fileName);
                    }
                }
            }
        }

        #endregion Reports

        #endregion PCS Manager

        #region Specialist

        private int SessionStudentGet()
        {
            int result = -1;
            int.TryParse((string) Session["Student"], out result);
            return result;
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Specialist()
        {
            Session["Student"] = 0;
            return View(Repository.RoleAreasGet(CurrentUser.UserRole));
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult SpecialistArea(int areaId)
        {
            switch (areaId)
            {
                case (int)SpecialistAreas.Students:
                    return RedirectToAction("StudentSearch");
                    break;
                case (int)SpecialistAreas.Office:
                    return RedirectToAction("TimerecordsOffice");
                    break;
                case (int)SpecialistAreas.Reports:
                    return RedirectToAction("Reports");
                    break;
            }
            return RedirectToAction("StudentSearch");
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Background()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            ViewBag.Background = Repository.BackgroundGet(CurrentUser.id, SessionStudentGet(), (int)TabItemEnum.Current, 0);//SessionStudentGet());
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult BackgroundGet(int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            BackgroundForm res = new BackgroundForm();
            try
            {
                res = Repository.BackgroundGet(CurrentUser.id, SessionStudentGet(), type, id);
                if (res == null)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<BackgroundForm>
              {
                  Total = 1,
                  Data = new List<BackgroundForm>() { res },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult Background(string background, int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.BackgroundSave(CurrentUser.id, SessionStudentGet(), background, type, id);
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { background },
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Observations()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            ViewBag.content = Repository.ObservationsGet(CurrentUser.id, SessionStudentGet(), (int)TabItemEnum.Current, 0);
            return View();
            //Repository.ObservationsGet(CurrentUser.id, SessionStudentGet())
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult ObservationsGet(int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            string res = "";
            try
            {
                res = Repository.ObservationsGet(CurrentUser.id, SessionStudentGet(), type, id);
                if (res == null)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { res },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult Observations(string observation, int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.ObservationsSave(CurrentUser.id, SessionStudentGet(), observation, type, id);
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { observation },
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Test()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            ViewBag.content = Repository.TestGet(CurrentUser.id, SessionStudentGet(), (int)TabItemEnum.Current, 0);
            return View();
            //Repository.ObservationsGet(CurrentUser.id, SessionStudentGet())
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult TestGet(int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            string res = "";
            try
            {
                res = Repository.TestGet(CurrentUser.id, SessionStudentGet(), type, id);
                if (res == null)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { res },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult Test(string test, int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.TestSave(CurrentUser.id, SessionStudentGet(), test, type, id);
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { test },
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Goals()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            ViewBag.content = Repository.GoalsGet(CurrentUser.id, SessionStudentGet(), (int)TabItemEnum.Current, 0);
            return View();
            //Repository.ObservationsGet(CurrentUser.id, SessionStudentGet())
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult GoalsGet(int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            string res = "";
            try
            {
                res = Repository.GoalsGet(CurrentUser.id, SessionStudentGet(), type, id);
                if (res == null)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { res },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult Goals(string goals, int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.GoalsSave(CurrentUser.id, SessionStudentGet(), goals, type, id);
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { goals },
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Progress()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            ViewBag.content = Repository.ProgressGet(CurrentUser.id, SessionStudentGet(), (int)TabItemEnum.Current, 0);
            return View();
            //Repository.ObservationsGet(CurrentUser.id, SessionStudentGet())
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult ProgressGet(int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            string res = "";
            try
            {
                res = Repository.ProgressGet(CurrentUser.id, SessionStudentGet(), type, id);
                if (res == null)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { res },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult Progress(string progress, int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.ProgressSave(CurrentUser.id, SessionStudentGet(), progress, type, id);
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { progress },
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Recomendations()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            ViewBag.content = Repository.RecomendationsGet(CurrentUser.id, SessionStudentGet(), (int)TabItemEnum.Current, 0);
            return View();
            //Repository.ObservationsGet(CurrentUser.id, SessionStudentGet())
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult RecomendationsGet(int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            string res = "";
            try
            {
                res = Repository.RecomendationsGet(CurrentUser.id, SessionStudentGet(), type, id);
                if (res == null)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { res },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult Recomendations(string recomendation, int type, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.RecomendationsSave(CurrentUser.id, SessionStudentGet(), recomendation, type, id);
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { recomendation },
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Summary()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            ViewBag.Summary = Repository.SummaryGet(CurrentUser.id, SessionStudentGet(), (int)TabItemEnum.Current, 0);
            ViewBag.Status = Repository.StatusGet(CurrentUser.id, SessionStudentGet(), (int)TabItemEnum.Current, 0);
            return View();
            //Repository.ObservationsGet(CurrentUser.id, SessionStudentGet())
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult SummaryGet(string type, int typeId, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            string res = "";
            try
            {
                switch (type)
                {
                    case "Summary":
                        res = Repository.SummaryGet(CurrentUser.id, SessionStudentGet(), typeId, id);
                        break;
                    case "Status":
                        res = Repository.StatusGet(CurrentUser.id, SessionStudentGet(), typeId, id);
                        break;
                }
                if (res == null)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { res },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult Summary(string type, string data, int typeId, int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                switch (type)
                {
                    case "Summary":
                        rows = Repository.SummarySave(CurrentUser.id, SessionStudentGet(), data, typeId, id);
                        break;
                    case "Status":
                        rows = Repository.StatusSave(CurrentUser.id, SessionStudentGet(), data, typeId, id);
                        break;
                }
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { data },
                  Result = result
              });
        }

        #region Attach Documents

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult AttachDocuments()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult AttachDocumentsGet()
        {
            List<StudentSupportReportFileShort> result = Repository.StudentSupportReportFilesGet(CurrentUser.id, SessionStudentGet());

            return JSON(new JsonTableResult<StudentSupportReportFileShort>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult AttachDocumentGet(int id)
        {
            StudentSupportReportFile result = Repository.StudentSupportReportFileGet(id);
            using (MemoryStream memStream = new MemoryStream())
            {
                
                return File(result != null ? result.FileContent.ToArray() : new byte[0], "application/octet-stream", result != null ? result.FileName : "empty");
            }
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult AttachDocumentUpload(HttpPostedFileBase importFiles)
        {
            List<StudentSupportReportFileShort> reportResults = new List<StudentSupportReportFileShort>();
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            try
            {
                using (Stream inputStream = importFiles.InputStream)
                {
                    MemoryStream memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }

                    byte[] binData = memoryStream.ToArray();

                    StudentSupportReportFile reportFile = new StudentSupportReportFile
                                                              {
                                                                  Id = -1,
                                                                  FileName = importFiles.FileName,
                                                                  DateUploaded = DateTime.UtcNow,
                                                                  FileContent = new System.Data.Linq.Binary(binData)
                                                              };
                    StudentSupportReportFileShort reportResult = Repository.StudentSupportReportFileUpload(CurrentUser.id, SessionStudentGet(), reportFile);
                    reportResults.Add(reportResult);
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<StudentSupportReportFileShort>
              {
                  Total = reportResults.Count,
                  Data = reportResults,
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult AttachDocumentRemoveEmpty(string fileNames)
        {
            return new JsonResult();
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult AttachDocumentRemove(int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                Repository.StudentSupportReportFileRemove(id);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<StudentSupportReportFileShort>
              {
                  Total = 0,
                  Data = new List<StudentSupportReportFileShort>(),
                  Result = result
              });
        }

        #endregion Attach Documents

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Office()
        {
            return View();
        }

        private void SpecialistMenuSetViewBag(int area)
        {
            Session["user"] = CurrentUser.id;
            StudentMenuFull menu = Repository.MenuGet(area);
            ViewBag.leftUpperMenu = menu.first;
            ViewBag.leftMiddleMenu = menu.second;
            ViewBag.leftLowMenu = Repository.RoleAreasGet(CurrentUser.UserRole);
            //var tabs = Repository.TabMenuGet(CurrentUser.id, SessionStudentGet());
            if (area == (int)SpecialistAreas.Students)
            {
                var tabs = Repository.TabMenuGet(CurrentUser.id, SessionStudentGet());
                ViewBag.tabsMenu = tabs;
            }
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult Timerecords()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult TimerecordsGet(DateRange range)
        {
            List<TimeRecordFrom> result = Repository.TimeRecordsGet(CurrentUser.id, SessionStudentGet());
            return JSON(new JsonTableResult<TimeRecordFrom>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult TimerecordsGetOne(int recordId)
        {
            TimeRecordFrom result = Repository.TimeRecordOneGet(recordId);
            return JSON(new JsonTableResult<TimeRecordFrom>
            {
                Data = new List<TimeRecordFrom>() { result }
            });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult Timerecords(TimeRecordFrom form)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.TimeRecordsSave(form, CurrentUser.id, SessionStudentGet());
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            List<TimeRecordFrom> results = Repository.TimeRecordsGet(CurrentUser.id, SessionStudentGet());
            return JSON(
              new JsonTableResult<TimeRecordFrom>
              {
                  Total = 1,
                  Data = results,
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult TimerecordsDelete(int recordId)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.TimeRecordsDelete(recordId);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            List<TimeRecordFrom> results = Repository.TimeRecordsGet(CurrentUser.id, SessionStudentGet());
            return JSON(
              new JsonTableResult<TimeRecordFrom>
              {
                  Total = 1,
                  Data = results,
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult TimerecordsOffice()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Office);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult TimerecordsOfficeGet(DateRange range)
        {
            List<TimeRecordFrom> result = Repository.TimeRecordsOfficeGet(range, CurrentUser.id);
            return JSON(new JsonTableResult<TimeRecordFrom>
            {
                Data = result
            });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult TimerecordsOffice(TimeRecordFrom form)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.TimeRecordsOfficeSave(form, CurrentUser.id);
                if (rows == 0)
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<TimeRecordFrom>
              {
                  Total = 1,
                  Data = new List<TimeRecordFrom>() { form },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult TimerecordsOfficeDelete(int recordId)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                rows = Repository.TimeRecordsDelete(recordId);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<TimeRecordFrom>
              {
                  Total = 1,
                  Data = new List<TimeRecordFrom>(),
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult StudentSearch()
        {
            if (Session["Student"] != null && int.Parse(Session["Student"].ToString()) != 0)
            {
                return RedirectToAction("SelectStudent", new RouteValueDictionary(new { studentId = (string)Session["Student"] }));
            }
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult StudentSearchGet()
        {
            List<StudentSearchForm> result = Repository.StudentSearchGet(CurrentUser.id);
            return JSON(new JsonTableResult<StudentSearchForm>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult StudentSearchGetParams(string filters)
        {
            List<StudentSearchForm> result = Repository.StudentSearchGetParams(filters, CurrentUser.id);
            return JSON(new JsonTableResult<StudentSearchForm>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult SelectStudent(string studentId)
        {
            Session["Student"] = studentId;
            Session["SelectedTab"] = "0";

            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            //ViewBag.Background = Repository.BackgroundGet(CurrentUser.id, SessionStudentGet());
            return RedirectToAction(FirstMenuItemGet((int)SpecialistAreas.Students));
        }

        private string FirstMenuItemGet(int area)
        {
            return Repository.MenuGet(area).first.First().href;
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public ActionResult StudentInfo()
        {
            SpecialistMenuSetViewBag((int)SpecialistAreas.Students);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult OtehrUsersGet()
        {
            List<UserShortInfo> result = Repository.OtherSpecialistsGet(CurrentUser.id, SessionStudentGet());
            return JSON(new JsonTableResult<UserShortInfo>
            {
                Data = result
            });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.Specialist)]
        public JsonResult TeamConsultCreate(string users)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                Repository.TeamConsultCreate(CurrentUser.id, SessionStudentGet(), users);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>() { Request.UrlReferrer.ToString() },
                  Result = result
              });
        }

        #endregion Specialist

        #region Organizations

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult Organizations()
        {
            MenuCurrentSet(PCSManagerIconsMenu.Organizations);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult OrganizationsGet()
        {
            List<OrganizationForm> result = Repository.OrganizationsGet();

            return JSON(new JsonTableResult<OrganizationForm>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult OrganizationUpdate(int? id)
        {
            MenuCurrentSet(PCSManagerIconsMenu.Organizations);
            OrganizationForm result = Repository.OrganizationGetForEdit(id.GetValueOrDefault(-1));
            return View(result);
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public ActionResult OrganizationUpdate(OrganizationForm organization)
        {
            try
            {
                organization = Repository.OrganizationUpdate(organization, (int)UserRoleEnum.DistrictSysAdmin, CurrentUser.id);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                MenuCurrentSet(PCSManagerIconsMenu.Organizations);
                return View(organization);
            }
            return RedirectToAction("OrganizationUpdate", new { id = organization.Id });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.PCSManager)]
        public JsonResult OrganizationRemove(int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "Organization was removed successfully", "ORGANIZATION REMOVE");
            try
            {
                Repository.OrganizationRemove(id);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "ORGANIZATION REMOVE");
            }
            return JSON(
              new JsonTableResult<OrganizationForm>
              {
                  Total = 0,
                  Data = new List<OrganizationForm>(),
                  Result = result
              });
        }

        #endregion Organizations

        #region SchoolUser

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult SchoolUser()
        {
            return RedirectToAction(Repository.SchoolUserMenuGet(CurrentUser.id).First().href);
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult RequestSupport(int? id)
        {
            UserSchoolMenuSetViewBag();
            StudentSupportRequestForm form = Repository.RequestFormGetOne(id.GetValueOrDefault(-1));
            if (form.SelectedFile != null && form.SelectedFile.Id != -1)
            {
                Session["file"] = form.SelectedFile;
            }
            return View(form);
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult RequestSupportList()
        {
            UserSchoolMenuSetViewBag();
            return View();
        }
        
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult RequestSuportRemove(int id)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                Repository.StudentSupportRequestDelete(id);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = 1,
                  Data = new List<string>(),
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult ApproveSupportGet()
        {
            List<StudentSupportRequestSchortForm> result = Repository.ApprovalRequestsGet(CurrentUser.id);
            return JSON(new JsonTableResult<StudentSupportRequestSchortForm>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult ApproveSupport()
        {
            UserSchoolMenuSetViewBag();
            var data = Repository.ApprovalRequestsGet(CurrentUser.id);
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult ApproveSupportEdit(int id)
        {
            UserSchoolMenuSetViewBag();
            return View(Repository.ApprovalRequestOneGet(id));
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult ApproveSupportFileDownload(int id)
        {
            UploadedFile file = Repository.ApprovalRequestFileGet(id);
            return File(file.Content.ToArray(), "application/octet-stream", file.Name);
        }
        
        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult SupportRequestSave(StudentSupportRequestForm form)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            StudentSupportRequestForm resultRequest = new StudentSupportRequestForm();
            int rows = 0;
            form.SelectedFile = Session["file"] as UploadedFile ?? null;
            try
            {
                resultRequest = Repository.StudentSupportRequestSave(form, CurrentUser.id);
                if (resultRequest == null)
                {
                    throw new Exception("No rows for saving.");
                }
                NewRequestMail(CurrentUser.id, form.StudentId);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<StudentSupportRequestForm>
              {
                  Total = 1,
                  Data = new List<StudentSupportRequestForm>() { resultRequest },
                  Result = result
              });
        }

        public void UserSchoolMenuSetViewBag()
        {
            List<StudentMenuItem> menu = Repository.SchoolUserMenuGet(CurrentUser.id);
            List<UserShortInfo> students = Repository.StudentsForCurrentSchoolOrg(CurrentUser.id);
            List<IdTitleDescription> types = Repository.SupportRequestTypesGet();
            CurrentSchoolUsers users = Repository.CurrentDistrictSchoolUsers(CurrentUser.id, (int)UserRoleEnum.SchoolUser);
            ViewBag.menu = menu;
            ViewBag.students = students;
            ViewBag.types = types;
            ViewBag.users = users;
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult SupportRequestDeny(int request)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                if (!Repository.StudentRequestDeny(request))
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<StudentSupportRequestForm>
              {
                  Total = 1,
                  //Data = new List<StudentSupportRequestForm>() { form },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult SupportRequestApprove(int request, string Note)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                if (!Repository.StudentRequestApprove(request, Note))
                {
                    throw new Exception("No rows for saving.");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<StudentSupportRequestForm>
              {
                  Total = 1,
                  //Data = new List<StudentSupportRequestForm>() { form },
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult CurrentTabSave(int index)
        {
            Session["SelectedTab"] = index;
            return new JsonResult();
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult fileUpload(HttpPostedFileBase files)
        {
            byte[] binData;
            using (Stream inputStream = files.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                binData = memoryStream.ToArray();
                //List<UploadedFile> fileList = Session["files"] as List<UploadedFile> ?? new List<UploadedFile>();
                UploadedFile file = new UploadedFile();
                file.Name = files.FileName;
                file.Content = binData;
                //fileList.Add(file);
                Session["file"] = file;
            }
            //return RedirectToAction("RequestSupport", "Home", null);
            return new JsonResult();
            //here i have got the files value is null.
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult fileRemove(string fileNames)
        {
            UploadedFile fileList = Session["file"] as UploadedFile ?? new UploadedFile();
            if (!string.IsNullOrEmpty(fileNames) && fileList != null && fileList.Name == fileNames)
            {
                Session.Remove("file");
            }
            //return RedirectToAction("RequestSupport", "Home", null);
            return new JsonResult();
            //here i have got the files value is null.
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult StudentReports()
        {
            UserSchoolMenuSetViewBag();
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult StudentReportsGet()
        {
            List<SchoolStudentsSearch> result = Repository.StudentOrgSchoolGet(CurrentUser.id);
            return JSON(new JsonTableResult<SchoolStudentsSearch>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public JsonResult StudentReportsGetParams(string filters)
        {
            List<SchoolStudentsSearch> result = Repository.StudentOrgSchoolGetParams(filters, CurrentUser.id);
            return JSON(new JsonTableResult<SchoolStudentsSearch>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult StudentReport(int id, int type = (int)TabItemEnum.Current, int recordId = 0)
        {
            var data = Repository.StudentReportGet(id, type, recordId);
            UserSchoolMenuSetViewBag();
            ViewBag.reports = Repository.StudentReportsGet(id);
            return View(data);
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult StudentReportPDF(int id, int type = (int)TabItemEnum.Current, int recordId = 0)
        {
            StudentFullInfo data = Repository.StudentReportGet(id, type, recordId);

            ActiveReport rpt = new StudentReportPdf();
            ((TextBox)rpt.Sections["detail"].Controls["txtDate"]).Text = DateTime.UtcNow.Date.ToString();
            ((TextBox)rpt.Sections["detail"].Controls["txtStudName"]).Text = string.Format("{0} {1}",
                                                                                            data.MainInfo.LastName,
                                                                                            data.MainInfo.FirstName);
            ((TextBox)rpt.Sections["detail"].Controls["txtSchool"]).Text = data.MainInfo.School;
            ((TextBox)rpt.Sections["detail"].Controls["txtTeacher"]).Text = "Teacher"; //TODO: teacher???

            ((TextBox)rpt.Sections["detail"].Controls["txtDob"]).Text = data.MainInfo.School;
            ((TextBox)rpt.Sections["detail"].Controls["txtGradeProgram"]).Text =
                !string.IsNullOrWhiteSpace(data.MainInfo.SpecialPrograms)
                    ? string.Format("{0}/{1}", data.MainInfo.Grade, data.MainInfo.SpecialPrograms)
                    : data.MainInfo.Grade;

            ((TextBox)rpt.Sections["detail"].Controls["txtSpecialityArea"]).Text = string.Join("; ",
                                                                                                data.Services.Select(
                                                                                                    p => p.Title));

            ((TextBox)rpt.Sections["detail"].Controls["txtConsultants"]).Text = data.ConsultNames;

            ((RichTextBox)rpt.Sections["detail"].Controls["richBackground"]).Html = data.Background;
            ((RichTextBox)rpt.Sections["detail"].Controls["richObservations"]).Html = data.Observations;
            ((RichTextBox)rpt.Sections["detail"].Controls["richTestResults"]).Html = data.Test;
            ((RichTextBox)rpt.Sections["detail"].Controls["richTreatment"]).Html = data.Goals;
            ((RichTextBox)rpt.Sections["detail"].Controls["richProgress"]).Html = data.Progress;
            ((RichTextBox)rpt.Sections["detail"].Controls["richRecommendations"]).Html = data.Recomendations;
            ((RichTextBox)rpt.Sections["detail"].Controls["richSummary"]).Html = string.Format("{0}<br/>{1}", data.Summary,
                                                                                                data.Status);

            rpt.Document.Printer.PrinterName = "";
            rpt.PageSettings.Margins.Left = 0.5f;
            rpt.PageSettings.Margins.Right = 0.5f;
            rpt.PageSettings.Margins.Top = 0.5f;
            rpt.PageSettings.Margins.Bottom = 0.5f;
            rpt.Document.Printer.Landscape = false;
            rpt.Document.Printer.PaperKind = System.Drawing.Printing.PaperKind.Letter;
            rpt.Run(false);

            PdfExport pdf = new PdfExport();
            pdf.ConvertMetaToPng = true;
            pdf.Version = PdfVersion.Pdf13;
            pdf.ImageQuality = ImageQuality.Highest;
            pdf.ImageResolution = 200;
            using (pdf)
            {
                using (System.IO.MemoryStream memStream = new System.IO.MemoryStream())
                {
                    string fileName = string.Format("Student info report {0}.pdf", DateTime.UtcNow.Date.ToShortDateString());
                    pdf.Export(rpt.Document, memStream);

                    //UploadedFile file = Repository.ApprovalRequestFileGet(id);
                    return File(memStream.ToArray(), "application/octet-stream", fileName);
                }
            }
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.SchoolUser)]
        public ActionResult StudentFileDownload(int id)
        {
            UploadedFile file = Repository.StudentFileGet(id);
            return File(file.Content.ToArray(), "application/octet-stream", file.Name);
        }

        #endregion SchoolUser

        #region DistrictSysAdmin

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public ActionResult DistrictSysAdmin()
        {
            //get the settings for this users organization
            int orgId = CurrentUser.userOrganizations.FirstOrDefault().organizationId;
            OrganizationSettingsForm orgSettings = Repository.OrganizationSettingsGet(orgId) ??
                                                   Repository.OrganizationSettingsCreate(orgId);

            ViewBag.orgSettings = orgSettings;
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public ActionResult DistrictSysAdminSchools()
        {
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public ActionResult DistrictSysAdminStaff()
        {
            return View();
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult DistrictSysAdminStaffGet()
        {
            List<DistrictStaffs> result = Repository.OrganizationStaffListGet(CurrentUser.id);
            return JSON(new JsonTableResult<DistrictStaffs>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult DistrictSysAdminStudentsGet()
        {
            List<StudentShortForm> result = Repository.OrganizationStudentsGet(CurrentUser.userOrganizations.FirstOrDefault().organizationId);
            return JSON(new JsonTableResult<StudentShortForm>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public ActionResult DistrictSysAdminStudentInfo(int id)
        {
            StudentForm form = Repository.StudentGet(id);
            return View(form);
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult OrganiztionSchoolsGet()
        {
            List<IdTitle> result = Repository.CurrentOrganiztionsSchoolsGet(CurrentUser.userOrganizations.FirstOrDefault().organizationId);
            return JSON(new JsonTableResult<IdTitle>
            {
                Data = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public ActionResult DistrictSysAdminStudents()
        {
            return View();
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult OrganizationSettingsUpdate(OrganizationSettingsForm settings)
        {
            //Updated/Saved settings update database
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "Settings saved successfully", "Organizations Settings SAVE");
            try
            {
                Repository.OrganizationSettingsUpdate(settings);
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message, "Organizations Settings SAVE");
            }
            return JSON(
              new JsonTableResult<OrganizationSettingsForm>
              {
                  Total = 0,
                  //Data = new OrganizationSettingsForm(),
                  Result = result
              });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult OrganizationSettingsSyncTest()
        {
            //test sync call


            //REPLACE WITH API CALL
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS, "Sync Test Successful", "SYNC TEST");
            Random rand = new Random();
            double val = rand.NextDouble();
            if (val > 0.5)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, "Sync Test Failed " + val.ToString(), "SYNC TEST");
            }
            //END API CALL

            return JSON(new JsonTableResult<string>
            {
                Total = 0,
                //Data = new OrganizationSettingsForm(),
                Result = result
            });
        }

        [HttpGet]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult DistrictSysAdminSchoolsGet()
        {
            List<SchoolForm> result = Repository.SchoolsGetByAdminId(CurrentUser.userOrganizations.FirstOrDefault().organizationId);
            return JSON(new JsonTableResult<SchoolForm>
            {
                Data = result
            });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult DistrictSysAdminSchoolsSave(List<UserActionForm> actions)
        {
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            int rows = 0;
            try
            {
                if (!Repository.OrganizationStaffListSave(actions))
                {
                    throw new Exception("Wasn't saved");
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<DistrictStaffs>
              {
                  Total = 1,
                  Data = new List<DistrictStaffs>(),
                  Result = result
              });
        }

        //[HttpGet]
        //public JsonResult DistrictSysAdminStaffGet()
        //{
        //List<SchoolForm> result = Repository.StaffGetByAdminId(CurrentUser.userOrganizations.FirstOrDefault().organizationId);
        //return JSON(new JsonTableResult<UserForm>
        //{
        //    Data = result
        //});
        //}

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult ImportSchools(HttpPostedFileBase importFiles)
        {
            List<IdTitle> resultMessages = new List<IdTitle>();
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            try
            {
                using (Stream inputStream = importFiles.InputStream)
                {
                    MemoryStream memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }

                    byte[] binData = memoryStream.ToArray();

                    string strData = System.Text.Encoding.UTF8.GetString(binData).Replace("\r", string.Empty);
                    string[] lines = strData.Split('\n');
                    List<userOrganization> currentUserOrganizations = CurrentUser.userOrganizations.ToList();
                    int userOrganizationId = -1;
                    if (currentUserOrganizations.Count > 0)
                    {
                        userOrganizationId = currentUserOrganizations.FirstOrDefault().organizationId;
                    }
                    resultMessages = Repository.ImportSchools(lines, userOrganizationId);
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = resultMessages.Count,
                  Data = resultMessages.Select(p => p.Title),
                  Result = result
              });
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult ImportSchoolsRemove(string fileNames)
        {
            //UploadedFile fileList = Session["file"] as UploadedFile ?? new UploadedFile();
            //if (!string.IsNullOrEmpty(fileNames) && fileList != null && fileList.Name == fileNames)
            //{
            //    Session.Remove("file");
            //}

            return new JsonResult();
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult ImportStudents(HttpPostedFileBase importFiles)
        {
            List<IdTitle> resultMessages = new List<IdTitle>();
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            try
            {
                using (Stream inputStream = importFiles.InputStream)
                {
                    MemoryStream memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }

                    byte[] binData = memoryStream.ToArray();

                    string strData = System.Text.Encoding.UTF8.GetString(binData).Replace("\r", string.Empty);
                    string[] lines = strData.Split('\n');
                    List<userOrganization> currentUserOrganizations = CurrentUser.userOrganizations.ToList();
                    int userOrganizationId = -1;
                    if (currentUserOrganizations.Count > 0)
                    {
                        userOrganizationId = currentUserOrganizations.FirstOrDefault().organizationId;
                    }
                    resultMessages = Repository.ImportStudents(lines, userOrganizationId, CurrentUser.FullName);
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = resultMessages.Count,
                  Data = resultMessages.Select(p => p.Title),
                  Result = result
              });

        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult ImportStudentsRemove(string fileNames)
        {
            //UploadedFile fileList = Session["file"] as UploadedFile ?? new UploadedFile();
            //if (!string.IsNullOrEmpty(fileNames) && fileList != null && fileList.Name == fileNames)
            //{
            //    Session.Remove("file");
            //}

            return new JsonResult();
        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult ImportStaff(HttpPostedFileBase importFiles)
        {
            List<IdTitle> resultMessages = new List<IdTitle>();
            JsonExecuteResult result = new JsonExecuteResult(JsonExecuteResultTypes.SUCCESS);
            try
            {
                using (Stream inputStream = importFiles.InputStream)
                {
                    MemoryStream memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }

                    byte[] binData = memoryStream.ToArray();

                    string strData = System.Text.Encoding.UTF8.GetString(binData).Replace("\r", string.Empty);
                    string[] lines = strData.Split('\n');

                    List<userOrganization> currentUserOrganizations = CurrentUser.userOrganizations.ToList();
                    int userOrganizationId = -1;
                    if (currentUserOrganizations.Count > 0)
                    {
                        userOrganizationId = currentUserOrganizations.FirstOrDefault().organizationId;
                    }
                    resultMessages = Repository.ImportStaff(lines, (int)UserRoleEnum.SchoolUser, userOrganizationId, CurrentUser.id);
                }
            }
            catch (Exception ex)
            {
                result = new JsonExecuteResult(JsonExecuteResultTypes.ERROR, ex.Message);
            }
            return JSON(
              new JsonTableResult<string>
              {
                  Total = resultMessages.Count,
                  Data = resultMessages.Select(p => p.Title),
                  Result = result
              });

        }

        [HttpPost]
        [DynamicAuthorize(Role = UserRoleEnum.DistrictSysAdmin)]
        public JsonResult ImportStaffRemove(string fileNames)
        {
            //UploadedFile fileList = Session["file"] as UploadedFile ?? new UploadedFile();
            //if (!string.IsNullOrEmpty(fileNames) && fileList != null && fileList.Name == fileNames)
            //{
            //    Session.Remove("file");
            //}

            return new JsonResult();
        }

        #endregion DistrictSysAdmin

        #region Schools

        /// <summary>
        /// Gets list of schools by district ids
        /// </summary>
        /// <param name="disctrictIds">serialized district id. Delimeter - underline, i.e. 1_2</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult SchoolsGet(string disctrictIds)
        {
            int[] organizationIds = new int[0];
            try
            {
                organizationIds = disctrictIds.Split('_').Select(int.Parse).ToArray();
            }
            catch (Exception)
            {

            }
            List<SchoolFormShort> result = Repository.SchoolsGet(organizationIds);
            return JSON(new JsonTableResult<SchoolFormShort>
            {
                Data = result
            });
        }

        #endregion Schools

        #region Emails
        private bool NewRequestMail(int user, int student)
        {
            try
            {
                NewRequestEmail form = Repository.NewRequestEmailDataGet(user, student);
                foreach (string email in form.Emails)
                {
                    new EmailController().SendEmail(new EmailModel()
                    {
                        From = "sergey.koza4enko@gmail.com",
                        To = email,
                        Subject = "New Support Request",
                        Body = "A Support Request has been requested by " + form.User + " for a student at " + form.School + ".  Please access PCS site to review the request <br/>" + "*this email is an automated message, please do not reply to this email*"
                    }).Deliver();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}