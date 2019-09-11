using System;
using System.Collections.Generic;
using JCHVRF_New.Common.Helpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using JCHVRF_New.Model;
using Language = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    public class HelpViewModel : ViewModelBase
    {
        private ObservableCollection<HelpModel> _helpCollection;

        public ObservableCollection<HelpModel> HelpCollection
        {
            get
            {
                if (_helpCollection == null)
                    _helpCollection = new ObservableCollection<HelpModel>();
                return _helpCollection;
            }
            set { this.SetValue(ref _helpCollection, value); }
        }


        public HelpViewModel()
        {
            try
            {
                HelpCollection.Add(new HelpModel()
                {

                    Header = Language.Current.GetMessage("GETTING_STARTED"),//"Getting Started",
                    Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                });
                HelpCollection.Add(new HelpModel()
                {

                    Header = Language.Current.GetMessage("FAQ"),//"FAQs",
                    Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                });
                HelpCollection.Add(new HelpModel()
                {

                    Header = Language.Current.GetMessage("KNOWLEDGE BASE"),//"Knowledge Base",
                    Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                });
                HelpCollection.Add(new HelpModel()
                {

                    Header = Language.Current.GetMessage("ABOUT_VRF_NEXT_GEN"),//"About VRF Next Gen",
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                });
                HelpCollection.Add(new HelpModel()
                {

                    Header = Language.Current.GetMessage("CHAT_WITH_JOHNSON"),//"Chat with Johnson",
                    Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                });
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }

    }
}
