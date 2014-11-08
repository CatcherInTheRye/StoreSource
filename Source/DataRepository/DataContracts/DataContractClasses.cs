using PCS.DataRepository.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace DataRepository.DataContracts
{
    //TODO: Tolik. Подумать как можно сделать преобразование одного типа в другой.
    //public static class DataContractClassWrapper
    //{
    //    public static object Wrap(object entityClass, Type toType)
    //    {
    //        Type result = null;
    //        Type entityClassType = entityClass.GetType();
    //        PropertyInfo[] propertyInfos = toType.GetProperties();
    //        foreach (PropertyInfo source in entityClassType.GetProperties())
    //        {
    //            PropertyInfo dest = propertyInfos.FirstOrDefault(p => p.Name.Equals(source.Name));
    //            if (dest != null)
    //        }
    //        return result;
    //    }
    //}

    public class IdTitle
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class IdTitleDescription : IdTitle
    {
        public string Description { get; set; }
    }

    #region User

    public class UserForm
    {
        public UserForm()
        {
            Id = -1;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                            ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First Name is Required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is Required")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string Salutation { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        public string Pwd { get; set; }

        [Required(ErrorMessage = "Confirmation Password is Required")]
        [Compare("Pwd", ErrorMessage = "Confirmation Password must match")]
        [DataType(DataType.Password)]
        public string ConfirmPwd { get; set; }

        [Required(ErrorMessage = "UserName is Required")]
        public string UserName { get; set; }

        public DateTime PwdChangedDate { get; set; }

        public int PwdChangedBy { get; set; }

        public bool Active { get; set; }

        public DateTime ApplyDate { get; set; }

        public string UserImageFile { get; set; }

        public string UserType { get; set; }

        public string PsUserId { get; set; }

        public string Phone { get; set; }

        public string Cell { get; set; }

        //added from another table

        public int UserRole { get; set; }

        //extensions

        public string FullName
        {
            get
            {
                return string.Format("{0}, {1}{2}", LastName, FirstName,
                    !string.IsNullOrWhiteSpace(MiddleName) ? string.Format(" {0}", MiddleName) : "");
            }
        }

        public string Speciality { get; set; }

        public string CaseLoad { get; set; }

        public string Location { get; set; }

        //for saving

        public string UserSupportRequests { get; set; }

        public string Districts { get; set; }

        public string Schools { get; set; }

        public string Organizations { get; set; }

        public UserActionForm UserActionForm { get; set; }
    }

    //TODO: not used
    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    //public sealed class MustMatchAttribute : ValidationAttribute
    //{
    //    private const string DefaultErrorMessage = "Must match {0}";
    //    private readonly object _typeId = new object();

    //    public MustMatchAttribute(string propertyToMatch)
    //        : base(DefaultErrorMessage)
    //    {
    //        PropertyToMatch = propertyToMatch;
    //    }

    //    public string PropertyToMatch { get; private set; }

    //    public override object TypeId
    //    {
    //        get
    //        {
    //            return _typeId;
    //        }
    //    }

    //    public override string FormatErrorMessage(string name)
    //    {
    //        return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString, PropertyToMatch);
    //    }

    //    public override bool IsValid(object value)
    //    {
    //        // we don't have enough information here to be able to validate against another field
    //        // we need the DataAnnotationsMustMatchValidator adapter to be registered
    //        throw new Exception("MustMatchAttribute requires the DataAnnotationsMustMatchValidator adapter to be registered"); // TODO – make this a typed exception :-)
    //    }
    //}

    public class FteForm
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Value { get; set; }
        //public string ValueS { get; set; }

        public string From { get; set; }
        //public string FromS { get; set; }
        public string To { get; set; }
        //public string ToS { get; set; }
    }

    public class RoleForm
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }

    public class UserActionForm
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool CanRequest { get; set; }
        public bool CanApprove { get; set; }
        public bool CanViewReports { get; set; }
    }

    #endregion User

    #region Support Request Type

    public class SupportRequestTypeForm
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
    }

    public class UserSupportRequestTypeForm : SupportRequestTypeForm
    {
        public int UserId { get; set; }
    }

    #endregion Support Request Type

    #region School and Organizations


    public class SchoolForm
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string SchoolName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string SourceId { get; set; }

        public string FullPrintAddress
        {
            get
            {
                return string.Format("{0}\n\r{1},{2}\n\r{3}", Address, City, Province, PostalCode);
            }
        }

        public string FullWebAddress
        {
            get
            {
                return string.Format("{0}{1}{2}{3}{4}{5}{6}", Address ?? "",
                                     ((Address ?? "").Length == 0 || (City ?? "").Length == 0 ? "" : "<br/>"),
                                     City ?? "", ((City ?? "").Length == 0 || (Province ?? "").Length == 0 ? "" : ","),
                                     Province ?? "",
                                     ((Province ?? "").Length == 0 || (PostalCode ?? "").Length == 0 ? "" : "<br/>"),
                                     PostalCode ?? "");
            }
        }
    }


    public class SchoolFormShort : IdTitle
    {
        public int OrganizationId { get; set; }
    }


    public class UserSchoolForm : SchoolFormShort
    {
        public int UserId { get; set; }
    }

    public class OrganizationForm
    {
        public OrganizationForm()
        {
            Id = -1;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "District Name is Required")]
        public new string Title { get; set; }

        public bool Active { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Province { get; set; }

        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Phone is Required")]
        public string ContactPhone { get; set; }

        public string ContactFax { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                            ErrorMessage = "Email is not valid")]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "Contact Name is Required")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "System Username is Required")]
        public string SystemUserName { get; set; }

        public string FullName { get
        {
            string result = string.Empty;
            if (!string.IsNullOrWhiteSpace(ContactName))
            {
                result = ContactName;
                if (!string.IsNullOrWhiteSpace(SystemUserName))
                {
                    result = string.Format("{0}/{1}", result, SystemUserName);
                }
            }
            else
            {
                result = SystemUserName;
            }
            return result;
        } }

        [Required(ErrorMessage = "System Password is Required")]
        [DataType(DataType.Password)]
        public string SystemPassword { get; set; }

        [Required(ErrorMessage = "System Confirm Password is Required")]
        [Compare("SystemPassword", ErrorMessage = "Confirmation Password must match")]
        [DataType(DataType.Password)]
        public string SystemConfirmPassword { get; set; }
    }

    public class UserOrganizationForm : OrganizationForm
    {
        public int UserId { get; set; }
    }


    public class OrganizationSettingsForm
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public bool Sync { get; set; }
        public string PowerSchoolUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }


    }

    public class SchoolStudentsSearch : StudentSearchForm
    {
        public string Gender { get; set; }
        public string Services { get; set; }
    }

    public class StudentReport
    {
        public int Id { get; set; }
        public string Background { get; set; }
        public string Observations { get; set; }
        public string Test { get; set; }
        public string Goals { get; set; }
        public string Progress { get; set; }
        public string Recomendations { get; set; }
        public string Summary { get; set; }
        public string Status { get; set; }
        public string ConsultNames { get; set; }
    }

    public class StudentFullInfo : StudentReport
    {
        public StudentForm MainInfo { get; set; }
        public List<UploadedFile> Files { get; set; }
        public List<IdTitleDescription> Services { get; set; }
    }
    #endregion School and Organizations

    public class SettingsForm
    {
        public int Id { get; set; }

        public int Month { get; set; }

        public string MonthS { get { return CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[Month - 1]; } }

        public int Year { get; set; }

        public int Hours { get; set; }
    }

    #region District System Admin

    public class DistrictStaffs
    {
        public int Id { get; set; }
        public string Dictrict { get; set; }
        public string School { get; set; }
        public string Cell { get; set; }
        public string Salutation { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool CanRequest { get; set; }
        public bool CanApprove { get; set; }
        public bool CanViewReports { get; set; }
    }

    public class UploadedFile
    {
        public UploadedFile()
        {
            Id = -1;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Binary Content { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        public int Size { get { return Content == null ? 0 : Content.Length; } }
    }

    #endregion  District System Admin

    #region Emails
    public class NewRequestEmail
    {
        public List<string> Emails { get; set; }
        public string User { get; set; }
        public string School { get; set; }
    }
    #endregion

    #region Manager
    public class TimeRecordReportForm
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Specialist { get; set; }
        public string Student { get; set; }
        public string Code { get; set; }
        public int Time { get; set; }
        public string Note { get; set; }
    }

    public class CaseRecordReportForm
    {
        public int Id { get; set; }
        public string Specialist { get; set; }
        public string Type { get; set; }
        public string District { get; set; }
        public string School { get; set; }
        public string Student { get; set; }
        public DateTime Dob { get; set; }
        public string Grade { get; set; }
    }

    public class ManagerRequests : StudentSupportRequestSchortForm
    {
        public string School { get; set; }
        public string District { get; set; }
        public string Specialists { get; set; }
        public List<RequestTypeSpecialist> AssignedSpecialists { get; set; }
        public string Grade { get; set; }
        public string Gender { get; set; }
        public int StudentId { get; set; }
    }

    public class RequestTypeSpecialist
    {
        public IdTitle Type { get; set; }
        public IdTitle Specialist { get; set; }
    }
    #endregion
}
