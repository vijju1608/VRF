using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;

namespace JCHVRF_New.LanguageData
{
    public class UILanguageDefn
    {
        public const int DefaultMinFontSize = 12;
        public const int DefaultHeadingFontSize = 16;

        private Dictionary<string, string> _uiText;
        private bool _isRightToLeft;      
        private bool _isLoaded;

        private static Dictionary<string, string> _languageDisplayNames;
        private static List<string> _supportedLanguageCodes;

        static UILanguageDefn()
        {
            _languageDisplayNames = new Dictionary<string, string>();
            _supportedLanguageCodes = new List<string>();
        }

        internal UILanguageDefn()
        {
            _uiText = new Dictionary<string, string>();
            _isRightToLeft = false;            
            _isLoaded = false;
        }
       
        /// <summary>
        /// Gets a value indicating whether a language has been loaded.
        /// </summary>
        /// <value>True if this instance is loaded, otherwise false.</value>
        internal bool IsLoaded
        {
            get { return _isLoaded; }
        }

        /// <summary>
        /// Gets a value indicating whether the language is right-to-left.
        /// </summary>
        /// <value>true if the language is right-to-left, otherwise false.</value>
        public bool IsRightToLeft
        {
            get { return _isRightToLeft; }
        }

        /// <summary>
        /// Gets the localised name of the given language.
        /// </summary>
        /// <param name="queriedLanguage">The language to look up.</param>
        /// <returns></returns>
        public static string GetLanguageName(string queriedLanguageCode)
        {
            if (_languageDisplayNames.ContainsKey(queriedLanguageCode))
                return _languageDisplayNames[queriedLanguageCode];

            return "";
        }

        /// <summary>
        /// Gets the localised text value for the given key.
        /// </summary>
        /// <param name="key">The key of the localised text to retrieve.</param>
        /// <returns>The localised text if found, otherwise an empty string.</returns>
        public string GetTextValue(string key)
        {
            if (_uiText.ContainsKey(key))
                return _uiText[key];

            return "";
        }

        /// <summary>
        /// Loads the language data for a single language.
        /// </summary>
        /// <param name="languageData">The language data.</param>
        internal void LoadLanguageData(XmlElement languageData)
        {
            try
            {
                XmlNodeList mappings = languageData.SelectNodes(Constants.TextEntryXPath);

                //Add key-value pairs for each localised text entry.
                _uiText.Clear();
                foreach (XmlNode currentMapping in mappings)
                {
                    XmlAttribute key = currentMapping.Attributes[0];
                    if (key != null && !String.IsNullOrEmpty(key.Value)&&key.Name.ToLower() == Constants.TextEntryKeyAttr)
                        _uiText.Add(key.Value, currentMapping.InnerText);
                    else
                    {
                        Debug.WriteLine(key);
                    }
                }
                _isLoaded = true;
            }
            catch(Exception ex)
            { }
        }


    }
}
