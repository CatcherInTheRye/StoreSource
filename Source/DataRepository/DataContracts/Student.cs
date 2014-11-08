using System.Data.Linq;
using DataRepository.DataContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace PCS.DataRepository.DataContracts
{
    [DataContract]
    public class StudentMenuItem
    {
        [DataMember]
        public string text;
        [DataMember]
        public string href;
        [DataMember]
        public string title;
        [DataMember(EmitDefaultValue = false)]
        public string level;
    }

    [DataContract]
    public class StudentMenuFull
    {
        [DataMember]
        public List<StudentMenuItem> first;
        [DataMember]
        public List<StudentMenuItem> second;
    }

    /*[DataContract]
    public class StudentSearchForm
    {
        [DataMember]
        public string id;
        [DataMember]
        public string name;
        [DataMember]
        public string number;
        [DataMember]
        public string grade;
        [DataMember]
        public string school;
        [DataMember]
        public string district;
        [DataMember]
        public string dob;
    }*/

    public class RoleAreaForm
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Href { get; set; }
        public string Image { get; set; }
    }

    public class BackgroundForm
    {
        public string background { get; set; }
        public string ReferedBy { get; set; }
        public string Reason { get; set; }
    }

    public class IdValue
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class SummaryAndStatus
    {
        public string Summary { get; set; }
        public string Status { get; set; }
    }

    public class TimeRecordFrom
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Code { get; set; }
        public int Time { get; set; }
        public string Notes { get; set; }
    }

    public class DateRange
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
    }

    public class StudentSearchForm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Grade { get; set; }
        public string School { get; set; }
        public string District { get; set; }
        public DateTime Dob { get; set; }
    }

    public class UserShortInfo
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
    }

    public class UserShortInfoDistrict : UserShortInfo
    {
        public string District { get; set; }
        public string School { get; set; }

        public int Order
        {
            get
            {
                if (District != "" && School != "")
                    return 0;
                else if (District != "" && School == "")
                    return 1;
                else
                    return 2;
            }
        }

        public string DistrictSchool
        {
            get { return District + (School != "" ? string.Format(" - {0}", School) : ""); }
        }
    }

    public class TabMenuItem
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string SubItems { get; set; }
    }

    public class CurrentSchoolUsers
    {
        public List<UserShortInfo> schoolUsers { get; set; }
        public List<UserShortInfo> districtUser { get; set; }
    }

    public class StudentSupportRequestForm
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int RequestedBy { get; set; }
        public DateTime DateRequested { get; set; }
        public string Reason { get; set; }
        public string approvalNote { get; set; }
        public bool Approved { get; set; }
        public bool Denied { get; set; }
        public bool Submitted { get; set; }
        public int WhoApproved { get; set; }
        public DateTime DateApproved { get; set; }
        public string Users { get; set; }
        public string Types { get; set; }
        public UploadedFile SelectedFile { get; set; }
    }

    public class StudentSupportRequestSchortForm
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string RequestedBy { get; set; }
        public string Specialities { get; set; }
        public List<int> SpecialitiyIds { get; set; }
        public DateTime DateOfRequest { get; set; }
        public string Note { get; set; }
        public string Reason { get; set; }
        public UploadedFile file { get; set; }
    }

    //public class StudentForm
    //{
    //    public int Id { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string MiddleName { get; set; }
    //    public string School { get; set; }
    //    public string District { get; set; }
    //    public string Gender { get; set; }
    //    public DateTime Dob { get; set; }
    //    public string Grade { get; set; }
    //    public string Code { get; set; }
    //    public string SpecialPrograms { get; set; }
    //    public string HomePhone { get; set; }
    //    public string MailingAddress { get; set; }
    //    public string MailingCity { get; set; }
    //    public string MailingProvince { get; set; }
    //    public string MailingPostalCode { get; set; }
    //    public string Address { get; set; }
    //    public string City { get; set; }
    //    public string Province { get; set; }
    //    public string PostalCode { get; set; }
    //    public string MotherName { get; set; }
    //    public string MotherPhone { get; set; }
    //    public string MotherEmail { get; set; }
    //    public string FatherName { get; set; }
    //    public string FatherPhone { get; set; }
    //    public string FatherEmail { get; set; }
    //    public string GuardianName { get; set; }
    //    public string GuardianPhone { get; set; }
    //    public string GuardianEmail { get; set; }
    //    public string SourceId { get; set; }
    //    public string StudentNumber { get; set; }
    //}

    public class StudentShortForm
    {
        public int Id { get; set; }
        public string School { get; set; }
        public string StudentName { get; set; }
        public string Gender { get; set; }
        public DateTime Dob { get; set; }
        public string Grade { get; set; }
    }

    public class StudentForm
    {
        public int Id { get; set; }
        //not null
        public int SchoolId { get; set; }
        //not null
        public string StudentNumber { get; set; }
        //not null
        public string FirstName { get; set; }
        //not null
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        //not null
        public char Gender { get; set; }

        public string GenderS { get { return Gender.Equals('m') ? "Male" : "Female"; } }

        //not null
        public DateTime Dob { get; set; }
        //not null
        public string Grade { get; set; }
        public string Code { get; set; }
        public string SpecialPrograms { get; set; }
        public string HomePhone { get; set; }
        
        public string MailingAddress { get; set; }
        public string MailingCity { get; set; }
        public string MailingProvince { get; set; }
        public string MailingPostalCode { get; set; }
        
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }

        public string MotherName { get; set; }
        public string MotherPhone { get; set; }
        public string MotherEmail { get; set; }

        public string FatherName { get; set; }
        public string FatherPhone { get; set; }
        public string FatherEmail { get; set; }

        public string GuardianName { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianEmail { get; set; }

        public string SourceId { get; set; }
        //not null
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public bool Active { get; set; }

        public string School { get; set; }
        public string District { get; set; }
    }

    //without content
    public class StudentSupportReportFileShort
    {
        public int Id { get; set; }
        public int StudentSupportReportId { get; set; }
        public string FileName { get; set; }
        public DateTime DateUploaded { get; set; }
        public string DateUploadedS { get { return DateUploaded.ToShortDateString(); } }
        public int UploadedBy { get; set; }
        public string UploadedByName { get; set; }
        public string Description { get; set; }
    }

    public class StudentSupportReportFile : StudentSupportReportFileShort
    {
        public Binary FileContent { get; set; }
    }
}
