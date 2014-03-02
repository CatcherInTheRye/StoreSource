using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Modules.ValIdation
{
    public enum ValIdationType
    {
        Required = 1,
        Email = 2,
        MinLength = 3,
        MaxLength = 4,
        Year = 5,
        Alpha = 6,
        Alphanumeric = 7,
        Numeric = 8,
        AmericanPhone = 9,
        FieldNonSpaced = 10,
        DateTime = 11,
        Decimal = 12,
        Address = 13,
        MinValue = 14
    }

    #region IFieldValIdationAttribute
    public interface IFieldValIdationAttribute
    {
        bool ValIdate(object value);
        string GetErrorMessage(string title);
        string GetAdditionalParams();
        ValIdationType Type { get; }
    }
    #endregion

    #region FieldTitleAttribute
    public class FieldTitleAttribute : Attribute
    {
        protected string fieldTitleStr = string.Empty;

        public string FieldTitle
        {
            get { return fieldTitleStr; }
        }

        public FieldTitleAttribute(string title)
        {
            fieldTitleStr = title;
        }

    }
    #endregion

    #region FieldValIdationAttribute
    public class FieldValIdationAttribute : Attribute, IFieldValIdationAttribute
    {
        public ValIdationType type;

        public ValIdationType Type
        {
            get { return type; }
        }

        virtual public bool ValIdate(object value)
        {
            return true;
        }

        virtual public string GetErrorMessage(string title)
        {
            return string.Empty;
        }

        virtual public string GetAdditionalParams()
        {
            return string.Empty;
        }

    }
    #endregion

    #region FieldRequiredAttribute
    public class FieldRequiredAttribute : FieldValIdationAttribute
    {
        public FieldRequiredAttribute()
        {
            type = ValIdationType.Required;
        }

        public overrIde bool ValIdate(object value)
        {
            return !ValIdationCheck.IsEmpty(value);
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetRequired(title);
        }
    }
    #endregion

    #region FieldNonSpacedAttribute
    public class FieldNonSpacedAttribute : FieldValIdationAttribute
    {
        public FieldNonSpacedAttribute()
        {
            type = ValIdationType.FieldNonSpaced;
        }

        public overrIde bool ValIdate(object value)
        {
            return ValIdationCheck.IsSpaced(value == null ? string.Empty : value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetFieldNonSpaced(title);
        }
    }
    #endregion

    #region FieldCheckEmailAttribute
    public class FieldCheckEmailAttribute : FieldValIdationAttribute
    {
        public FieldCheckEmailAttribute()
        {
            type = ValIdationType.Email;
        }

        public overrIde bool ValIdate(object value)
        {
            return ValIdationCheck.IsEmail(value == null ? string.Empty : value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetInvalIdEmail(title);
        }
    }
    #endregion

    #region FieldCheckMinLengthAttribute
    public class FieldCheckMinLengthAttribute : FieldValIdationAttribute
    {
        int MinLength = 0;

        public FieldCheckMinLengthAttribute(int minLength)
        {
            type = ValIdationType.MinLength;
            MinLength = minLength;
        }

        public overrIde bool ValIdate(object value)
        {
            return ValIdationCheck.CheckMinValue(value == null ? string.Empty : value.ToString(), MinLength);
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetMinLength(title, MinLength);
        }

    }
    #endregion

    #region FieldCheckMaxLengthAttribute
    public class FieldCheckMaxLengthAttribute : FieldValIdationAttribute
    {
        int MaxLength = 0;

        public FieldCheckMaxLengthAttribute(int maxLength)
        {
            type = ValIdationType.MaxLength;
            MaxLength = maxLength;
        }

        public overrIde bool ValIdate(object value)
        {
            return ValIdationCheck.CheckMaxValue(value == null ? string.Empty : value.ToString(), MaxLength);
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetMaxLength(title, MaxLength);
        }

        public overrIde string GetAdditionalParams()
        {
            return "," + MaxLength.ToString();
        }
    }
    #endregion

    #region FieldCheckYearAttribute
    public class FieldCheckYearAttribute : FieldValIdationAttribute
    {
        public FieldCheckYearAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            return ValIdationCheck.CheckYear(value);
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetYearWrong(title);
        }
    }
    #endregion

    #region FieldCheckDateTimeAttribute
    public class FieldCheckDateTimeAttribute : FieldValIdationAttribute
    {
        public FieldCheckDateTimeAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            return ValIdationCheck.CheckDateTime(value);
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetDateTimeWrong(title);
        }
    }
    #endregion

    #region FieldCheckAlpha
    public class FieldCheckAlphaAttribute : FieldValIdationAttribute
    {
        public FieldCheckAlphaAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            return (value == null) || ValIdationCheck.CheckAlpha(value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetAlphaWrong(title);
        }
    }
    #endregion

    #region FieldCheckAlphanumeric
    public class FieldCheckAlphanumericAttribute : FieldValIdationAttribute
    {
        public FieldCheckAlphanumericAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            return (value == null) || ValIdationCheck.CheckAlphanumeric(value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetAlphanumericWrong(title);
        }
    }
    #endregion

    #region FieldCheckAlphanumericDot
    public class FieldCheckAlphanumericDotAttribute : FieldValIdationAttribute
    {
        public FieldCheckAlphanumericDotAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            return (value == null) || ValIdationCheck.CheckAlphanumericDot(value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetAlphanumericDotWrong(title);
        }
    }
    #endregion

    #region FieldCheckNumeric
    public class FieldCheckNumericAttribute : FieldValIdationAttribute
    {
        public FieldCheckNumericAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            return (value == null) || ValIdationCheck.CheckFieldNumeric(value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetFieldNumericWrong(title);
        }
    }
    #endregion

    #region FieldCheckDecimal
    public class FieldCheckDecimalAttribute : FieldValIdationAttribute
    {
        public FieldCheckDecimalAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            return (value == null) ? true : ValIdationCheck.CheckFieldDecimal(value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetFieldDecimalWrong(title);
        }
    }
    #endregion

    #region FieldCheckMinValue
    public class FieldCheckMinValueAttribute : FieldValIdationAttribute
    {
        private double MinValue = 0;
        public FieldCheckMinValueAttribute(double minValue)
        {
            type = ValIdationType.MinValue;
            MinValue = minValue;
        }

        public overrIde bool ValIdate(object value)
        {
            return (value == null) ? true : (ValIdationCheck.CheckFieldDecimal(value.ToString()) && ValIdationCheck.CheckMinValue(value.ToString(), (decimal)MinValue));
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetFieldMinValueWrong(title, MinValue.ToString());
        }

        public overrIde string GetAdditionalParams()
        {
            return "," + MinValue.ToString();
        }
    }
    #endregion

    #region FieldCheckAmericanPhone
    public class FieldCheckAmericanPhoneAttribute : FieldValIdationAttribute
    {
        public FieldCheckAmericanPhoneAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            if (value == null || Convert.ToString(value).Length < 1) return true;

            return ValIdationCheck.CheckFieldAmericanPhone(value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetFieldAmericanPhoneWrong(title);
        }
    }
    #endregion

    #region FieldCheckPhone
    public class FieldCheckPhoneAttribute : FieldValIdationAttribute
    {
        public FieldCheckPhoneAttribute()
        {

        }

        public overrIde bool ValIdate(object value)
        {
            if (value == null || Convert.ToString(value).Length < 1) return true;

            return ValIdationCheck.CheckFieldPhone(value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetFieldPhoneWrong(title);
        }
    }
    #endregion

    #region FieldCheckUserNameAttribute
    public class FieldCheckUserNameAttribute : FieldValIdationAttribute
    {
        public overrIde bool ValIdate(object value)
        {
            return ValIdationCheck.CheckUserName(value == null ? string.Empty : value.ToString());
        }

        public overrIde string GetErrorMessage(string title)
        {
            return ErrorMessages.GetInvalId(title);
        }
    }
    #endregion

    #region FieldOnlyAddress
    public class FieldCheckOnlyAddress : FieldValIdationAttribute
    {
        public FieldCheckOnlyAddress()
        {
            type = ValIdationType.Address;
        }
    }
    #endregion
}
