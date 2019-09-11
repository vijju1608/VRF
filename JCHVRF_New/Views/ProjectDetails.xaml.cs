using JCHVRF_New.Model;
using JCHVRF_New.ViewModels;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ProjectDetails.xaml
    /// </summary>    
    public partial class ProjectDetails : UserControl
    {
        public ProjectDetails()
        {
            InitializeComponent();

        }
        private void textBlockHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FindTreeItem(e.OriginalSource as DependencyObject).IsSelected)
            {
               
                e.Handled = true; // otherwise the newly activated control will immediately loose focus
            }
        }

        
        static TreeViewItem FindTreeItem(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
        }

        private void ProjectsView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = (sender as TreeView).SelectedItem as LeftSideBarChild;
            if (selected != null)
            {
                selected.IsEditable = true;
                selected.IsSideBarVisible = false;
                var ucDesignerCanvasViewModel = (ProjectDetailsViewModel)this.DataContext;
                ucDesignerCanvasViewModel.isEditable = selected.IsEditable;
                ucDesignerCanvasViewModel.isSideBarVisible = selected.IsSideBarVisible;
                ucDesignerCanvasViewModel.RefreshSystemsList();
                //ucDesignerCanvasViewModel.RaisePropertyChanged("AllSystems");
            }

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var selected = (sender as TextBox).DataContext as LeftSideBarChild;
            if (selected != null)
            {
                selected.IsEditable = false;
                selected.IsSideBarVisible = true;
                selected.Source.Name = selected.Header;
                var ucDesignerCanvasViewModel = (ProjectDetailsViewModel)this.DataContext;
                ucDesignerCanvasViewModel.isEditable = selected.IsEditable;
                ucDesignerCanvasViewModel.isSideBarVisible = selected.IsSideBarVisible;
                ucDesignerCanvasViewModel.RefreshSystemsList();
            }
        }
    }
}