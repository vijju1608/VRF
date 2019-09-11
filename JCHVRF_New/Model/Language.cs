using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Common.Helpers;

namespace JCHVRF_New.Model
{
    public class Language:ModelBase
    {
        private string _languagename;
        /// <summary>
        /// Gets or sets the Language
        /// </summary>
        public string LanguageName
        {
            get { 
                
                return _languagename;
            }
            set { 
                this.SetValue(ref _languagename, value); }
        }
        private string _languagecode;
        public string LanguageCode
        {
            get
            {

                return _languagecode;
            }
            set
            {
                this.SetValue(ref _languagecode, value);
            }
        }
        private bool _isSelected =true;
        /// <summary>
        /// Gets or sets the IsSelected
        /// </summary>
        public bool IsSelected
        {
            get { if (_isSelected)
                {
                    JCHVRF.Model.SystemSetting.SelectedLanguage = LanguageName;
                    JCHVRF.Model.SystemSetting.SelectedLanguageCode = LanguageCode;
                }
                return this._isSelected;
            }
            set {
                this.SetValue(ref _isSelected, value); }
        }

        private string _selectedLanguage;
        /// <summary>
        /// Gets or sets the SelectedLanguage
        /// </summary>
        public string SelectedLanguage
        {
            get { return this._selectedLanguage; }
            set { this.SetValue(ref _selectedLanguage, value); }
        }
        
    }
}
