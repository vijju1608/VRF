using System;
using System.Linq;
using JCHVRF.Const;
using JCHVRF.Model.NextGen;
using NextGenModel = JCHVRF.Model.NextGen;
using Lassalle.WPF.Flow;

namespace JCHVRF_New.Common.Helpers
{
    class SystemValidationHelper
    {
        private static readonly JCHVRF_New.LanguageData.LanguageViewModel CurrentLanguage = JCHVRF_New.LanguageData.LanguageViewModel.Current;
       
        public static bool IsSystemValidForDuplication(NextGenModel.SystemVRF vrfSystem, out string validationMessage)
        {
            var result = true;
            validationMessage = string.Empty;
            var nodeOut = vrfSystem.MyPipingNodeOut;
            if (nodeOut == null || nodeOut.ChildNode == null)
            {
                result = false;
                validationMessage = CurrentLanguage.GetMessage("DUPLICATION_ERROR_INVALID_SYSTEM");
            }
            return result;
        }
    }
}
