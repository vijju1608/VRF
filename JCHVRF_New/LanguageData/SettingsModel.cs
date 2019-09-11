using JCHVRF_New.LanguageData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JCHVRF_New.Model
{   
    public class SettingsModel
    {
        private JCHVRF_New.ViewModels.SettingsViewModel _globalState;              
        public SettingsModel(JCHVRF_New.ViewModels.SettingsViewModel globalState)
        {
            _globalState = globalState;                      
        }          
    }
}
