using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace JCHVRF_New.LanguageData
{
    public class LocalisedTextExtension : MarkupExtension
    {
        private Binding _lookupBinding;
        public LocalisedTextExtension()
        {
            _lookupBinding = UITextLookupConverter.CreateBinding("");
        }

        [System.ComponentModel.DefaultValue("")]
        public string Key
        {
            get { return (string)_lookupBinding.ConverterParameter; }
            set { _lookupBinding.ConverterParameter = value; }
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _lookupBinding.ProvideValue(serviceProvider);
        }


    }
}
