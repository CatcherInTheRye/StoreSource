using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using StoreLib.Model;
using StoreLib.Model.Classes;
using StoreLib.Modules.Application;
using StoreLib.Modules.Autorization;
using StoreLib.Modules.Performance;
using StoreLib.Modules.Route;
using StoreLib.Modules.ValIdation;
using StoreLib.Services.Interface;
using WebMatrix.WebData;
using VStore.Filters;
using VStore.Models;

namespace VStore.Controllers
{
    //[Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : BaseController
    {
        #region Init

        protected IUserService userService;
        
        public AccountController(IUserService userService)
        {
            this.userService = userService;
        }

        private IFormsAuthenticationService FormsService { get; set; }

        protected overrIde void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            if (FormsService == null)
            {
                FormsService = new FormsAuthenticationService();
            }
            base.Initialize(requestContext);
        }

        #endregion Init

        #region Login & Logout
        
        //LoginUser
        private ActionResult LoginUser(User user, bool rememberMe, bool toLogOnPage)
        {
            if (user.Status == Statuses.Locked || user.Status == Statuses.Inactive)
            {
                if (toLogOnPage)
                    return RedirectToAction("Login");
                ViewBag.ErrorType = 1; // is locked
                return View("Login");
            }
            if (user.Status == Statuses.Pending || !user.IsEmailConfirmed)
            {
                if (toLogOnPage)
                    return RedirectToAction("Login");
                ViewBag.ErrorType = 2; // not confirmed
                return View("Login");
            }
            SessionUser cuser = AppSession.CurrentUser;
            if (cuser != null && cuser.Id != user.Id)
            {
                AppSession.UserCart.ClearCart();
            }
            FormsService.SignIn(user.Login, rememberMe, user);
            var jsonController = new JsonController(DependencyResolver.Current.GetService<IGeneralRepository_1_1>(),
                                                            DependencyResolver.Current.GetService<IProductService>(),
                                                            DependencyResolver.Current.GetService<IGeneralServiceOld>(),
                                                            DependencyResolver.Current.GetService<IAuctionRepository>(),
                                                            DependencyResolver.Current.GetService<IInvoiceRepository>(),
                                                            DependencyResolver.Current.GetService<IInvoiceService>(),
                                                            DependencyResolver.Current.GetService<IUserService>(),
                                                            DependencyResolver.Current.GetService<IInvoiceRepository_1_1>());
            jsonController.SyncSessionUserCart(user.Id, user.Group.Id);
            userService.UpdateUserLoginInformation(user, HttpContext.Request.UserHostAddress);
            return null;
        }

        [HttpGet, RequireSslFilter, Compress]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost, RequireSslFilter, Compress]
        public ActionResult Login(string login, string password, string returnUrl)
        {
            if (String.IsNullOrEmpty(login))
            {
                ViewBag.ErrorType = LoginStatus.UsernameRequired;
                return View();
            }
            if (String.IsNullOrEmpty(password))
            {
                ViewBag.Login = login;
                ViewBag.ErrorType = LoginStatus.PasswordRequired;
                return View();
            }
            User trying = userService.GetUser(login, true, false);
            if (trying == null || trying.Password.ToLower() != password.ToLower())
            {
                ViewBag.ErrorType = LoginStatus.WrongPassword; // wrong password
                return View();
            }
            ActionResult ar = LoginUser(trying, true, false);
            if (ar != null) return ar;

            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = HttpContext.Request.UrlReferrer != null && (HttpContext.Request.UrlReferrer.Segments.Count() > 2 && HttpContext.Request.UrlReferrer.Segments[1] != "Account/") ? HttpContext.Request.UrlReferrer.PathAndQuery : String.Empty;

            if (trying.IsNeedToModify)
            {
                Session["redirectUrl"] = returnUrl;
                return RedirectToAction("Profile", "Account");
            }

            if (trying.IsNeedToResetPassword)
            {
                Session["redirectUrl"] = returnUrl;
                return RedirectToAction("ChangePassword", "Account");
            }

            if (Url.IsLocalUrl(returnUrl)) return new RedirectResult(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet, Compress]
        public ActionResult Logoff()
        {
            FormsService.SignOut();
            return RedirectToAction("Index", "Home");
        }

        #endregion Login & Logout

        #region registration & profile

        [HttpPost]
        [AllowAnonymous]
        [ValIdateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValId)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //InitAccountInfo
        private void InitAccountInfo(/*long billingcountry_Id, long? billingstate_Id, long shippingcountry_Id, long? shippingstate_Id, */bool isRegister)
        {
            ViewBag.IsRegister = isRegister;
        }

        //UserUpdate
        private User UserUpdate(RegisterInfoDataAnnotation registerInfo, bool generateconfirmationcode, bool isProfile)
        {
            User user = userService.GetUser(registerInfo.Id, true, false) ?? new User();
            registerInfo.InitUser(user, userService, dictionaryService, isProfile);
            userService.UpdateUser(user, generateconfirmationcode);
            registerInfo.Id = user.Id;
            return new User(user);
        }

        //Register (get)
        //[RequireSslFilter, HttpGet, Compress]
        public virtual ActionResult Register()
        {
            if (AppHelper.CurrentUser != null) return RedirectToAction("Profile");
            InitAccountInfo(1, null, 1, null, null, true);
            return View(new RegisterInfoDataAnnotation { biCustom2 = (long)SubscribeType.Email });
        }

        //Register (post)
        [HttpPost, Compress]
        public virtual ActionResult Register(RegisterInfoDataAnnotation user, string extended)
        {
            RegisterExt regExt = new JavaScriptSerializer().Deserialize<RegisterExt>(extended);
            user.Agree = true;
            user.Password = Server.HtmlEncode(user.Password);
            user.ConfirmPassword = Server.HtmlEncode(user.ConfirmPassword);
            user.IsRegister = true;
            user.ValIdate(ModelState, userService);
            if (ModelState.IsValId)
            {
                User usr = UserUpdate(user, true, false);
                if (usr != null)
                {
                    invoiceRepository.UpdateUserInvoices(usr.Email, usr.Id, string.Empty);
                    List<UserRelationship> rel = new List<UserRelationship>(userRepository.UserRelationshipsGetByUser(usr.Id));
                    List<UserRelationship> r = new List<UserRelationship>();
                    foreach (RelationshipShort relation in regExt.Relationships.Where(t => t.Id != -1))
                    {
                        UserRelationship ur = rel.FirstOrDefault(t => t.Id == relation.Id) ?? new UserRelationship { User_Id = usr.Id };
                        ur.Relationship_Id = relation.Relationship_Id;
                        ur.Name = relation.Name;
                        ur.Email = ValIdationCheck.IsEmail(relation.Email) ? relation.Email : string.Empty;
                        ur = userRepository.UserRelationshipUpdate(ur);
                        r.Add(ur);
                        rel.RemoveAll(t => t.Id == relation.Id);
                    }
                    foreach (UserRelationship relation in rel)
                    {
                        userRepository.UserRelationshipDelete(relation.Id);
                    }
                    List<CalendarEvent> calendarEvents = generalServiceOld.CalendarEventsGet(true);
                    foreach (CalendarEventShort eventShort in regExt.Events)
                    {
                        CalendarEvent ce = calendarEvents.First(t => t.Id == eventShort.Event);
                        UserRelationship ur = r.FirstOrDefault(t => t.User_Id == usr.Id && t.Name == eventShort.Relation);
                        userRepository.UserEventUpdate(new UserEventView { CalendarEvent_Id = eventShort.Event, Frequency_Id = ce.Frequency, Date = eventShort.Date, User_Id = usr.Id, UserRelationship_Id = ur != null ? (long?)ur.Id : null, IsNeedToNotify = true, Note = eventShort.ENote, NotificationFrequencyType_Id = (long)NotificationFrequencyType.d, NotificationFrequency = 1, NotificationStartDate = eventShort.Date.AddDays(-2), NotificationEndDate = eventShort.Date });
                    }
                    List<EmailParamForm> emailParams = generalRepository11.EmailParamsGet(true).Where(t => t.Email_Id.GetValueOrDefault((long)EmailTemplates.RegisterConfirm) == (long)EmailTemplates.RegisterConfirm).ToList();
                    EmailTemplate emailTemplate = generalRepository11.EmailTemplatesGet(string.Empty).First(t => t.Id == (long)EmailTemplates.RegisterConfirm);
                    Mail.SendRegisterConfirmation(emailTemplate.Title, usr, ApplicationHelper.GetSiteUrl("/Account/RegisterFinish/" + usr.ConfirmationCode), emailParams, emailTemplate);
                    return View("RegisterConfirm");
                }
            }
            InitAccountInfo(user.BillingCountry, user.BillingState_Id, user.ShippingCountry, user.ShippingState_Id, regExt, true);
            return View(user);
        }

        //RegisterFinish
        [RequireSslFilter, HttpGet, Compress]
        public ActionResult RegisterFinish(string Id)
        {
            try
            {
                if (String.IsNullOrEmpty(Id)) throw new Exception("The confirmation code can't be null");
                User user = userService.GetUserByConfirmationCode(Id.Trim());
                userService.ConfirmUserEmail(user);
                if (user.Status != UserStatuses.Pending) return RedirectToAction("LogOff");
                userService.ActivatingUser(user);
                userService.DeleteSubscription(user.Email);
                return View("RegisterConfirmSuccess");
            }
            catch (Exception ex)
            {
                Logger.LogException(Id ?? String.Empty, ex);
                return View("RegisterConfirmFail");
            }
        }

        //Profile (get)
        [StoreAuthorize, RequireSslFilter, HttpGet, Compress]
        public virtual ActionResult Profile()
        {
            SessionUser currentuser = AppHelper.CurrentUser;
            User user = userService.GetUser(currentuser.Id, true, true);
            List<Relationship> relationships = generalServiceOld.RelationshipsGet(true).Where(t => t.IsActive).ToList();
            RegisterInfoDataAnnotation registerInfo = new RegisterInfoDataAnnotation(user, userService);
            List<long> calendarEventsIds = generalServiceOld.CalendarEventsGet(true).Where(t => t.IsActive).Select(t => t.Id).ToList();
            RegisterExt regExt = new RegisterExt { Relationships = userRepository.UserRelationshipsGetByUser(registerInfo.Id).Where(t => relationships.FirstOrDefault(r => r.Id == t.Relationship_Id) != null).Select(t => new RelationshipShort { Id = t.Id, Name = t.Name, Email = t.Email, Relationship_Id = t.Relationship_Id, Relationship_Title = relationships.First(r => r.Id == t.Relationship_Id).Title }).ToList(), Events = userRepository.UserEventsGet(registerInfo.Id).ToList().Where(t => calendarEventsIds.Contains(t.CalendarEvent_Id)).Select(t => new CalendarEventShort { Id = t.Id, Date = t.Date, Event = t.CalendarEvent_Id, Relation = t.UserRelationship_Title, ENote = t.Note }).ToList() };
            InitAccountInfo(registerInfo.BillingCountry, registerInfo.BillingState_Id, registerInfo.ShippingCountry, registerInfo.ShippingState_Id, regExt, false);
            return View(registerInfo);
        }

        //Register (post)
        [RequireSslFilter, HttpPost, Compress, StoreAuthorize]
        public virtual ActionResult Profile(RegisterInfoDataAnnotation user, string extended)
        {
            if (AppHelper.CurrentUser.Id != user.Id) return RedirectToAction("Index", "Home");
            User olduser = userService.GetUser(user.Id, true, true);
            string oldemail = olduser.Email;
            RegisterExt regExt = new JavaScriptSerializer().Deserialize<RegisterExt>(extended);
            user.Agree = true;
            user.Password = Server.HtmlEncode(user.Password);
            user.ConfirmPassword = Server.HtmlEncode(user.ConfirmPassword);
            user.ValIdate(ModelState, userService);
            if (ModelState.IsValId)
            {
                user.IsConfirmed = oldemail == user.Email;
                user.IsNeedToModify = false;
                User usr = UserUpdate(user, !user.IsConfirmed, true);
                if (usr != null)
                {
                    if (!user.IsConfirmed)
                    {
                        invoiceRepository.UpdateUserInvoices(usr.Email, user.Id, oldemail);
                        List<EmailParamForm> emailParams = generalRepository11.EmailParamsGet(true).Where(t => t.Email_Id.GetValueOrDefault((long)EmailTemplates.ResendConfirmationCode) == (long)EmailTemplates.ResendConfirmationCode).ToList();
                        EmailTemplate emailTemplate = generalRepository11.EmailTemplatesGet(string.Empty).First(t => t.Id == (long)EmailTemplates.ResendConfirmationCode);
                        Mail.ResendConfirmationCode(emailTemplate.Title, usr, ApplicationHelper.GetSiteUrl("/Account/RegisterFinish/" + usr.ConfirmationCode), emailParams, emailTemplate);
                    }
                    List<UserRelationship> rel = new List<UserRelationship>(userRepository.UserRelationshipsGetByUser(usr.Id));
                    List<UserRelationship> r = new List<UserRelationship>();
                    foreach (RelationshipShort relation in regExt.Relationships.Where(t => t.Id != -1))
                    {
                        UserRelationship ur = rel.FirstOrDefault(t => t.Id == relation.Id) ?? new UserRelationship { User_Id = usr.Id };
                        ur.Relationship_Id = relation.Relationship_Id;
                        ur.Name = relation.Name;
                        ur.Email = ValIdationCheck.IsEmail(relation.Email) ? relation.Email : string.Empty;
                        ur = userRepository.UserRelationshipUpdate(ur);
                        r.Add(ur);
                        rel.RemoveAll(t => t.Id == relation.Id);
                    }
                    foreach (UserRelationship relation in rel)
                    {
                        userRepository.UserRelationshipDelete(relation.Id);
                    }

                    List<CalendarEvent> calendarEvents = generalServiceOld.CalendarEventsGet(true);
                    List<UserEventView> userevents = userRepository.UserEventsGet(user.Id).ToList();
                    foreach (CalendarEventShort eventShort in regExt.Events)
                    {
                        CalendarEvent ce = calendarEvents.First(t => t.Id == eventShort.Event);
                        UserRelationship ur = r.FirstOrDefault(t => t.User_Id == usr.Id && t.Name == eventShort.Relation);
                        UserEventView userEventView = userevents.FirstOrDefault(t => t.Id == eventShort.Id) ?? new UserEventView { CalendarEvent_Id = eventShort.Event, Frequency_Id = ce.Frequency, Date = eventShort.Date, User_Id = usr.Id, IsNeedToNotify = true, NotificationFrequencyType_Id = (long)NotificationFrequencyType.d, NotificationFrequency = 1, NotificationStartDate = eventShort.Date.AddDays(-2), NotificationEndDate = eventShort.Date };
                        userEventView.UserRelationship_Id = ur != null ? (long?)ur.Id : null;
                        userEventView.Note = eventShort.ENote;
                        if (userEventView.CalendarEvent_Id != eventShort.Event)
                        {
                            userEventView.CalendarEvent_Id = eventShort.Event;
                            userEventView.Frequency_Id = ce.Frequency;
                        }
                        if (userEventView.Date != eventShort.Date)
                        {
                            userEventView.Date = eventShort.Date;
                            userEventView.IsNeedToNotify = true;
                            userEventView.NotificationStartDate = eventShort.Date.AddDays(-2);
                            userEventView.NotificationEndDate = eventShort.Date;
                            userEventView.NotificationFrequency = 1;
                            userEventView.NotificationFrequencyType_Id = (long)NotificationFrequencyType.d;
                        }
                        userRepository.UserEventUpdate(userEventView);
                        userevents.RemoveAll(t => t.Id == userEventView.Id);
                    }
                    foreach (UserEventView ue in userevents)
                    {
                        userRepository.UserEventDelete(ue.Id);
                    }
                }
                return View("ProfileUpdate");
            }
            InitAccountInfo(user.BillingCountry, user.BillingState_Id, user.ShippingCountry, user.ShippingState_Id, regExt, true);
            return View(user);
        }

        #endregion registration & profile

        #region forgot password/ resent confirmation / subscribe

        //ForgotPassword (get)
        [HttpGet, Compress]
        public ActionResult ForgotPassword()
        {
            return View(new ForgotPassword());
        }

        //ForgotPassword (post)
        [HttpPost, Compress]
        public ActionResult ForgotPassword(ForgotPassword data)
        {
            if (data == null) return View(new ForgotPassword());
            data.ValIdate(ModelState, userService);
            if (!ModelState.IsValId) return View(data);
            User usr = !string.IsNullOrEmpty(data.Email) ? userService.GetUserByEmail(data.Email, false, true) : userService.GetUser(data.Login, false, true);
            if (usr != null)
            {
                userService.ResetUserPassword(usr);
                List<EmailParamForm> emailParams = generalRepository11.EmailParamsGet(true).Where(t => t.Email_Id.GetValueOrDefault((long)EmailTemplates.PasswordReset) == (long)EmailTemplates.PasswordReset).ToList();
                EmailTemplate emailTemplate = generalRepository11.EmailTemplatesGet(string.Empty).First(t => t.Id == (long)EmailTemplates.PasswordReset);
                Mail.ForgotPassword(emailTemplate.Title, usr, emailParams, emailTemplate);
                return View("ForgotPasswordSend");
            }
            return View(data);
        }

        //ResendConfirmationCode (get)
        [RequireSslFilter, HttpGet, Compress]
        public ActionResult ResendConfirmationCode()
        {
            return View(new ResendConfirmationCode());
        }

        //ResendConfirmationCode (post)
        [HttpPost, Compress]
        public ActionResult ResendConfirmationCode(ResendConfirmationCode data)
        {
            if (data == null) return View(new ResendConfirmationCode());
            data.ValIdate(ModelState, userService);
            if (ModelState.IsValId)
            {
                User usr = !string.IsNullOrEmpty(data.Email) ? userService.GetUserByEmail(data.Email, false, true) : userService.GetUser(data.Login, false, true);
                if (usr == null)
                {
                    ModelState.AddModelError("Email", "Account with this email doesn't exist in the system");
                    ModelState.AddModelError("Login", "Account with this login doesn't exist in the system");
                    return View(data);
                }
                if (String.IsNullOrEmpty(usr.ConfirmationCode) || (usr.ConfirmationCode.Length < 20)) userService.UpdateUser(usr, true, false);
                List<EmailParamForm> emailParams = generalRepository11.EmailParamsGet(true).Where(t => t.Email_Id.GetValueOrDefault((long)EmailTemplates.ResendConfirmationCode) == (long)EmailTemplates.ResendConfirmationCode).ToList();
                EmailTemplate emailTemplate = generalRepository11.EmailTemplatesGet(string.Empty).First(t => t.Id == (long)EmailTemplates.ResendConfirmationCode);
                Mail.ResendConfirmationCode(emailTemplate.Title, usr, ApplicationHelper.GetSiteUrl("/Account/RegisterFinish/" + usr.ConfirmationCode), emailParams, emailTemplate);
                return View("ResendConfirmationCodeSuccess");
            }
            return View(data);
        }

        #endregion forgot password/ resent confirmation / subscribe

        #region my account
        //MyAccount
        [VauctionAuthorize, RequireSslFilter, HttpGet, Compress]
        public ActionResult MyAccount()
        {
            return View();
        }

        #region change password
        //ChangePassword (get)
        [VauctionAuthorize, RequireSslFilter, HttpGet, Compress]
        public ActionResult ChangePassword()
        {
            return View(new ChangePassword());
        }

        //ChangePassword (post)
        [VauctionAuthorize, HttpPost, Compress]
        public ActionResult ChangePassword(ChangePassword data)
        {
            if (data == null) return View(new ChangePassword());
            User user = userService.GetUser(AppHelper.CurrentUser.Id, false, true);
            if (user.Password != data.OldPassword) ModelState.AddModelError("OldPassword", "Old password is not correct.");
            data.ValIdate(ModelState);
            if (!ModelState.IsValId) return View(new ChangePassword());
            user.Password = Server.HtmlEncode(data.Password);
            user.IsNeedToResetPassword = false;
            userService.UpdateUser(user, false, false);
            return View("ChangePasswordSuccess");
        }
        #endregion

        #region order history
        //OrderHistory
        [VauctionAuthorize, RequireSslFilter, HttpGet, Compress]
        public ActionResult OrderHistory()
        {
            return View(invoiceRepository11.OrderHistoryGet(AppHelper.CurrentUser.Id));
        }

        //OrderPreview
        [VauctionAuthorize, RequireSslFilter, HttpGet, Compress]
        public ActionResult OrderPreview(long Id)
        {
            SessionUser user = AppHelper.CurrentUser;
            UserInvoice ui = invoiceRepository.GetUserInvoice(Id);
            if (ui == null || ui.User_Id != user.Id) return RedirectToAction("Index", "Home");
            List<ProductEmail> products;
            Vauction.Library.Core.Data.Model.AddressCard billing;
            Vauction.Library.Core.Data.Model.AddressCard shipping;
            List<PaymentForm> paymentforms;
            List<OrderService> services;
            invoiceRepository11.UserInvoiceDetailsGet(user.Id, Id, out products, out billing, out shipping, out paymentforms, out services, "image_coming_soon.jpg");
            List<GiftCard> gc = productService.GiftCardsGet(true);
            List<IdTitleValue> giftCards = invoiceRepository.UserInvoiceGiftCardsGet().Where(t => t.UserInvoice_Id == ui.Id).ToList().Select(t => new IdTitleValue { Id = t.GiftCard_Id, Title = gc.First(g => g.Id == t.GiftCard_Id).SecureCode, Value = t.Amount }).ToList();
            ViewBag.Payments = paymentforms;
            ViewBag.PaymentsByGift = giftCards;
            ViewBag.DeliveryServices = generalServiceOld.DeliveryDictionariesGet(true);
            ViewBag.BillingAddress = billing;
            ViewBag.ShippingAddress = shipping;
            ViewBag.Shipments = invoiceRepository.GetShipments().Where(t => t.UserInvoices_Id == ui.Id).ToList();
            ViewBag.Products = products;
            ViewBag.Services = services;
            ViewBag.CanCancel = (ui.Status_Id < (byte)UserInvoiceStatuses.Shipped && ApplicationHelper.DateTimeNow.AddMinutes(-45) <= ui.SaleDate);
            List<EmailParamForm> emailParams = generalRepository11.EmailParamsGet(true).ToList();
            EmailTemplate emailTemplate = generalRepository11.EmailTemplatesGet(string.Empty).First(t => t.Id == (long)EmailTemplates.InvoicePaymentConfirmation);
            string cancelLink = Url.Action("OrderPreview", "Account", new { Id = ui.Id });
            ViewBag.PageForPrint = Mail.SendInvoicePaymentConfirmation(false, true, ui.Email, emailTemplate.Title, cancelLink, ui.Id, ui.SaleDate, Address.GetAddress(billing), Address.GetAddress(shipping), paymentforms, giftCards, products, services, emailParams, emailTemplate);

            return View(new IdTitleFlag { Id = ui.Id, Title = ui.SaleDate.ToString(), Flag = ui.Event_Id == 0 });
        }

        //OrderCancel
        [VauctionAuthorize, RequireSslFilter, HttpGet, Compress]
        public ActionResult OrderCancel(long Id)
        {
            SessionUser user = AppHelper.CurrentUser;
            UserInvoice ui = invoiceRepository.GetUserInvoice(Id);
            if (ui == null || ui.User_Id != user.Id) return RedirectToAction("Index", "Home");
            DateTime now = ApplicationHelper.DateTimeNow;
            if (ui.Status_Id == (byte)UserInvoiceStatuses.Fulfilled || ui.Status_Id == (byte)UserInvoiceStatuses.Canceled)
            {
                ViewBag.Message = "Sorry, order are already closed.";
                return View("Error");
            }
            if (ui.Status_Id == (byte)UserInvoiceStatuses.Shipped)
            {
                ViewBag.Message = "Sorry, order are already shipped.";
                return View("Error");
            }
            if (now.AddMinutes(-45) > ui.SaleDate)
            {
                ViewBag.Message = "Sorry, option to cancel order is available for 45 minutes.";
                return View("Error");
            }
            OrderCancelling(ui, now);
            return RedirectToAction("OrderHistory");
        }

        //OrderCancelA
        [HttpGet, Compress]
        public ActionResult OrderCancelA(string ui)
        {
            long userInvoiceId;
            long.TryParse(Encryption.DecryptPassword(ui, ApplicationHelper.EncryptionKey), out userInvoiceId);
            UserInvoice userInvoice = invoiceRepository.GetUserInvoice(userInvoiceId);
            if (userInvoice == null) return RedirectToAction("Index", "Home");
            User user = userService.GetUser(userInvoice.User_Id, false, true);
            SessionUser currentUser = AppHelper.CurrentUser;
            if (user.Id != ApplicationHelper.DefaultRepresentativeId && user.Status != UserStatuses.Pending && (currentUser == null || currentUser.Id != user.Id))
            {
                ViewBag.Message = "You can cancel order on page 'Order History' in 'My Account'.";
                return View("Error");
            }
            DateTime now = ApplicationHelper.DateTimeNow;
            if (userInvoice.Status_Id == (byte)UserInvoiceStatuses.Fulfilled || userInvoice.Status_Id == (byte)UserInvoiceStatuses.Canceled)
            {
                ViewBag.Message = "Sorry, order are already closed.";
                return View("Error");
            }
            if (userInvoice.Status_Id == (byte)UserInvoiceStatuses.Shipped)
            {
                ViewBag.Message = "Sorry, order are already shipped.";
                return View("Error");
            }
            if (now.AddMinutes(-45) > userInvoice.SaleDate)
            {
                ViewBag.Message = "Sorry, option to cancel order is available for 45 minutes.";
                return View("Error");
            }
            return View(userInvoice.Id);
        }

        //OrderCancelA
        [HttpPost, Compress]
        public ActionResult OrderCancelA(long userInvoice_Id)
        {
            UserInvoice ui = invoiceRepository.GetUserInvoice(userInvoice_Id);
            if (ui == null) return RedirectToAction("Index", "Home");
            DateTime now = ApplicationHelper.DateTimeNow;
            if (ui.Status_Id == (byte)UserInvoiceStatuses.Fulfilled || ui.Status_Id == (byte)UserInvoiceStatuses.Canceled)
            {
                ViewBag.Message = "Sorry, order are already closed.";
                return View("Error");
            }
            if (ui.Status_Id == (byte)UserInvoiceStatuses.Shipped)
            {
                ViewBag.Message = "Sorry, order are already shipped.";
                return View("Error");
            }
            if (now.AddMinutes(-45) > ui.SaleDate)
            {
                ViewBag.Message = "Sorry, option to cancel order is available for 45 minutes.";
                return View("Error");
            }
            OrderCancelling(ui, now);
            return View("OrderCanceled", userInvoice_Id);
        }

        //OrderCancelling
        private void OrderCancelling(UserInvoice ui, DateTime now)
        {
            invoiceService.OrderCancelling(ui.User_Id, ui, now, productService);
            Payment payment = invoiceRepository11.PaymentsGet(ui.Id).FirstOrDefault();
            if (payment == null) return;
            Vauction.Library.Core.Data.Model.AddressCard addressCard = generalRepository.GetAddressCard(payment.Billing_AddressCard_Id.GetValueOrDefault(0), true);
            List<EmailParamForm> emailParams = generalRepository11.EmailParamsGet(true).ToList();
            EmailTemplate emailTemplate = generalRepository11.EmailTemplatesGet(string.Empty).First(t => t.Id == (long)EmailTemplates.OrderCancel);
            Mail.SendOrderCancel(ui.Email, emailTemplate.Title, addressCard.FirstName, addressCard.LastName, ui.Id, ui.SaleDate, string.Empty, emailParams, emailTemplate);
        }
        #endregion

        #region wishlist & registries
        //Wishlist
        [VauctionAuthorize, RequireSslFilter, HttpGet, Compress]
        public ActionResult Wishlist()
        {
            SessionUser currentuser = AppHelper.CurrentUser;
            List<WishList> wishLists = productService.WishListsGetByUser(currentuser.Id, true);
            List<WishListItem> wishListItems = productService.WishListItemsGetByUser(currentuser.Id, true);
            List<Tuple<WishList, int>> result = wishLists.Select(t => new Tuple<WishList, int>(t, wishListItems.Count(c => c.WishList_Id == t.Id))).ToList();
            return View(result);
        }

        //Wishlist
        [VauctionAuthorize, RequireSslFilter, HttpGet, Compress]
        public ActionResult WishlistDetail(long Id)
        {
            SessionUser currentuser = AppHelper.CurrentUser;
            WishList wishList = productService.WishListsGetByUser(currentuser.Id, true).FirstOrDefault(t => t.Id == Id);
            if (wishList == null) return RedirectToAction("Wishlist", "Account");
            WishListDetail result = new WishListDetail { WishList = wishList };
            List<WishListItemObject> items = new List<WishListItemObject>();
            foreach (WishListItem wishListItem in productService.WishListItemsGetByUser(currentuser.Id, true).Where(t => t.WishList_Id == wishList.Id))
            {
                object o = null;
                switch ((ObjectTypes)wishListItem.ItemType_Id)
                {
                    case ObjectTypes.Product:
                        ProductShortRange pr = productService.ProductShortRangeGet(wishListItem.Object_Id, currentuser.Group_Id, true);
                        if (pr == null)
                        {
                            productService.WishListItemDelete(currentuser.Id, new List<long> { wishListItem.Id });
                            continue;
                        }
                        o = pr;
                        break;
                    case ObjectTypes.Package:
                        break;
                }
                items.Add(new WishListItemObject { Item = wishListItem, Detail = o });
            }
            result.WishListItem = items;
            ViewBag.CanUpdate = true;
            return View(result);
        }

        //WishlistP
        [RequireSslFilter, HttpGet, Compress]
        public ActionResult WishlistP(string scode)
        {
            SessionUser currentuser = AppHelper.CurrentUser;
            WishList wishList = productService.WishListsGet().FirstOrDefault(t => t.SecureCode == scode);
            if (wishList == null) return RedirectToAction("Index", "Home");
            WishListDetail result = new WishListDetail { WishList = wishList };
            List<WishListItemObject> items = new List<WishListItemObject>();
            foreach (WishListItem wishListItem in productService.WishListItemsGet().Where(t => t.WishList_Id == wishList.Id))
            {
                object o = null;
                switch ((ObjectTypes)wishListItem.ItemType_Id)
                {
                    case ObjectTypes.Product:
                        ProductShortRange pr = productService.ProductShortRangeGet(wishListItem.Object_Id, currentuser != null ? currentuser.Group_Id : 1, true);
                        if (pr == null)
                        {
                            continue;
                        }
                        o = pr;
                        break;
                    case ObjectTypes.Package:
                        break;
                }
                items.Add(new WishListItemObject { Item = wishListItem, Detail = o });
            }
            result.WishListItem = items;
            User owner = userService.GetUser(wishList.User_Id, false, true);
            ViewBag.Owner = owner != null ? owner.FullName : string.Empty;
            return View("WishlistDetail", result);
        }
        #endregion

        #endregion

        #region shopping cart
        //Cart
        [RequireSslFilter, HttpGet, Compress]
        public ActionResult Cart()
        {
            Session["CartPayment"] = null;
            ViewBag.ShowUpdateItem = true;
            ViewBag.HIdeMiniCart = true;
            SessionUser currentuser = AppHelper.CurrentUser;
            if (currentuser != null)
            {
                List<WishList> wishlists = productService.WishListsGetByUser(currentuser.Id, true);
                if (!wishlists.Any()) wishlists.Add(productService.WishListUpdate(new WishList { User_Id = currentuser.Id, DateIn = ApplicationHelper.DateTimeNow, SecureCode = string.Empty, Title = "Default wish list" }));
                ViewBag.WishLists = wishlists;
            }
            return View();
        }

        //ClearCart
        [HttpGet, Compress]
        public ActionResult ClearCart()
        {
            ClearItemsInCart();
            return RedirectToAction("Cart");
        }

        //ClearItemsInCart
        private void ClearItemsInCart()
        {
            SessionUser currentUser = AppHelper.CurrentUser;
            SessionShoppingCart cart = AppHelper.Cart;
            SessionPackageCart packageCart = AppHelper.CartPackage;
            SessionGiftCart giftCart = AppHelper.GiftCart;
            AppHelper.CartPackage.ClearCart();
            if (currentUser != null)
            {
                foreach (long skuId in cart.Lines.Keys) invoiceRepository11.ShoppingCartRemoveItem(new ShoppingCart { User_Id = currentUser.Id, ObjectType_Id = (long)ObjectTypes.Product, Object_Id = skuId });
                foreach (GiftCardLine giftCardLine in giftCart.Lines.Values) invoiceRepository11.ShoppingCartRemoveItem(new ShoppingCart { User_Id = currentUser.Id, ObjectType_Id = (long)ObjectTypes.GiftCard, Object_Id = giftCardLine.SKU_Id, Price = giftCardLine.Price });
            }
            cart.ClearCart();
            packageCart.ClearCart();
            giftCart.ClearCart();
        }
        #endregion

        #region checkout
        //InitCheckoutInfo
        private void InitCheckoutInfo(long billingcountry_Id, long? billingstate_Id, long shippingcountry_Id, long? shippingstate_Id)
        {
            List<State> statesUSA = new List<State>();
            List<Country> usas = dictionaryService.GetCountries(null, string.Empty, "us");
            foreach (Country country in usas) statesUSA.AddRange(dictionaryService.GetStates(null, string.Empty, string.Empty, country.Id));
            ViewBag.USAStatesList = new SelectList(statesUSA, "Id", "Code");

            List<State> statesCanada = new List<State>();
            Country canada = dictionaryService.GetCountries(null, string.Empty, "ca").FirstOrDefault() ?? new Country();
            statesCanada.AddRange(dictionaryService.GetStates(null, string.Empty, string.Empty, canada.Id));
            ViewBag.CanadaStatesList = new SelectList(statesCanada, "Id", "Code");

            ViewBag.BillingStates = billingcountry_Id == 1 ? (billingstate_Id.HasValue) ? new SelectList(statesUSA, "Id", "Code", billingstate_Id.Value) : new SelectList(statesUSA, "Id", "Code") : (billingstate_Id.HasValue) ? new SelectList(statesCanada, "Id", "Code", billingstate_Id.Value) : new SelectList(statesCanada, "Id", "Code");
            ViewBag.ShippingStates = shippingcountry_Id == 1 ? (shippingstate_Id.HasValue) ? new SelectList(statesUSA, "Id", "Code", shippingstate_Id.Value) : new SelectList(statesUSA, "Id", "Code") : (shippingstate_Id.HasValue) ? new SelectList(statesCanada, "Id", "Code", shippingstate_Id.Value) : new SelectList(statesCanada, "Id", "Code");

            ViewBag.CountryList = new SelectList(dictionaryService.GetCountries(null, string.Empty, string.Empty), "Id", "Title");
            ViewBag.HIdeMiniCart = true;
        }

        //ChechoutStep0 (get)
        [RequireSslFilter, HttpGet, Compress]
        public ActionResult CheckoutStep0(string si, string sg)
        {
            CartPayment cartPayment = Session["CartPayment"] as CartPayment ?? new CartPayment();
            SessionUser currentUser = AppHelper.CurrentUser;
            if (currentUser != null)
            {
                cartPayment.User.User_Id = currentUser.Id;
                cartPayment.User.Email = currentUser.Email;
                cartPayment.User.EmailConfirmation = currentUser.Email;
                cartPayment.User.IsNewUser = false;
            }
            if (Session["CartPayment"] != null)
            {
                List<OrderService> services = cartPayment.Services.Where(s => s.IdTitle.Id == (long)StoreServices.Coupon || s.IdTitle.Id == (long)StoreServices.ShippingDiscount).ToList();
                foreach (OrderService coupon in services)
                {
                    cartPayment.Services.Remove(coupon);
                }
                InitCheckoutInfo(cartPayment.AddressBilling.Country_Id, cartPayment.AddressBilling.State_Id, cartPayment.AddressShipping.Country_Id, cartPayment.AddressShipping.State_Id);
                return View(cartPayment);
            }
            SessionShoppingCart cart = AppHelper.Cart;
            SessionPackageCart cartPackage = AppHelper.CartPackage;
            SessionGiftCart giftCart = AppHelper.GiftCart;
            List<long> sItems = !string.IsNullOrEmpty(si) ? new JavaScriptSerializer().Deserialize<List<long>>(si) : new List<long>();
            List<string> sGift = !string.IsNullOrEmpty(sg) ? new JavaScriptSerializer().Deserialize<List<string>>(sg) : new List<string>();
            if (cart.IsEmpty && cartPackage.IsEmpty && giftCart.IsEmpty) return RedirectToAction("Cart");
            List<CartItem> items = new List<CartItem>(cart.Lines.Values.Where(t => !sItems.Contains(t.SKU.IdTitle.Id)));
            //TODO add lines from packages
            AddressCardRegistration billing = new AddressCardRegistration();
            AddressCardRegistration shipping = new AddressCardRegistration();
            cartPayment = new CartPayment(cartPayment.User, items, giftCart.Lines.Values.Where(t => !sGift.Contains(t.UId))) { AddressBilling = billing, AddressShipping = shipping, Event_Id = 0 };
            if (!cartPayment.GiftCardLines.Any() && !cartPayment.ProductLines.Any() && !cartPayment.Services.Any()) return RedirectToAction("Cart");

            if (currentUser != null)
            {
                cartPayment.AddressBilling.FirstName = cartPayment.AddressBilling.FirstName = currentUser.Address_Billing.FirstName;
                cartPayment.AddressBilling.LastName = cartPayment.AddressBilling.LastName = currentUser.Address_Billing.LastName;
                cartPayment.AddressBilling.Address1 = currentUser.Address_Billing.Address1;
                cartPayment.AddressBilling.Address2 = currentUser.Address_Billing.Address2;
                cartPayment.AddressBilling.City = currentUser.Address_Billing.City;
                cartPayment.AddressBilling.State = currentUser.Address_Billing.State.Id == 0 ? currentUser.Address_Billing.InternationalState : string.Empty;
                cartPayment.AddressBilling.Zip = currentUser.Address_Billing.Zip;
                cartPayment.AddressBilling.HomePhone = currentUser.Address_Billing.PhoneHome;
                cartPayment.AddressBilling.WorkPhone = currentUser.Address_Billing.PhoneWork;
                cartPayment.AddressBilling.CellPhone = currentUser.Address_Billing.vCustom1;
                cartPayment.AddressBilling.Fax = currentUser.Address_Billing.Fax;
                cartPayment.AddressBilling.Country_Id = currentUser.Address_Billing.Country.Id;
                cartPayment.AddressBilling.State_Id = currentUser.Address_Billing.State.Id;
                cartPayment.AddressBilling.Company = currentUser.Address_Billing.Company;

                cartPayment.AddressShipping.FirstName = cartPayment.AddressShipping.FirstName = currentUser.Address_Shipping.FirstName;
                cartPayment.AddressShipping.LastName = cartPayment.AddressShipping.LastName = currentUser.Address_Shipping.LastName;
                cartPayment.AddressShipping.Address1 = currentUser.Address_Shipping.Address1;
                cartPayment.AddressShipping.Address2 = currentUser.Address_Shipping.Address2;
                cartPayment.AddressShipping.City = currentUser.Address_Shipping.City;
                cartPayment.AddressShipping.State = currentUser.Address_Shipping.State.Id == 0 ? currentUser.Address_Shipping.InternationalState : string.Empty;
                cartPayment.AddressShipping.Zip = currentUser.Address_Shipping.Zip;
                cartPayment.AddressShipping.HomePhone = currentUser.Address_Shipping.PhoneHome;
                cartPayment.AddressShipping.WorkPhone = currentUser.Address_Shipping.PhoneWork;
                cartPayment.AddressShipping.CellPhone = currentUser.Address_Shipping.vCustom1;
                cartPayment.AddressShipping.Fax = currentUser.Address_Shipping.Fax;
                cartPayment.AddressShipping.Country_Id = currentUser.Address_Shipping.Country.Id;
                cartPayment.AddressShipping.State_Id = currentUser.Address_Shipping.State.Id;
                cartPayment.AddressShipping.Company = currentUser.Address_Shipping.Company;
            }
            InitCheckoutInfo(cartPayment.AddressBilling.Country_Id, cartPayment.AddressBilling.State_Id, cartPayment.AddressShipping.Country_Id, cartPayment.AddressShipping.State_Id);
            Session["CartPayment"] = cartPayment;
            return View(cartPayment);
        }

        //ChechoutStep0 (post)
        [RequireSslFilter, HttpPost, Compress]
        public ActionResult CheckoutStep0(CartPayment cartPayment)
        {
            CartPayment cp = Session["CartPayment"] as CartPayment;
            if (cp == null) return RedirectToAction("Cart");
            cartPayment.AddressBilling.ValIdate(ModelState, "AddressBilling.");
            if (cartPayment.BillingLikeShipping) cartPayment.AddressShipping = new AddressCardRegistration(cartPayment.AddressBilling);
            cartPayment.AddressShipping.ValIdate(ModelState, "AddressShipping.");
            long reguser_Id = CartPaymentUserValIdate(cartPayment.User, ModelState);
            if (!ModelState.IsValId)
            {
                InitCheckoutInfo(cartPayment.AddressBilling.Country_Id, cartPayment.AddressBilling.State_Id, cartPayment.AddressShipping.Country_Id, cartPayment.AddressShipping.State_Id);
                return View(cartPayment);
            }
            SessionUser currentUser = AppHelper.CurrentUser;
            bool isTaxable = currentUser == null || currentUser.IsTaxable;
            long usergroup_Id = currentUser != null ? currentUser.Group_Id : 1;
            if (currentUser == null)
            {
                if (reguser_Id > 0)
                {
                    cp.User.User_Id = reguser_Id;
                    cp.User.IsNewUser = true;
                }
                cp.User.Email = cartPayment.User.Email;
                cp.User.EmailConfirmation = cartPayment.User.EmailConfirmation;
            }
            cp.AddressBilling = cartPayment.AddressBilling;
            cp.AddressShipping = cartPayment.AddressShipping;
            cp.BillingLikeShipping = cartPayment.BillingLikeShipping;
            decimal taxServiceValue = 0;
            decimal tiersServiceValue = 0;
            StoreForm defaultStore = ApplicationHelper.SaleDataFromDefaultStore ? generalRepository11.StoresGet(true).First(t => t.IsDefault) : generalRepository11.StoresGet(true).First(t => t.IsDefault); //TODO поиск ближайшего магазина
            List<TaxRuleDetail> taxRules = generalServiceOld.TaxRulesGet(true).Where(t => t.TaxClass_Id == defaultStore.TaxClass_Id).ToList();
            foreach (CartItem cartItem in cp.ProductLines)
            {
                List<Tier> tiers = productService.GetProductTiers(cartItem.Product.Product.Id, usergroup_Id, true).OrderBy(t => t.LowerRange).ToList();
                if (!tiers.Any()) tiers = productService.GetCategoryTiersForProduct(cartItem.Product.Product.Id, usergroup_Id, true).OrderBy(t => t.LowerRange).ToList();
                int productQty = cp.ProductLines.Where(t => t.Product.Product.Id == cartItem.Product.Product.Id).Sum(t => t.TotalQty);
                Tier tierQty = tiers.Where(t => t.LowerRange <= productQty).OrderBy(t => t.LowerRange).LastOrDefault();
                if (tierQty != null)
                {
                    decimal maxTierDiscount = tiers.Where(t => t.LowerRange == tierQty.LowerRange).Select(tier => tier.IsPercent ? (cartItem.SKU.FinalPrice * tier.Value / 100).GetPrice() : Math.Min(cartItem.SKU.FinalPrice, tier.Value.GetValueOrDefault(0)).GetPrice()).Concat(new decimal[] { 0 }).Max();
                    cartItem.TiersDiscount = maxTierDiscount * cartItem.TotalQty;
                    tiersServiceValue += cartItem.TiersDiscount;
                }
                if (isTaxable && cartItem.Product.IsTaxable)
                {
                    long country_Id = cartItem.Product.TaxByShipping ? cp.AddressShipping.Country_Id : cp.AddressBilling.Country_Id;
                    long state_Id = cartItem.Product.TaxByShipping ? cp.AddressShipping.State_Id : cp.AddressBilling.State_Id;
                    string zip = cartItem.Product.TaxByShipping ? cp.AddressShipping.Zip : cp.AddressBilling.Zip;
                    List<TaxRuleDetail> tr = taxRules.Where(t => t.TaxShipping == cartItem.Product.TaxByShipping && t.DestinationState_Id == state_Id && t.DestinationCountry_Id == country_Id).ToList();
                    TaxRuleDetail taxRule = tr.FirstOrDefault(t => (t.DestinationZip ?? string.Empty).ToLower().Trim() == zip.ToLower().Trim()) ?? tr.FirstOrDefault(t => string.IsNullOrEmpty(t.DestinationZip));
                    if (taxRule == null) continue;
                    decimal taxValue = (cartItem.TotalPrice - cartItem.Discount - cartItem.CouponDiscount - cartItem.TiersDiscount) * taxRule.SalesTax * (decimal)0.01;
                    taxServiceValue += taxValue;
                }
            }
            OrderService tax = cp.Services.FirstOrDefault(s => s.IdTitle.Id == (long)StoreServices.Tax);
            if (tax == null)
            {
                cp.Services.Add(new OrderService { IdTitle = new IdTitle { Id = (long)StoreServices.Tax, Title = StoreServices.Tax.ToString() }, Value = taxServiceValue.GetPrice(), IsAddInInvoice = true, Adding = true });
            }
            else
            {
                tax.Value = taxServiceValue.GetPrice();
            }
            OrderService tiersService = cp.Services.SingleOrDefault(s => s.IdTitle.Id == (long)StoreServices.SpecialOffersDiscount);
            if (tiersService == null)
            {
                cp.Services.Add(new OrderService { IdTitle = new IdTitle { Id = (long)StoreServices.SpecialOffersDiscount, Title = AppHelper.SplitCamelCase(StoreServices.SpecialOffersDiscount.ToString()) }, IsAddInInvoice = true, Value = tiersServiceValue, Adding = false });
            }
            else
            {
                tiersService.Value = tiersServiceValue;
            }
            cp.CartGuId = GuId.NewGuId().ToString().Replace("-", "");
            Session["CartPayment"] = cp;
            return RedirectToAction("CheckoutStep1");
        }

        //CheckoutStep1 (get)
        [RequireSslFilter, HttpGet, Compress]
        public ActionResult CheckoutStep1(string message)
        {
            CartPayment cartPayment = Session["CartPayment"] as CartPayment;
            if (cartPayment == null) return RedirectToAction("Cart");
            if (!cartPayment.Payments.Any()) cartPayment.Payments.Add(new PaymentForm { CardHolderName = string.Format("{0} {1}", cartPayment.AddressBilling.FirstName, cartPayment.AddressBilling.LastName), IsActive = true, PaymentType = (long)PaymentTypes.CreditCard });
            if (cartPayment.ProductLines.Any())
            {
                List<DeliveryShipping> shippingList = InitShippingServices(cartPayment);
                if (shippingList.Count == 0)
                {
                    //shippingList.Add(new DeliveryShipping { DeliveryId = 0, FullDescription = "Some data", Price = 10 });
                    const string error = "Shipping services aren't available or your address isn't correct.";
                    return RedirectToAction("CheckoutFailed", new { error });
                }
                ViewBag.ShippingList = shippingList;
                DeliveryShipping dsh = shippingList.FirstOrDefault(t => t.DeliveryId == cartPayment.DeliveryService_Id) ?? shippingList.First();
                cartPayment.DeliveryService_Id = dsh.DeliveryId;
                OrderService shipping = cartPayment.Services.FirstOrDefault(s => s.IdTitle.Id == (long)StoreServices.Shipping);
                if (shipping == null)
                {
                    cartPayment.Services.Add(new OrderService { IdTitle = new IdTitle { Id = (long)StoreServices.Shipping, Title = StoreServices.Shipping.ToString() }, Value = dsh.Price, IsAddInInvoice = true, Adding = true });
                }
                else
                {
                    shipping.Value = dsh.Price;
                }
            }
            ViewBag.vDiscount = invoiceRepository.ValIdDiscountForUserId(cartPayment.User.User_Id);
            ViewBag.Message = message;
            ViewBag.HIdeMiniCart = true;
            return View(cartPayment);

        }

        //CheckoutStep1 (post)
        [RequireSslFilter, HttpPost, Compress]
        public ActionResult CheckoutStep1(CartPayment cartPayment)
        {
            CartPayment cp = Session["CartPayment"] as CartPayment;
            if (cp == null || !cartPayment.Payments.Any()) return RedirectToAction("Cart");
            cp.Payments = cartPayment.Payments;
            cp.Payments[0].Amount = cp.TotalDue - cp.GiftCardPayments.Sum(t => t.Amount);
            switch (cp.Payments[0].CardTypeTitle)
            {
                case "amex": cp.Payments[0].CardType_Id = (long)CreditCardTypes.AmericanExpress; break;
                case "diners_club_carte_blanche":
                case "diners_club_international": cp.Payments[0].CardType_Id = (long)CreditCardTypes.DinersClub; break;
                case "discover": cp.Payments[0].CardType_Id = (long)CreditCardTypes.Discover; break;
                case "jcb": cp.Payments[0].CardType_Id = (long)CreditCardTypes.JCB; break;
                case "laser": cp.Payments[0].CardType_Id = (long)CreditCardTypes.Laser; break;
                case "visa_electron": cp.Payments[0].CardType_Id = (long)CreditCardTypes.VisaElectron; break;
                case "visa": cp.Payments[0].CardType_Id = (long)CreditCardTypes.Visa; break;
                case "mastercard": cp.Payments[0].CardType_Id = (long)CreditCardTypes.MasterCard; break;
                case "maestro": cp.Payments[0].CardType_Id = (long)CreditCardTypes.Maestro; break;
                default: cp.Payments[0].CardType_Id = (long)CreditCardTypes.Other; break;
            }
            foreach (PaymentForm payment in cp.Payments)
            {
                if (payment.IsActive)
                {
                    payment.ValIdate();
                }
                else
                {
                    payment.IsValId = true;
                }
            }
            if (cp.Payments.Any(t => !t.IsValId))
            {
                return RedirectToAction("CheckoutStep1");
            }
            return RedirectToAction("CheckoutStep2");
        }

        //CheckoutStep2 (get)
        [RequireSslFilter, HttpGet, Compress]
        public ActionResult CheckoutStep2()
        {
            CartPayment cartPayment = Session["CartPayment"] as CartPayment;
            if (cartPayment == null) return RedirectToAction("Cart");
            State state = dictionaryService.GetState(cartPayment.AddressBilling.State_Id) ?? new State();
            ViewBag.BillingState = state.Id > 0 ? state.Code : cartPayment.AddressBilling.State;
            state = dictionaryService.GetState(cartPayment.AddressShipping.State_Id) ?? new State();
            ViewBag.ShippingState = state.Id > 0 ? state.Code : cartPayment.AddressShipping.State;
            ViewBag.ShippingMethod = generalServiceOld.DeliveryDictionariesGet(true).FirstOrDefault(t => t.Id == cartPayment.DeliveryService_Id);
            ViewBag.HIdeMiniCart = true;
            return View(cartPayment);
        }

        //CheckoutPayment
        [RequireSslFilter, HttpPost, Compress]
        public ActionResult CheckoutPayment()
        {
            try
            {
                CartPayment cartPayment = Session["CartPayment"] as CartPayment;
                if (cartPayment == null) return RedirectToAction("Cart");
                CartPaymentValIdateGiftCards(cartPayment.GiftCardPayments);
                if (cartPayment.GiftCardPayments.Any(t => !t.IsValId)) return RedirectToAction("CheckoutStep1");
                if (cartPayment.User.User_Id == 0)
                {
                    cartPayment.User.IsNewUser = true;
                    if (ApplicationHelper.CreateAnonymousUser) CheckoutCreateUser(cartPayment); else cartPayment.User.User_Id = ApplicationHelper.DefaultRepresentativeId;
                }
                UserInvoice userInvoice = invoiceRepository.UpdateUserInvoice(new UserInvoice { Event_Id = 0, User_Id = cartPayment.User.User_Id, SaleDate = ApplicationHelper.DateTimeNow, Email = cartPayment.User.Email });
                cartPayment.PaymentDate = userInvoice.SaleDate;
                decimal gifts = cartPayment.GiftCardPayments.Sum(t => t.Amount).GetPrice();
                if (gifts < cartPayment.TotalDue)
                {
                    Vauction.Library.Core.Model.AddressCard ac = dictionaryService.AddressCardInit(cartPayment.AddressShipping);
                    Address address = Address.GetAddress(ac);
                    foreach (PaymentForm payment in cartPayment.Payments.Where(t => t.IsActive && t.PaymentType == (long)PaymentTypes.CreditCard))
                    {
                        try
                        {
                            long profile_Id = AuthorizeNet.CreateCustomerProfile(cartPayment.User.User_Id, cartPayment.User.Email, "Payment for items");
                            if (profile_Id <= 0)
                            {
                                payment.IsValId = false;
                                payment.Errors += AuthorizeNet.LastError;
                            }
                            CustomerProfileMaskedType profile = AuthorizeNet.GetCustomerProfile(profile_Id);
                            if (profile == null)
                            {
                                payment.IsValId = false;
                                payment.Errors += AuthorizeNet.LastError;
                            }
                            long shipping_profile_Id = AuthorizeNet.CreateCustomerShippingAddress(profile_Id, address, address.Country);
                            if (shipping_profile_Id <= 0)
                            {
                                payment.IsValId = false;
                                payment.Errors += AuthorizeNet.LastError;
                            }
                            CreditCardInfo cci = new CreditCardInfo { CardCode = payment.CardCode, CardNumber = payment.CardNumber, ExpirationMonth = payment.ExpirationMonth, ExpirationYear = payment.ExpirationYear };
                            long payment_profile_Id = AuthorizeNet.CreateCustomerPaymentProfile(profile_Id, cci);
                            if (payment_profile_Id <= 0)
                            {
                                payment.IsValId = false;
                                payment.Errors += AuthorizeNet.LastError;
                            }
                            profile = AuthorizeNet.GetCustomerProfile(profile_Id);
                            if (profile == null)
                            {
                                payment.IsValId = false;
                                payment.Errors += AuthorizeNet.LastError;
                            }
                            if (!AuthorizeNet.ValIdateCustomerPaymentProfile(profile_Id, payment_profile_Id, shipping_profile_Id, cci.CardCode))
                            {
                                payment.IsValId = false;
                                payment.Errors += AuthorizeNet.LastError;
                            }
                            if (!payment.IsValId) continue;
                            payment.transactionId = AuthorizeNet.CreateTransaction(profile_Id, payment_profile_Id, shipping_profile_Id, payment.Amount.GetPrice(), userInvoice.Id.ToString(CultureInfo.InvariantCulture), false, cci, "Payment for items (" + ApplicationHelper.DateTimeNowString + ")");
                        }
                        catch (Exception ex)
                        {
                            payment.IsValId = false;
                            payment.Errors += ex.Message;
                        }
                    }
                    if (cartPayment.Payments.Any(p => !p.IsValId))
                    {
                        foreach (PaymentForm payment in cartPayment.Payments.Where(p => p.IsActive && p.PaymentType == (long)PaymentTypes.CreditCard && !string.IsNullOrEmpty(p.transactionId)))
                        {
                            long profile_Id = AuthorizeNet.CreateCustomerProfile(cartPayment.User.User_Id, cartPayment.User.Email, string.Empty);
                            if (profile_Id > 0) AuthorizeNet.RefundTransaction(profile_Id, payment.Amount.GetPrice(), payment.CardNumber.Substring(1, 4), payment.transactionId, string.Empty);
                        }
                        const string message = "Checkout Failed. Please correct the errors and try again.";
                        return RedirectToAction("CheckoutStep1", new { message });
                    }
                    foreach (PaymentForm payment in cartPayment.Payments.Where(p => p.IsActive && p.PaymentType == (long)PaymentTypes.Cash))
                    {
                        payment.CardType_Id = null;
                        payment.transactionId = "Amount " + payment.Amount.GetCurrency(false);
                    }
                    foreach (PaymentForm payment in cartPayment.Payments.Where(p => p.IsActive && p.PaymentType == (long)PaymentTypes.Check))
                    {
                        payment.CardType_Id = null;
                        payment.transactionId = payment.CheckNumber;
                    }
                    foreach (PaymentForm payment in cartPayment.Payments.Where(p => p.IsActive && p.PaymentType == (long)PaymentTypes.PurchaseOrder))
                    {
                        payment.CardType_Id = null;
                        payment.CardNumber = payment.POFileName;
                        payment.transactionId = payment.PONumber;
                    }
                }
                Vauction.Library.Core.Model.AddressCard addressCard = dictionaryService.AddressCardInit(cartPayment.AddressBilling);
                Address billingAddress = Address.GetAddress(addressCard);
                addressCard = dictionaryService.AddressCardInit(cartPayment.AddressShipping);
                Address shippingAddress = Address.GetAddress(addressCard);
                List<IdTitleValue> paymentGiftCards = cartPayment.GiftCardPayments.Select(t => new IdTitleValue { Id = t.Id, Title = t.SecureCode, Value = t.Amount }).ToList();
                cartPayment.UserInvoice_Id = productService.AddPaymentForCartItems(userInvoice.Id, cartPayment.User.User_Id, cartPayment.User.Email, cartPayment.ProductLines, cartPayment.GiftCardLines, cartPayment.Services, billingAddress, shippingAddress, ApplicationHelper.DateTimeNow, cartPayment.DeliveryService_Id, new List<long>(), cartPayment.Payments, paymentGiftCards, null);
                List<ProductEmail> product = cartPayment.ProductLines.Select(t => new ProductEmail { ProductTitle = t.Product.Product.Title, Quantity = t.TotalQty, Price = t.SKU.FinalPrice, Discount = t.Discount, CouponDiscount = t.CouponDiscount, SkuAttributes = t.AttributesLine, SKUTitle = t.SKU.IdTitle.Title, ThumbnailImage = !string.IsNullOrEmpty(t.Product.ThumbnailImage) ? ApplicationHelper.Urls.ProductImage(t.Product.Product.Id, t.Product.ThumbnailImage) : ApplicationHelper.Urls.CompressImageFrontEnd("image_coming_soon.jpg") }).ToList();
                product.AddRange(cartPayment.GiftCardLines.Select(t => new ProductEmail { ProductTitle = t.ProductTitle, SKUTitle = t.SKU, Quantity = t.Quantity, Price = t.Price, Discount = 0, SkuAttributes = string.Empty, ThumbnailImage = !string.IsNullOrEmpty(t.Image.MediumImage) ? ApplicationHelper.Urls.ProductImage(t.Product_Id, t.Image.MediumImage, 182, 182) : ApplicationHelper.Urls.CompressImageFrontEnd("image_coming_soon.jpg") }));
                List<EmailParamForm> emailParams = generalRepository11.EmailParamsGet(true).ToList();
                EmailTemplate emailTemplate = generalRepository11.EmailTemplatesGet(string.Empty).First(t => t.Id == (long)EmailTemplates.InvoicePaymentConfirmation);
                string cancelLink = string.Format("{0}{1}", ApplicationHelper.SiteUrl, cartPayment.User.IsNewUser ? Url.Action("OrderCancelA", "Account", new { ui = Encryption.EncryptPassword(cartPayment.UserInvoice_Id.ToString(), ApplicationHelper.EncryptionKey) }) : Url.Action("OrderPreview", "Account", new { Id = cartPayment.UserInvoice_Id }));
                cartPayment.PageForPrint = Mail.SendInvoicePaymentConfirmation(true, true, cartPayment.User.Email, emailTemplate.Title, cancelLink, cartPayment.UserInvoice_Id, cartPayment.PaymentDate, billingAddress, shippingAddress, cartPayment.Payments.Where(t => t.IsActive), paymentGiftCards, product, cartPayment.Services.Where(t => t.Value > 0).ToList(), emailParams, emailTemplate);
                foreach (CartItem item in cartPayment.ProductLines)
                {
                    AppHelper.Cart.RemoveItem(item.SKU.IdTitle.Id);
                    invoiceRepository11.ShoppingCartRemoveItem(new ShoppingCart { User_Id = cartPayment.User.User_Id, ObjectType_Id = (long)ObjectTypes.Product, Object_Id = item.SKU.IdTitle.Id });
                }
                foreach (GiftCardLine item in cartPayment.GiftCardLines)
                {
                    AppHelper.GiftCart.RemoveItem(item.UId);
                    invoiceRepository11.ShoppingCartRemoveItem(new ShoppingCart { User_Id = cartPayment.User.User_Id, ObjectType_Id = (long)ObjectTypes.GiftCard, Object_Id = item.SKU_Id, Price = item.Price });
                }
                Tracking tracking = new Tracking { User_Id = cartPayment.User.User_Id, ObjectType_Id = (long)TrackingObjectTypes.UserInvoiceStatus, Object_Id = cartPayment.UserInvoice_Id, biCustom1 = null, biCustom2 = (byte)UserInvoiceStatuses.PaId, DateIn = cartPayment.PaymentDate };
                generalRepository.UpdateTracking(tracking);
                ViewBag.HIdeMiniCart = true;
                return View("CartPaymentConfirmation", cartPayment);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex.InnerException.Message);
                return RedirectToAction("CheckoutFailed");
            }
        }

        [RequireSslFilter, HttpGet, Compress]
        public ActionResult CheckoutFailed(string error)
        {
            ViewBag.Error = error;
            return View();
        }

        //CartPaymentValIdateGiftCards
        private void CartPaymentValIdateGiftCards(List<CartPaymentGiftCard> giftCards)
        {
            List<GiftCard> gc = productService.GiftCardsGet(true);
            foreach (CartPaymentGiftCard giftCard in giftCards)
            {
                GiftCard g = gc.FirstOrDefault(t => t.Id == giftCard.Id);
                if (g == null)
                {
                    giftCard.IsValId = false;
                    giftCard.Error = "Gift Card does not exist.";
                    return;
                }
                if (g.Status_Id == (long)GiftCardStatuses.Inactive || g.Status_Id == (long)GiftCardStatuses.Redeemed)
                {
                    giftCard.IsValId = false;
                    giftCard.Error = "Gift Card is not valId more.";
                }
                if (g.RemainingAmount < giftCard.Amount)
                {
                    giftCard.IsValId = false;
                    giftCard.Error = "Gift Card Remaining Amount is not enough.";
                }
            }
        }

        //CartPaymentUserValIdate
        private long CartPaymentUserValIdate(CartPaymentUser cartPaymentUser, ModelStateDictionary modelState)
        {
            if (!String.Equals(cartPaymentUser.Email, cartPaymentUser.EmailConfirmation, StringComparison.Ordinal))
            {
                modelState.AddModelError("User.Email", "The Email and confirmation Email do not match.");
            }
            User user = userService.GetUserByEmail(cartPaymentUser.Email, false, true);
            if (user == null || user.Id == cartPaymentUser.User_Id) return -1;
            if (user.Status == UserStatuses.Pending)
            {
                return user.Id;
            }
            modelState.AddModelError("User.Email", "Account with this email address is already registered.");
            return -1;
        }

        private void CheckoutCreateUser(CartPayment cartPayment)
        {
            PasswordGenerator passwordGenerator = new PasswordGenerator(7, 8);
            Core.Model.AddressCard billing = dictionaryService.AddressCardInit(cartPayment.AddressBilling);
            dictionaryService.UpdateAddressCard(billing);
            Core.Model.AddressCard shipping = dictionaryService.AddressCardInit(cartPayment.AddressShipping);
            dictionaryService.UpdateAddressCard(shipping);
            User user = new User
            {
                FirstName = billing.FirstName,
                LastName = billing.LastName,
                RegistrationDate = ApplicationHelper.DateTimeNow.Date,
                Email = cartPayment.User.Email,
                BirthDate = ApplicationHelper.DateTimeNow.Date,
                IsNeedToModify = true,
                IsTaxable = true,
                IsEmailConfirmed = false,
                Login = cartPayment.User.Email,
                Password = passwordGenerator.Generate(),
                PaymentTerm = new PaymentTerm { Id = ApplicationHelper.DefaultCommission },
                AddressBilling = billing,
                AddressShipping = shipping,
                Type = UserTypes.Buyer,
                Status = UserStatuses.Pending,
                Group = new Core.Model.UserGroup { Id = userService.GetDefaultRegistrationGroup().Id },
                Gender = Genders.Male,
                Commission = new Core.Model.CommissionRate { Id = ApplicationHelper.DefaultCommission }
            };
            userService.UpdateUser(user, true, true);
            cartPayment.User.User_Id = user.Id;
        }

        //InitShippingServices
        private List<DeliveryShipping> InitShippingServices(CartPayment cartPayment)
        {
            StoreForm defaultStore = ApplicationHelper.SaleDataFromDefaultStore ? generalRepository11.StoresGet(true).First(t => t.IsDefault) : generalRepository11.StoresGet(true).First(t => t.IsDefault); //TODO поиск ближайшего магазина
            Dictionary<long, Dictionary<int, string>> upsServiceCode;
            Dictionary<long, Dictionary<int, string>> uspsServiceCode;
            switch (cartPayment.AddressShipping.Country_Id)
            {
                case 1:
                    upsServiceCode = generalRepository.GetDeliveryInternalServiceDicList((int)DeliveryService.UPS, (int)DeliveryServiceDestination.USA);
                    uspsServiceCode = generalRepository.GetDeliveryInternalServiceDicList((int)DeliveryService.USPS, (int)DeliveryServiceDestination.USA);
                    break;
                default:
                    upsServiceCode = generalRepository.GetDeliveryInternalServiceDicList((int)DeliveryService.UPS, (int)DeliveryServiceDestination.AllInternational);
                    uspsServiceCode = generalRepository.GetDeliveryInternalServiceDicList((int)DeliveryService.USPS, (int)DeliveryServiceDestination.AllInternational);
                    break;
            }
            Country country = dictionaryService.GetCountry(cartPayment.AddressShipping.Country_Id);
            List<DeliveryShipping> result = invoiceRepository.GetDeliveryServicesList(cartPayment.CartGuId, cartPayment.ProductLines.Sum(l => ((l.SKU.WeightLBS + l.SKU.WeightOZ / 16) * l.Quantity)), cartPayment.ProductLines.Sum(l => l.TotalPrice).GetPrice(), ApplicationHelper.UPSDisable, ApplicationHelper.UPSAccessKey, ApplicationHelper.UPSUserName, ApplicationHelper.UPSUserPassword, ApplicationHelper.UPSAccountNumber, new string[] { defaultStore.AddressCard.Address12 }, defaultStore.AddressCard.City, defaultStore.AddressCard.Zip, dictionaryService.GetState(defaultStore.AddressCard.State_Id).Code, dictionaryService.GetCountry(defaultStore.AddressCard.Country_Id).Code, new string[] { cartPayment.AddressShipping.Address1 + " " + cartPayment.AddressShipping.Address2 }, cartPayment.AddressShipping.City, cartPayment.AddressShipping.Zip, cartPayment.AddressShipping.State_Id > 0 ? dictionaryService.GetState(cartPayment.AddressShipping.State_Id).Code : cartPayment.AddressShipping.State, cartPayment.AddressShipping.Country_Id, country.Title, country.Code, upsServiceCode, ApplicationHelper.USPSDisable, ApplicationHelper.USPSUserId, uspsServiceCode);
            List<DictionaryDeliveryService> deliveryServices = generalServiceOld.DeliveryDictionariesGet(true);
            foreach (DeliveryShipping deliveryShipping in result)
            {
                DictionaryDeliveryService ds = deliveryServices.FirstOrDefault(d => d.Id == deliveryShipping.DeliveryId);
                if (ds == null) continue;
                decimal oldPrice = deliveryShipping.Price;
                switch (ds.DeliveryServiceId)
                {

                    case 0:
                        deliveryShipping.Price = Math.Max(deliveryShipping.Price * (1 - ApplicationHelper.UPS_discount / 100), 0).GetPrice();
                        deliveryShipping.FullDescription = deliveryShipping.FullDescription.Replace(oldPrice.ToString(), deliveryShipping.Price.ToString());
                        break;
                    case 1:
                        deliveryShipping.Price = Math.Max(deliveryShipping.Price * (1 - ApplicationHelper.USPS_discount / 100), 0).GetPrice();
                        deliveryShipping.FullDescription = deliveryShipping.FullDescription.Replace(oldPrice.ToString(), deliveryShipping.Price.ToString());
                        break;
                }
            }
            result = result.OrderBy(sl => sl.Price).ToList();
            return result;
        }
        #endregion

        #region open orders
        //OpenOrderI
        [HttpGet, Compress]
        public ActionResult OpenOrderI(string Id)
        {
            string errorMsg = string.Format("This order does not exist or already closed. If you have questions or need assistance, please <a href='{0}'>contact us</a>.", Url.Action("ContactUs", "Home"));
            CustomerOrder customerOrder = invoiceRepository.CustomerOrdersGet().FirstOrDefault(t => t.Order_Id == Id && t.IsSaved);
            if (customerOrder == null || !customerOrder.BillingAddressCard_Id.HasValue || !customerOrder.ShippingAddressCard_Id.HasValue)
            {
                ViewBag.Message = errorMsg;
                return View("Error");
            }
            Order order = new Order(customerOrder.Order_Id);
            User orderUser = userService.GetUser(customerOrder.User_Id.GetValueOrDefault(0), true, true);
            if (orderUser == null)
            {
                ViewBag.Message = errorMsg;
                return View("Error");
            }
            order.Address_Billing = Address.GetAddress(generalRepository.GetAddressCard(customerOrder.BillingAddressCard_Id.Value, false));
            order.Address_Shipping = Address.GetAddress(generalRepository.GetAddressCard(customerOrder.ShippingAddressCard_Id.Value, false));
            order.Products = invoiceRepository11.OrderProductsGetList(order.Id);
            foreach (CustomerOrderService t in invoiceRepository.OrderServicesGet().Where(t => t.CustomerOrder_Id == customerOrder.Id))
            {
                OrderService orderService = order.Services.FirstOrDefault(s => s.IdTitle.Id == t.Service_Id && s.IsAddInInvoice == t.IsAddInInvoice);
                if (orderService == null)
                {
                    order.Services.Add(new OrderService { Adding = t.Adding, Value = t.Value.GetPrice(), IsAddInInvoice = t.IsAddInInvoice, IdTitle = new IdTitle { Id = t.Service_Id, Title = t.Service.Name } });
                }
                else
                {
                    orderService.IsAddInInvoice = t.IsAddInInvoice;
                    orderService.Adding = t.Adding;
                    orderService.Value = t.Value.GetPrice();
                }
            }
            if (!order.Products.Any() && !order.Services.Any())
            {
                ViewBag.Message = errorMsg;
                return View("Error");
            }
            order.Products.ForEach(t => t.Thumbnail = !string.IsNullOrEmpty(t.Thumbnail) ? ApplicationHelper.Urls.CompressProductImage(t.Product_Id, t.Thumbnail) : ApplicationHelper.Urls.CompressImageFrontEnd("image_coming_soon.jpg"));
            OrderService tax = order.ServicesToInvoice.FirstOrDefault(s => s.IdTitle.Id == (long)StoreServices.Tax);
            if (orderUser.IsTaxable)
            {
                bool saleDataFromDefaultStore = ApplicationHelper.SaleDataFromDefaultStore;
                StoreForm defaultStore = saleDataFromDefaultStore ? generalRepository11.StoresGet(true).First(t => t.IsDefault) : generalRepository11.StoresGet(true).First(t => t.IsDefault); //TODO поиск ближайшего магазина
                List<TaxRuleDetail> taxRules = generalServiceOld.TaxRulesGet(true).Where(t => t.TaxClass_Id == defaultStore.TaxClass_Id).ToList();
                decimal taxInvoiceServiceValue = 0;
                foreach (OrderProduct product in order.Products.Where(t => t.Taxable))
                {
                    long country_Id = order.DeliveryService_Id == 0 ? (customerOrder.Store == null || customerOrder.Store.AddressCard == null ? defaultStore.AddressCard.Country_Id : customerOrder.Store.AddressCard.Country_Id) : (product.TaxByShipping ? order.Address_Shipping.Country_Id : order.Address_Billing.Country_Id);
                    long state_Id = order.DeliveryService_Id == 0 ? (customerOrder.Store == null || customerOrder.Store.AddressCard == null ? defaultStore.AddressCard.State_Id : customerOrder.Store.AddressCard.State_Id.GetValueOrDefault(0)) : (product.TaxByShipping ? order.Address_Shipping.State_Id.GetValueOrDefault(0) : order.Address_Billing.State_Id.GetValueOrDefault(0));
                    string zip = order.DeliveryService_Id == 0 ? (customerOrder.Store == null || customerOrder.Store.AddressCard == null ? defaultStore.AddressCard.Zip : customerOrder.Store.AddressCard.Zip) : (product.TaxByShipping ? order.Address_Shipping.Zip : order.Address_Billing.Zip);
                    List<TaxRuleDetail> tr = taxRules.Where(t => t.TaxShipping == product.TaxByShipping && t.DestinationState_Id == state_Id && t.DestinationCountry_Id == country_Id).ToList();
                    TaxRuleDetail taxRule = tr.FirstOrDefault(t => (t.DestinationZip ?? string.Empty).ToLower().Trim() == zip.ToLower().Trim()) ?? tr.FirstOrDefault(t => string.IsNullOrEmpty(t.DestinationZip));
                    if (taxRule == null) continue;
                    decimal taxValue = product.AmountDue * taxRule.SalesTax * (decimal)0.01;
                    if (product.IsAddInInvoice) taxInvoiceServiceValue += taxValue;
                }
                if (tax == null)
                {
                    order.Services.Add(new OrderService { IdTitle = new IdTitle { Id = (long)StoreServices.Tax, Title = StoreServices.Tax.ToString() }, Value = taxInvoiceServiceValue.GetPrice(), IsAddInInvoice = true, Adding = true });
                }
                else
                {
                    tax.Value = taxInvoiceServiceValue.GetPrice();
                }
            }
            else
            {
                if (tax != null) order.Services.Remove(tax);
            }
            return View(order);

        }
        #endregion
    }
}
