using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace StoreLib.Modules.ValIdation
{
    public class ValIdationRule
    {
        public ValIdationRule(ValIdationType type, string fieldName, string errorMessage, IFieldValIdationAttribute iFieldValIdation, string fieldTitle)
        {
            Type = type;
            FieldName = fieldName;
            ErrorMessage = errorMessage;
            IFieldValIdation = iFieldValIdation;
            FieldTitle = fieldTitle;
        }
        public IFieldValIdationAttribute IFieldValIdation;
        public ValIdationType Type = ValIdationType.Required;
        public string FieldName = string.Empty;
        public string ErrorMessage = string.Empty;
        public string FieldTitle = string.Empty;
    }


    public class ValIdationCheck
    {
        #region GetErrors
        public static void CheckErrors(object model, System.Web.Mvc.ModelStateDictionary modelState, bool valIdateNotUpdateedFields, ValIdationType? valIdationType = null, string modelKeyPrefix = "")
        {
            Dictionary<string, List<ValIdationRule>> errorRules = ValIdationCheck.GetValIdationRules(model.GetType(), valIdationType);

            foreach (KeyValuePair<string, List<ValIdationRule>> item in errorRules)
            {
                object obj = model.GetType().InvokeMember(item.Key, BindingFlags.GetProperty, null, model, null);

                foreach (ValIdationRule rule in item.Value)
                {
                    if (!rule.IFieldValIdation.ValIdate(obj))
                    {
                        string modelKey = modelKeyPrefix + rule.FieldName;
                        if (modelState.ContainsKey(modelKey) || valIdateNotUpdateedFields) modelState.AddModelError(modelKey, rule.IFieldValIdation.GetErrorMessage(rule.FieldTitle));
                    }
                }
            }
        }

        public static void CheckErrors(object model, System.Web.Mvc.ModelStateDictionary modelState, bool valIdateNotUpdateedFields)
        {
            CheckErrors(model, modelState, valIdateNotUpdateedFields, null, "");
        }

        public static void CheckErrors(object model, System.Web.Mvc.ModelStateDictionary modelState, bool valIdateNotUpdateedFields, string modelKeyPrefix = "")
        {
            CheckErrors(model, modelState, valIdateNotUpdateedFields, null, modelKeyPrefix);
        }

        public static void CheckErrors(object model, System.Web.Mvc.ModelStateDictionary modelState)
        {
            CheckErrors(model, modelState, false, null);
        }
        #endregion

        #region GetValIdationRules

        public static Dictionary<string, List<ValIdationRule>> GetValIdationRules(Type modelType, ValIdationType? valIdationType = null)
        {
            Dictionary<string, List<ValIdationRule>> errorRules = new Dictionary<string, List<ValIdationRule>>();

            PropertyInfo[] infos = modelType.GetProperties();
            if (valIdationType == null) CheckProperties(infos, errorRules);
            else
                CheckProperties(infos, errorRules, valIdationType.Value);

            Type[] interfaces = modelType.GetInterfaces();
            foreach (Type type in interfaces)
            {
                if (!type.FullName.StartsWith("System."))
                {
                    PropertyInfo[] interfaceInfos = type.GetProperties();
                    CheckProperties(interfaceInfos, errorRules);
                }
            }

            return errorRules;
        }

        static void CheckProperties(PropertyInfo[] infos, Dictionary<string, List<ValIdationRule>> errorRules)
        {
            foreach (PropertyInfo info in infos)
            {
                string title = string.Empty;
                object[] attrs_title = info.GetCustomAttributes(typeof(FieldTitleAttribute), true);

                title = (attrs_title != null && attrs_title.Length > 0) ? ((FieldTitleAttribute)attrs_title[0]).FieldTitle : info.Name;

                object[] attrs = info.GetCustomAttributes(typeof(FieldValIdationAttribute), true);
                foreach (IFieldValIdationAttribute attr in attrs)
                {
                    if (attr != null)
                    {
                        ValIdationRule rule = rule = new ValIdationRule(attr.Type, info.Name, attr.GetErrorMessage(title), attr, title);

                        if (!errorRules.ContainsKey(rule.FieldName))
                        {
                            errorRules[rule.FieldName] = new List<ValIdationRule>(); ;
                        }
                        errorRules[rule.FieldName].Add(rule);
                    }
                }
            }
        }

        static void CheckProperties(PropertyInfo[] infos, Dictionary<string, List<ValIdationRule>> errorRules, ValIdationType checkType)
        {
            foreach (PropertyInfo info in infos)
            {
                string title = string.Empty;
                object[] attrs_title = info.GetCustomAttributes(typeof(FieldTitleAttribute), true);

                title = (attrs_title != null && attrs_title.Length > 0) ? ((FieldTitleAttribute)attrs_title[0]).FieldTitle : info.Name;

                object[] attrs = info.GetCustomAttributes(typeof(FieldValIdationAttribute), true);
                List<IFieldValIdationAttribute> attributes = ((IFieldValIdationAttribute[])attrs).ToList();
                if (attributes.FirstOrDefault(a => a.Type == checkType) == null) continue;
                foreach (IFieldValIdationAttribute attr in attrs)
                {
                    if (attr != null)
                    {
                        ValIdationRule rule = new ValIdationRule(attr.Type, info.Name, attr.GetErrorMessage(title), attr, title);

                        if (!errorRules.ContainsKey(rule.FieldName))
                        {
                            errorRules[rule.FieldName] = new List<ValIdationRule>(); ;
                        }
                        errorRules[rule.FieldName].Add(rule);
                    }
                }
            }
        }

        #endregion

        #region ValIdation functions
        public static bool IsEmail(string s)
        {
            if (String.IsNullOrEmpty(s)) return true;
            Regex regex = new Regex(RegularExpressions.Email);

            if (regex != null && !regex.IsMatch(s))
                return false;

            return true;
        }

        public static bool IsEmpty(object s)
        {
            if (s != null && s.ToString().Trim() != string.Empty)
                return false;

            return true;
        }

        public static bool IsDate(string s)
        {
            DateTime date = DateTime.MinValue;
            if (DateTime.TryParse(s, out date))
            {
                return true;
            }

            return false;
        }

        public static bool CheckMinValue(string s, int MinLength)
        {
            if (s != null && s.ToString().Length < MinLength && s.ToString().Length > 0)
                return false;

            return true;
        }

        public static bool CheckMaxValue(string s, int MaxLength)
        {
            if (s != null && s.ToString().Length > MaxLength && s.ToString().Length > 0)
                return false;

            return true;
        }

        public static bool CheckYear(object s)
        {
            if (s == null)
                return true;

            int year = 0;
            if (int.TryParse(Convert.ToString(s), out year) && year >= 1900 && year <= 2078)
                return true;

            return false;
        }

        public static bool CheckDateTime(object s)
        {
            if (s == null) return true;
            DateTime datetime;
            return (DateTime.TryParse(s.ToString(), out datetime));
        }

        public static bool CheckAlphanumeric(string s)
        {
            Regex regex = new Regex(RegularExpressions.AlphaNumeric);
            return regex.IsMatch(s);
        }

        public static bool CheckAlphanumericDot(string s)
        {
            Regex regex = new Regex(RegularExpressions.AlphaNumericDot);
            return regex.IsMatch(s);
        }

        public static bool CheckAlpha(string s)
        {
            Regex regex = new Regex(RegularExpressions.Alpha);

            if (regex != null && !regex.IsMatch(s))
                return false;

            return true;
        }

        public static bool CheckFieldNumeric(string s)
        {
            Regex regex = new Regex(RegularExpressions.Numbers);

            if (regex != null && !regex.IsMatch(s))
                return false;

            return true;
        }

        public static bool CheckFieldDecimal(string s)
        {
            Regex regex = new Regex(RegularExpressions.Decimal);
            if (regex != null && !regex.IsMatch(s))
                return false;
            return true;
        }

        public static bool CheckMinValue(string toCheck, decimal minValue)
        {
            decimal check = Convert.ToDecimal(toCheck);
            bool result = decimal.TryParse(toCheck, out check);
            if (!result)
                return result;
            result = (check >= minValue);
            return result;
        }

        public static bool CheckFieldAmericanPhone(string p)
        {
            Regex regex = new Regex(RegularExpressions.AmericanPhone);

            if (regex != null && !regex.IsMatch(p))
                return false;

            return true;
        }

        public static bool CheckFieldPhone(string p)
        {
            Regex regex = new Regex(RegularExpressions.Phone);
            return regex.IsMatch(p);
        }

        public static bool IsSpaced(string p)
        {
            return !p.Contains(" ");
        }

        public static bool CheckUserName(string s)
        {
            Regex regex = new Regex(RegularExpressions.UserName);
            return regex.IsMatch(s);
        }
        #endregion
    }
}
