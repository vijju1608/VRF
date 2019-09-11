using JCHVRF_New.Common.UI.Extensions;
using System;
using System.Collections;
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

namespace JCHVRF_New.Common.Controls
{
    /// <summary>
    /// Interaction logic for JCHTreeComparer.xaml
    /// </summary>
    public partial class JCHTreeComparer : UserControl
    {
        public JCHTreeComparer()
        {
            InitializeComponent();
        }



        public string PropertyToLook
        {
            get { return (string)GetValue(PropertyToLookProperty); }
            set { SetValue(PropertyToLookProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyToLook.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyToLookProperty =
            DependencyProperty.Register("PropertyToLook", typeof(string), typeof(JCHTreeComparer), new PropertyMetadata(null));



        public string LeftHeader
        {
            get { return (string)GetValue(LeftHeaderProperty); }
            set { SetValue(LeftHeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftHeaderProperty =
            DependencyProperty.Register("LeftHeader", typeof(string), typeof(JCHTreeComparer), new PropertyMetadata(string.Empty));

        public string RightHeader
        {
            get { return (string)GetValue(RightHeaderProperty); }
            set { SetValue(RightHeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightHeaderProperty =
            DependencyProperty.Register("RightHeader", typeof(string), typeof(JCHTreeComparer), new PropertyMetadata(string.Empty));




        public ICommand RightArrowCommand
        {
            get { return (ICommand)GetValue(RightArrowCommandProperty); }
            set { SetValue(RightArrowCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightArrowCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightArrowCommandProperty =
            DependencyProperty.Register("RightArrowCommand", typeof(ICommand), typeof(JCHTreeComparer), new PropertyMetadata(null));



        public ICommand LeftArrowCommand
        {
            get { return (ICommand)GetValue(LeftArrowCommandProperty); }
            set { SetValue(LeftArrowCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftArrowCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftArrowCommandProperty =
            DependencyProperty.Register("LeftArrowCommand", typeof(ICommand), typeof(JCHTreeComparer), new PropertyMetadata(null));



        public string LeftSearchText
        {
            get { return (string)GetValue(LeftSearchTextProperty); }
            set { SetValue(LeftSearchTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftSearchText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftSearchTextProperty =
            DependencyProperty.Register("LeftSearchText", typeof(string), typeof(JCHTreeComparer), new PropertyMetadata(string.Empty));

        public string RightSearchText
        {
            get { return (string)GetValue(RightSearchTextProperty); }
            set { SetValue(RightSearchTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightSearchText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightSearchTextProperty =
            DependencyProperty.Register("RightSearchText", typeof(string), typeof(JCHTreeComparer), new PropertyMetadata(string.Empty));




        public IList LeftItemSource
        {
            get { return (IList)GetValue(LeftItemSourceProperty); }
            set { SetValue(LeftItemSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftItemSourceProperty =
            DependencyProperty.Register("LeftItemSource", typeof(IList), typeof(JCHTreeComparer), new PropertyMetadata(null));


        public IList RightItemSource
        {
            get { return (IList)GetValue(RightItemSourceProperty); }
            set { SetValue(RightItemSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightItemSourceProperty =
            DependencyProperty.Register("RightItemSource", typeof(IList), typeof(JCHTreeComparer), new PropertyMetadata(null));

        private void OnToggleTreeItemsState(object sender, RoutedEventArgs e)
        {
            string[] tag = (sender as FrameworkElement)?.Tag?.ToString()?.Split(':');
            if (tag!=null && tag.Length==2) {
                if (tag[0] == "Left")
                {
                    foreach (var item in LeftTreeView.Items)
                    {
                        TreeViewItem treeItem = LeftTreeView.ItemContainerGenerator.ContainerFromItemRecursive(item);
                        treeItem.SetIsExpandedOfAllChildren(tag[1] == "Expand");
                    }
                }
                else
                {
                    foreach (var item in RightTreeView.Items)
                    {
                        TreeViewItem treeItem = RightTreeView.ItemContainerGenerator.ContainerFromItemRecursive(item);
                        treeItem.SetIsExpandedOfAllChildren(tag[1] == "Expand");
                    }
                }
            }
        }

        private void OnMoveToRightClicked(object sender, RoutedEventArgs e)
        {
           TreeViewItem topMostParent = LeftTreeView.ItemContainerGenerator.ContainerFromItemRecursive(LeftTreeView.SelectedItem)?.GetTopMostTreeViewItem();
            if (topMostParent != null)
            {
                var item = topMostParent.DataContext;
                LeftItemSource?.Remove(item);
                RightItemSource?.Add(item);
            }
        }

        private void OnMoveToLeftClicked(object sender, RoutedEventArgs e)
        {
            TreeViewItem topMostParent = RightTreeView.ItemContainerGenerator.ContainerFromItemRecursive(RightTreeView.SelectedItem)?.GetTopMostTreeViewItem();
            if (topMostParent != null)
            {
                var item = topMostParent.DataContext;
                RightItemSource?.Remove(item);
                LeftItemSource?.Add(item);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(PropertyToLook)) return;

            string tag = (sender as TextBox)?.Tag?.ToString();
            string text = (sender as TextBox)?.Text;
            if (tag != null)
            {
                if (tag == "Left")
                {
                    foreach (var item in LeftTreeView.Items)
                    {
                        TreeViewItem treeItem = LeftTreeView.ItemContainerGenerator.ContainerFromItemRecursive(item);
                        treeItem.Visibility = treeItem.DataContext.GetPropValue<string>(PropertyToLook).Contains(text) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else
                {
                    foreach (var item in RightTreeView.Items)
                    {
                        TreeViewItem treeItem = RightTreeView.ItemContainerGenerator.ContainerFromItemRecursive(item);
                        treeItem.Visibility = treeItem.DataContext.GetPropValue<string>(PropertyToLook).Contains(text) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
