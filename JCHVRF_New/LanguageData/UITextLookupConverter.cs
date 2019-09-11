using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JCHVRF_New.LanguageData
{
    [ValueConversion(typeof(UILanguageDefn), typeof(string))]
    class UITextLookupConverter : IValueConverter
    {
        private static UITextLookupConverter _sharedConverter;

        static UITextLookupConverter()
        {
            _sharedConverter = new UITextLookupConverter();
        }
        public static Binding CreateBinding(string key)
        {
            Binding languageBinding = new Binding("LanguageDefn")
            {
                Source = LanguageViewModel.Current,
                Converter = _sharedConverter,
                ConverterParameter = key,
            };
            return languageBinding;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key = parameter as string;
            UILanguageDefn defn = value as UILanguageDefn;

            if (defn == null || key == null) return "";

            return defn.GetTextValue(key);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the localised UI text for the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The localised text.</returns>
        internal static string GetText(string key)
        {
            UILanguageDefn languageDefn = LanguageViewModel.Current.LanguageDefn;
            if (languageDefn == null || String.IsNullOrEmpty(key))
                return "";

            return languageDefn.GetTextValue(key);
        }

        public object convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object convertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
