/****************************** File Header ******************************\
File Name:	ModelBase.cs
Date Created:	2/4/2019
Description:	Base Class for All models in the application. 
               Supports Property Change Notifications and Validation Handling.
\*************************************************************************/

namespace JCHVRF_New.Common.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class ModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        [NonSerialized]
        private readonly Dictionary<string, Func<ModelBase, object>> propertyGetters;
        [NonSerialized]
        private readonly Dictionary<string, ValidationAttribute[]> validators;
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public ModelBase()
        {
            this.validators = this.GetType()
                .GetProperties()
                .Where(p => this.GetValidations(p).Length != 0)
                .ToDictionary(p => p.Name, p => this.GetValidations(p));

            this.propertyGetters = this.GetType()
                .GetProperties()
                .Where(p => this.GetValidations(p).Length != 0)
                .ToDictionary(p => p.Name, p => this.GetValueGetter(p));
        }

        private ValidationAttribute[] GetValidations(PropertyInfo property)
        {
            return (ValidationAttribute[])property.GetCustomAttributes(typeof(ValidationAttribute), true);
        }

        private Func<ModelBase, object> GetValueGetter(PropertyInfo property)
        {
            return new Func<ModelBase, object>(viewmodel => property.GetValue(viewmodel, null));
        }

        public void SetValue<TField>(ref TField field, TField newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TField>.Default.Equals(field, newValue)) return;
            field = newValue;
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Gets the number of properties which have a validation attribute and are currently valid
        /// </summary>
        public int ValidPropertiesCount
        {
            get
            {
                return this.validators.Where(validator => validator.Value.All(attribute => attribute.IsValid(this.propertyGetters[validator.Key](this)))).Count();
            }
        }

        /// <summary>
        /// Gets the number of properties which have a validation attribute
        /// </summary>
        public int TotalPropertiesWithValidationCount
        {
            get
            {
                return this.validators.Count();
            }
        }

        /// <summary>
        /// Returns true if All validations are passed.
        /// </summary>
        public bool IsValid { get { return this.ValidPropertiesCount == this.TotalPropertiesWithValidationCount; } }



        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (this.propertyGetters!=null && this.propertyGetters.ContainsKey(propertyName))
            {
                RaisePropertyChanged("ValidPropertiesCount");
                RaisePropertyChanged("TotalPropertiesWithValidationCount");
                RaisePropertyChanged("IsValid");
            }
        }

        /// <summary>
        /// Property To Check whether there are validation errors.
        /// Will return true only when called from Validate/ Validate All methods.
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return _explicitlyCalled && !IsValid;
            }
        }

        [field: NonSerialized]
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (this.propertyGetters.ContainsKey(propertyName))
            {
                var propertyValue = this.propertyGetters[propertyName](this);
                var errorMessages = this.validators[propertyName]
                    .Where(v => !v.IsValid(propertyValue))
                    .Select(v => v.ErrorMessage).ToArray();

                return errorMessages;
            }
            return null;
        }

        bool _explicitlyCalled;

        public bool ValidateAll()
        {
            _explicitlyCalled = true;
            try
            {
                if (HasErrors)
                {
                    foreach (var validator in validators)
                    {
                        if (validator.Value.Any(attribute => !attribute.IsValid(this.propertyGetters[validator.Key](this))))
                        {
                            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(validator.Key));
                        }
                    }
                }
                else
                {
                    return true;
                }
                return false;
            }
            finally
            {
                _explicitlyCalled = false;
            }
        }

        public bool Validate([CallerMemberName] string propertyName = null)
        {
            if (!validators.ContainsKey(propertyName)) return true;

            _explicitlyCalled = true;
            try
            {
                if (validators[propertyName].Any(attribute => !attribute.IsValid(this.propertyGetters[propertyName](this))))
                {
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                    return false;
                }
                return true;
            }
            finally
            {
                _explicitlyCalled = false;
            }
        }
    }
}
