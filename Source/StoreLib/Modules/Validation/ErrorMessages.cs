using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Modules.ValIdation
{
    public static class ErrorMessages
    {
        public const string Required = @"Field '{0}' is required";
        public const string InvalId = @"Field '{0}' is invalId";
        public const string InvalIdEmail = @"Field '{0}' is invalId.";
        public const string MinLength = @"Field '{0}' must be at least {1} character(s)";
        public const string MaxLength = @"Field '{0}' must be maximum {1} character(s)";
        public const string MinMaxLength = @"The {0} must be at least {2} characters long, and not longer than {1}.";
        public const string YearWrong = @"Year must be 1900 - 2078.";
        public const string AlphaWrong = @"Field '{0}' should contain letters only";
        public const string NumericWrong = @"Field '{0}' should contain digits only";
        public const string DecimalWrong = @"Field '{0}' should contain decimal value only";
        public const string MinValueWrong = @"Field '{0}' can't be less than {1}";
        public const string AlphaNumericWrong = @"Field '{0}' should contain alphanumeric symbols only";
        public const string AlphaNumericDotWrong = @"Field '{0}' should cannot contain special symbols";
        public const string DateTimeWrong = @"Field '{0}' should contain date and/or time";
        public const string FieldNonSpacedWrong = @"Field '{0}' cannot contain spaces";
        public const string AmericanPhoneWrong = @"Field '{0}' should be like: XXX-XXX-XXXX or XXX-XXX-XXXX EXTXXXX";
        public const string PhoneWrong = @"Field '{0}' should contain digits and '-' only";


        public static string GetRequired(string field) { return string.Format(Required, field); }
        public static string GetInvalId(string field) { return string.Format(InvalId, field); }
        public static string GetInvalIdEmail(string field) { return string.Format(InvalIdEmail, field); }
        public static string GetMinLength(string field, int length) { return string.Format(MinLength, field, length); }
        public static string GetMaxLength(string field, int length) { return string.Format(MaxLength, field, length); }
        public static string GetYearWrong(string field) { return string.Format(YearWrong, field); }
        public static string GetDateTimeWrong(string field) { return string.Format(DateTimeWrong, field); }
        public static string GetAlphanumericWrong(string title) { return string.Format(AlphaNumericWrong, title); }
        public static string GetAlphanumericDotWrong(string title) { return string.Format(AlphaNumericDotWrong, title); }
        public static string GetAlphaWrong(string title) { return string.Format(AlphaWrong, title); }
        public static string GetFieldNumericWrong(string title) { return string.Format(NumericWrong, title); }
        public static string GetFieldDecimalWrong(string title) { return string.Format(DecimalWrong, title); }
        public static string GetFieldMinValueWrong(string title, string value) { return string.Format(MinValueWrong, title, value); }
        public static string GetFieldAmericanPhoneWrong(string title) { return string.Format(AmericanPhoneWrong, title); }
        public static string GetFieldPhoneWrong(string title) { return string.Format(PhoneWrong, title); }
        public static string GetFieldNonSpaced(string title) { return string.Format(FieldNonSpacedWrong, title); }
    }
}
