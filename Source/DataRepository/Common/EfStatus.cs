using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Entity.Core;
using System.Linq;

namespace DataRepository.Mappers
{
    public class EfStatus
    {
        private List<ValidationResult> errors;

        /// <summary>
        /// If there are no errors then it is valid
        /// </summary>
        public bool IsValid
        {
            get { return errors == null; }
        }

        public List<ValidationResult> Errors
        {
            get { return errors ?? new List<ValidationResult>(); }
        }

        /// <summary>
        /// This converts the Entity framework errors into Validation errors
        /// </summary>
        public EfStatus SetErrors(IEnumerable<DbEntityValidationResult> validationResults)
        {
            errors = validationResults.SelectMany(x => x.ValidationErrors.Select(y => new ValidationResult(y.ErrorMessage, new[] { y.PropertyName }))).ToList();
            return this;
        }

        public EfStatus SetErrors(IEnumerable<ValidationResult> validationResults)
        {
            errors = validationResults.ToList();
            return this;
        }

        public IEnumerable<ValidationResult> TryDecodeDbUpdateException(DbUpdateException ex)
        {
            if (!(ex.InnerException is System.Data.Entity.Core.UpdateException) || !(ex.InnerException.InnerException is System.Data.SqlClient.SqlException))
            {
                return null;
            }
            var sqlException = (System.Data.SqlClient.SqlException)ex.InnerException.InnerException;
            var result = new List<ValidationResult>();
            for (int i = 0; i < sqlException.Errors.Count; i++)
            {
                var errorNum = sqlException.Errors[i].Number;
                string errorText;
                if (SqlErrorTextDict.TryGetValue(errorNum, out errorText))
                {
                    result.Add(new ValidationResult(errorText));
                }
            }
            return result.Any() ? result : null;
        }

        private static readonly Dictionary<int, string> SqlErrorTextDict = new Dictionary<int, string>
                                                                                {
                                                                                    {
                                                                                        547,
                                                                                        "This operation failed because another data entry uses this entry."
                                                                                    },
                                                                                    {
                                                                                        2601,
                                                                                        "One of the properties is marked as Unique index and there is already an entry with that value."
                                                                                    }
                                                                                };
    }
}
