using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JCHVRF_New.Common.UI.Extensions
{
    public static class ExtensionMethods
    {
        public static void SetIsExpandedOfAllChildren(this TreeViewItem item, bool value)
        {
            if (item.HasItems)
            {
                item.IsExpanded = value;
                foreach (var childItem in item.Items)
                {
                    TreeViewItem childTreeItem = item.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
                    childTreeItem.SetIsExpandedOfAllChildren(value);
                }
            }
        }

        public static TreeViewItem ContainerFromItemRecursive(this ItemContainerGenerator root, object item)
        {
            var treeViewItem = root.ContainerFromItem(item) as TreeViewItem;
            if (treeViewItem != null)
                return treeViewItem;
            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                var search = treeViewItem?.ItemContainerGenerator.ContainerFromItemRecursive(item);
                if (search != null)
                    return search;
            }
            return null;
        }

        public static TreeViewItem GetTopMostTreeViewItem(this TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            TreeViewItem topMost = item;
            while (!(parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent is TreeViewItem)
                    topMost = parent as TreeViewItem;
            }
            return topMost;
        }

        public static Object GetPropValue(this Object obj, String name)
        {
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        public static T GetPropValue<T>(this Object obj, String name)
        {
            Object retval = GetPropValue(obj, name);
            if (retval == null) { return default(T); }

            // throws InvalidCastException if types are incompatible
            return (T)retval;
        }

        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
    }
}
