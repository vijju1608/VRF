using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCHVRF_New.Common.Helpers;
using System.Threading.Tasks;
using JCHVRF_New.Model;
using System.Collections.ObjectModel;
using language = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
   public class ToolsViewModel: ViewModelBase
    {
        private ObservableCollection<ToolsModel> _toolCollection;

        public ObservableCollection<ToolsModel> ToolCollection
        {
            get
            {
                if (_toolCollection == null)
                    _toolCollection = new ObservableCollection<ToolsModel>();
                return _toolCollection;
            }
            set { this.SetValue(ref _toolCollection, value); }
        }

        public ToolsViewModel()
        {
            try
            { 
            ToolCollection.Add(new ToolsModel() {

                Icon = FontAwesome.WPF.FontAwesomeIcon.SnowflakeOutline,
                Header = language.Current.GetMessage("HEAT_LOAD_CALCULATOR"),
                Content= "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."
            });

            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.Bolt,
                Header = language.Current.GetMessage("CONSUMPTION_CALCULATOR"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."
            });

            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.Th,
                Header = language.Current.GetMessage("CONTROLLER_SIMULATOR"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."
            });
            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.LineChart,
                Header = language.Current.GetMessage("PEAK_LOAD_CALCULATOR"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."
            });

            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.SnowflakeOutline,
                Header = language.Current.GetMessage("TEMPERATURE_SIMULATOR"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."
            });

            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.Download,
                Header = language.Current.GetMessage("DOWNLOAD_TEMPLATES"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."
            });
            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.Globe,
                Header = language.Current.GetMessage("EMMISSIONS_CALCULATOR"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."

            });

            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.FileText,
                Header = language.Current.GetMessage("SITE_AUDIT_REPORT"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."
            });

            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.FileZipOutline,
                Header = language.Current.GetMessage("STANDARDS_LIBRARY"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."
            });

            ToolCollection.Add(new ToolsModel()
            {
                Icon = FontAwesome.WPF.FontAwesomeIcon.FileText,
                Header = language.Current.GetMessage("PRODUCT_COMPARISIONS"),
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean euismod bibendum reet."

            });

            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }
    }
}
