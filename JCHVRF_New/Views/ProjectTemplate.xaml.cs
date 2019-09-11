using JCHVRF.BLL.New;
using JCHVRF.Entity;
using JCHVRF_New.Model;
using JCHVRF_New.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for ProjectTemplate.xaml
    /// </summary>
    public partial class ProjectTemplate : UserControl
    {

        public static readonly DependencyProperty SearchTypeProperty = DependencyProperty.Register
         (
         "SearchType",
         typeof(string),
         typeof(ProjectTemplate),
         new PropertyMetadata(string.Empty)
         );

        public string SearchType
        {
            get { return (string)GetValue(SearchTypeProperty); }
            set { SetValue(SearchTypeProperty, value); }
        }

        public ProjectTemplate()
        {
            InitializeComponent();
        }


        //public ProjectTemplate(String SearchType)
        //{
        //    InitializeComponent();
        //    //this.initController();
        //    //...
        //}

        //private void btnEdit_Click(object sender, RoutedEventArgs e)
        //{
        //    Button senderButton = sender as Button;
        //    var item = senderButton.DataContext;
        //    var projectID = ((Project)item).ProjectID;
        //    Application.Current.Properties["ProjectId"] = projectID;
        //    ProjectInfoBLL bll = new ProjectInfoBLL();
        //    JCHVRF.Entity.ProjectInfo projectNextGen = JCHVRF.Entity.ProjectInfo.ProjectInfoInstance;
        //    projectNextGen = bll.GetProjectInfo(projectID);
        //    projectNextGen.ProjectLegacy.RegionCode = "EU_W";
        //    projectNextGen.ProjectLegacy.SubRegionCode = "GBR";
        //    projectNextGen.ProjectLegacy.projectID = projectID;

        //    var winMain = new MasterDesigner(projectNextGen.ProjectLegacy);
        //    winMain.ShowDialog();
        //}

        private void btnSummary_Click(object sender, RoutedEventArgs e)
        {
            //JCHMessageBox.Show("This requirement is under discussion!");
        }
    }
}
