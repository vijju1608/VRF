/****************************** File Header ******************************\
File Name:	CustomValidations.cs
Created By:	HCLTECH\HimanshuGupta
Date Created:	2/4/2019
Description:	
\*************************************************************************/

namespace JCHVRF_New.Common.Helpers
{
    using System.ComponentModel.DataAnnotations;


    /// <summary>
    /// With this way we can have different messages for same kind of check :D
    /// </summary>
    public class IntLessThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public IntLessThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (int)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                return ValidationResult.Success;

            var comparisonValue = (int)property.GetValue(validationContext.ObjectInstance);

            if (currentValue > comparisonValue)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }



    /// <summary>
    /// With this way we have single message for single method
    /// </summary>
    public class CustomValidations
    {
       
        #region Methods

        /// <summary>
        /// The IsLength10, This is just a sample method, more methods will be added later.
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="ValidationResult"/></returns>
        public static ValidationResult IsLength10(string value)
        {

            if (value?.Length == 10)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Length is not 10.");
            }
        }

       

        #endregion
    }
}
