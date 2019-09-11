using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using JCBase.UI;
using JCHVRF.Model;

namespace JCHVRF_New.LanguageData
{
    public class LanguageViewModel : ViewModelBase
    {
        private static LanguageViewModel _current;

        private UILanguageDefn _languageDefn;
        private LanguageViewModel()
        {
            _languageDefn = null;
        }
        /// <summary>
        /// Gets the current instance of this singleton.
        /// </summary>
        /// <value>The current instance.</value>
        public static LanguageViewModel Current
        {
            get
            {
                if (_current == null)
                    _current = new LanguageViewModel();

                return _current;
            }
        }
        /// <summary>
        /// Gets or sets the language definition used by the entire interface.
        /// </summary>
        /// <value>The language definition.</value>
        public UILanguageDefn LanguageDefn
        {
            get { return _languageDefn; }
            set
            {
                if (_languageDefn != value)
                {
                    
                    _languageDefn = value;
                    OnPropertyChanged("LanguageDefn");
                    OnPropertyChanged("HeadingFontSize");                   
                }
            }
        }      
        public bool IsRightToLeft
        {
            get
            {
                if (_languageDefn != null)
                    return _languageDefn.IsRightToLeft;

                return false;
            }
        }

        private UILanguageDefn _languageMapping;
        
        public UILanguageDefn CurrentLanguage
        {
            get { return _languageMapping; }
        }


        private XmlDocument _languageDataDoc;

        public XmlDocument LanguageDataDoc
        {
            get { if(_languageDataDoc==null)
                    _languageDataDoc = new XmlDocument();
                return _languageDataDoc; }
            set { _languageDataDoc = value; }
        }


        public void UpdateLanguageData()
        {
            try
            {
                string files = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').ToString();
                string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                string navigateToFolder = "..\\..\\LanguageData\\Lang" + SystemSetting.UserSetting.defaultSetting.LanguageCode + ".xml";
                string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                //XmlDocument languageData = new XmlDocument();
                LanguageDataDoc = new XmlDocument();
                LanguageDataDoc.Load(sourceDir);
                _languageMapping = new UILanguageDefn();
                _languageMapping.LoadLanguageData(LanguageDataDoc.DocumentElement);
                LanguageViewModel.Current.LanguageDefn = CurrentLanguage;
                setLanguageforReport(SystemSetting.UserSetting.defaultSetting.LanguageCode);
            }
            catch(Exception ex)
            {

            }
            
        }
        public void setLanguageforReport(string languageCode)
        {
            switch (languageCode)
            {
                case "DE":
                    LangType.CurrentLanguage = LangType.GERMANY;
                    break;
                case "EN":
                    LangType.CurrentLanguage = LangType.ENGLISH;
                    break;
                case "FR":
                    LangType.CurrentLanguage = LangType.FRENCH;
                    break;
                case "IT":
                    LangType.CurrentLanguage = LangType.ITALIAN;
                    break;
                case "SP":
                    LangType.CurrentLanguage = LangType.SPANISH;
                    break;
                case "TK":
                    LangType.CurrentLanguage = LangType.TURKISH;
                    break;
                case "ZH":
                    LangType.CurrentLanguage = LangType.CHINESE_SIMPLE;
                    break;
                case "ZHT":
                    LangType.CurrentLanguage = LangType.CHINESE_TRADITIONAL;
                    break;
                case "PT_BR":
                    LangType.CurrentLanguage = LangType.BRAZILIAN_PORTUGUESS;
                    break;
                default:
                    LangType.CurrentLanguage = LangType.ENGLISH;
                    break;
            }

        }
        public string GetMessage(string key)
        {
            UILanguageDefn obj = new UILanguageDefn();
            obj.LoadLanguageData(LanguageDataDoc.DocumentElement);
            return obj.GetTextValue(key); 
        }
    }

}
