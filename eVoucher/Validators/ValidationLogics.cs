using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eVoucher.Validators
{
    public class ValidationLogics : IValidationLogics
    {
        public bool ChecknullRequestParameter<T>(T validateModel)
        {
            var results = ValidateNull(validateModel);

            if (results.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private IList<ValidationResult> ValidateNull<T>(T reqmodel)
        {
            var results = new List<ValidationResult>();

            var context = new ValidationContext(reqmodel);

            Validator.TryValidateObject(
                reqmodel, context, results, true);

            return results;
        }
        public string HasCorrectLength<T>(T validateModel)
        {
            bool isValid = true;
            string result = null;
            var entityType = typeof(T);

            var properties = entityType.GetProperties()
                .Where(property => property.IsDefined(typeof(LengthValidation)))
                .ToList();

            foreach (var property in properties)
            {
                string parameterName = property.Name;
                var attribute = property.GetCustomAttribute<LengthValidation>();
                var propertyValue = property.GetValue(validateModel);
                isValid = attribute.CheckValid(propertyValue);

                if (!isValid)
                {
                    result += property.Name + " length validation failed. ";
                }
            }

            return result;
        }
    }
}
