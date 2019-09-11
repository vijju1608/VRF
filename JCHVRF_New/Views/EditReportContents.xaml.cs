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
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JCHVRF.BLL;
using JCHVRF.Model.NextGen;
using JCHVRF.Model;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for EditReportContents.xaml
    /// </summary>
    public partial class EditReportContents : UserControl
    {
        TreeViewItem item;
        ItemsControl Includedparent;
        public EditReportContents()
        {
            InitializeComponent();
            BindToControl_ReportItems();
        }
        public ObservableCollection<JCHVRF_New.Model.LeftSideBarItem> RightSideBarItems { get; set; }
        private void BindToControl_ReportItems()
        {
            treeView1.ClearValue(ItemsControl.ItemsSourceProperty);
            treeView1.Items.Clear();
            treeView2.Items.Clear();
            foreach (JCHVRF.Model.NextGen.SystemVRF sysItem in JCHVRF.Model.Project.CurrentProject.SystemListNextGen)
            {
                if (sysItem.OutdoorItem == null)
                    continue;
                if (sysItem.editrpt == true)
                {
                    string text = sysItem.Name + "[" + sysItem.OutdoorItem.AuxModelName + "]";
                    if (sysItem.IsExportToReport == true)
                        treeView1.Items.Add(text);
                    else
                        treeView2.Items.Add(text);
                }
                else
                {
                    sysItem.editrpt = true;
                    //string text = sysItem.Name + "[" + sysItem.OutdoorItem.ModelFull + "]";  
                    string text = sysItem.Name + "[" + sysItem.OutdoorItem.AuxModelName + "]";
                    sysItem.IsExportToReport = true;
                    treeView1.Items.Add(text);
                }
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selecteditem = treeView1.SelectedItem.ToString();
                treeView2.Items.Add(treeView1.SelectedItem);
                treeView1.Items.Remove(treeView1.SelectedItem);
                JCHVRF.Model.Project.CurrentProject.SystemListNextGen.ForEach((p) =>
                {
                    if (selecteditem == p.Name + "[" + p.OutdoorItem.AuxModelName + "]")
                    {
                        p.IsExportToReport = false;
                    }
                });
            }
            catch (Exception ex)
            {
            }
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selecteditem = treeView2.SelectedItem.ToString();
                treeView1.Items.Add(treeView2.SelectedItem);
                treeView2.Items.Remove(treeView2.SelectedItem);
                JCHVRF.Model.Project.CurrentProject.SystemListNextGen.ForEach((p) =>
                {
                    if (selecteditem == p.Name + "[" + p.OutdoorItem.AuxModelName + "]")
                    {
                        p.IsExportToReport = true;
                    }
                });
            }
            catch
            {
            }
        }
        //private void AddButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var proj = JCHVRF.Model.Project.GetProjectInstance;
        //        var treevalue = treeView1.SelectedItem;
        //        TreeViewItem RightSideBarItems = new TreeViewItem();
        //        TreeViewItem remove = new TreeViewItem();
        //        TreeViewItem Left = new TreeViewItem();
        //        if (parent.Parent != null)
        //        {
        //            RightSideBarItems = new TreeViewItem() { Header = treeView1.SelectedItem };
        //        }
        //        else
        //        {
        //            RightSideBarItems = new TreeViewItem() { Header = parent };
        //        }
        //        for (int i = 0; i < treeView1.Items.Count; i++)
        //        {

        //            remove.Items.Add(treeView1.Items[i]);
        //        }
        //        try
        //        {
        //            var treevalue1 = (JCHVRF_New.Model.LeftSideBarChild)treevalue;



        //            if (treeView2.Items.Contains(RightSideBarItems.Header))
        //            {
        //                // RightSideBarItems.Header = (((System.Windows.Controls.HeaderedItemsControl)RightSideBarItems.Header));
        //                RightSideBarItems.ItemsSource = new JCHVRF_New.Model.LeftSideBarChild[] { (JCHVRF_New.Model.LeftSideBarChild)treevalue };
        //                treeView2.Items.Add(RightSideBarItems);
        //                for (int i = 0; i < treeView1.Items.Count; i++)
        //                {
        //                    if (treeView1.Items[i] == (JCHVRF_New.Model.LeftSideBarChild)treevalue)
        //                        remove.Items.Remove(treeView1.Items[i]);
        //                }
        //                // treeView1.Items.Remove(RightSideBarItems.ItemsSource);
        //            }
        //            else
        //            {

        //                RightSideBarItems.ItemsSource = new JCHVRF_New.Model.LeftSideBarChild[] { (JCHVRF_New.Model.LeftSideBarChild)treevalue };
        //                treeView2.Items.Add(RightSideBarItems);
        //                for (int i = 0; i < treeView1.Items.Count; i++)
        //                {
        //                    if (treeView1.Items[i] == (JCHVRF_New.Model.LeftSideBarChild)treevalue)
        //                        remove.Items.Remove(treeView1.Items[i]);
        //                }
        //                //  treeView1.Items.Remove(RightSideBarItems.ItemsSource);
        //            }

        //        }
        //        catch
        //        {
        //            if (parent.Parent != null)
        //            {
        //                treeView2.Items.Add(RightSideBarItems.Header);
        //                treeView2.Items.Remove(RightSideBarItems.Header);
        //                remove.Items.Clear();
        //            }
        //            else
        //            {
        //                treeView2.Items.Add(((System.Windows.Controls.HeaderedItemsControl)RightSideBarItems.Header).Header);
        //                treeView1.Items.Remove(((System.Windows.Controls.HeaderedItemsControl)RightSideBarItems.Header).Header);
        //                remove.Items.Clear();
        //            }
        //        }



        //        treeView1.ClearValue(ItemsControl.ItemsSourceProperty);
        //        if (remove.Items.Count == 1)
        //            treeView1.Items.Add(remove.Items[0]);
        //        else if (remove.Items.Count == 0)
        //        {
        //            treeView1.Items.Clear();
        //        }
        //        else
        //            for (int i = 0; i < remove.Items.Count; i++)
        //            {
        //                treeView1.Items.Add(remove.Items[i]);
        //            }

        //        parent.ClearValue(ItemsControl.ItemsSourceProperty);

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}


        //private void RemoveButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var proj = JCHVRF.Model.Project.GetProjectInstance;
        //        TreeViewItem Left = new TreeViewItem();
        //        if (Includedparent.Parent != null)
        //        {
        //            TreeViewItem RightSideBarItems = new TreeViewItem() { Header = treeView2.SelectedItem };
        //            treeView1.Items.Add(RightSideBarItems.Header);
        //            for (int i = 0; i < treeView2.Items.Count; i++)
        //            {
        //                if (treeView2.Items[i] != RightSideBarItems.Header)
        //                {
        //                    Left.Items.Add(treeView2.Items[i]);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            TreeViewItem RightSideBarItems = new TreeViewItem() { Header = Includedparent };
        //            treeView1.Items.Add(((System.Windows.Controls.HeaderedItemsControl)RightSideBarItems.Header).Header);
        //            for (int i = 0; i < treeView2.Items.Count; i++)
        //            {

        //                if (treeView2.Items[i] != ((System.Windows.Controls.HeaderedItemsControl)RightSideBarItems.Header).Header)
        //                {
        //                    Left.Items.Add(treeView2.Items[i]);
        //                }
        //            }
        //        }
        //        if (Left.Items.Count != 0)
        //            treeView2.ClearValue(ItemsControl.ItemsSourceProperty);
        //        if (Left.Items.Count == 1)
        //            treeView2.Items.Add(Left.Items[0]);
        //        else if (Left.Items.Count == 0)
        //        {
        //            treeView2.Items.Clear();
        //        }
        //        else
        //            for (int i = 0; i < Left.Items.Count; i++)
        //            {
        //                treeView2.Items.Add(Left.Items[i]);
        //            }
        //        parent.ClearValue(ItemsControl.ItemsSourceProperty);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        private void txtSearchSelectedItem_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchSelectedItem.Text == "Search")
                txtSearchSelectedItem.Text = "";
        }

        private void txtSearchSelectedItem_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchSelectedItem.Text.Trim() == "")
                txtSearchSelectedItem.Text = "Search";
        }
        private void txtSearchExcludedItem_GotFocusobject(object sender, RoutedEventArgs e)
        {
            if (txtSearchExcludedItem.Text == "Search")
                txtSearchExcludedItem.Text = "";
        }

        private void txtSearchExcludedItem_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchExcludedItem.Text.Trim() == "")
                txtSearchExcludedItem.Text = "Search";
        }
        private void treeView1_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }
        private void treeView1_Selected(object sender, RoutedEventArgs e)
        {
            //item = e.OriginalSource as TreeViewItem;
            //if (item != null)
            //{
            //    parent = ItemsControl.ItemsControlFromItemContainer(item);

            //}
        }
        private void treeView2_Selected(object sender, RoutedEventArgs e)
        {
            item = e.OriginalSource as TreeViewItem;
            if (item != null)
            {
                Includedparent = ItemsControl.ItemsControlFromItemContainer(item);
            }
        }
    }
}


