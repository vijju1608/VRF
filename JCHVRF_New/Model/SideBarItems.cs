/****************************** File Header ******************************\
File Name:	SideBarItems.cs
Date Created:	2/7/2019
Description:	
\*************************************************************************/

namespace JCHVRF_New.Model
{
    using FontAwesome.WPF;
    using JCHVRF_New.Common.Helpers;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Media;
    using language = JCHVRF_New.LanguageData.LanguageViewModel;

    #region Interfaces

    public interface ISideBarItem
    {
        #region Properties

        /// <summary>
        /// Gets the Header
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Gets the Header
        /// </summary>
        string HeaderKey { get; }

        /// <summary>
        /// Gets the MenuHeader
        /// </summary>
        string MenuViewKey { get; }

        #endregion
    }

    #endregion

    public class SideBarChild : ModelBase, ISideBarItem
    {
        #region Fields

        private ObservableCollection<SideBarChild> _children;

        private string _headerLangKey;

        private string _menuViewKey;

        #endregion

        #region Constructors

        public SideBarChild(string headerLangKey, string menuViewKey)
        {
            _headerLangKey = headerLangKey;
            _menuViewKey = menuViewKey;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Children
        /// </summary>
        public ObservableCollection<SideBarChild> Children
        {
            get { return _children; }
            set { this.SetValue(ref _children, value); }
        }

        /// <summary>
        /// Gets or sets the Header
        /// </summary>
        public string Header
        {
            get { return language.Current.GetMessage(_headerLangKey); }
            set { this.SetValue(ref _headerLangKey, value); }
        }

        /// <summary>
        /// Gets or sets the HeaderKey
        /// </summary>
        public string HeaderKey
        {
            get { return _headerLangKey; }
            set { this.SetValue(ref _headerLangKey, value); }
        }


        /// <summary>
        /// Gets or sets the MenuHeader
        /// </summary>
        public string MenuViewKey
        {
            get { return _menuViewKey; }
            set { this.SetValue(ref _headerLangKey, value); }
        }

        #endregion
    }

    public class SideBarItem : ModelBase, ISideBarItem
    {
        #region Fields

        private ObservableCollection<SideBarChild> _children;

        private string _viewKey;

        private string _headerLangKey;

        private string _icon;

        #endregion

        #region Constructors

        public SideBarItem(string viewKey, string headerLangKey, string icon)
        {
            _viewKey = viewKey;
            _headerLangKey = headerLangKey;
            _icon = icon;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Children
        /// </summary>
        public ObservableCollection<SideBarChild> Children
        {
            get {
                if (_children == null)
                {
                    _children = new ObservableCollection<SideBarChild>();
                }
                return _children; }
            set { this.SetValue(ref _children, value); }
        }

        /// <summary>
        /// Gets or sets the Header
        /// </summary>
        public string Header
        {
            get { return language.Current.GetMessage(_headerLangKey); }
        }

        /// <summary>
        /// Gets or sets the HeaderKey
        /// </summary>
        public string HeaderKey
        {
            get { return _headerLangKey; }
            set { this.SetValue(ref _headerLangKey, value);
                RaisePropertyChanged(nameof(Header));
            }
        }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        public string Icon
        {
            get { return _icon; }
            set
            {
                this.SetValue(ref _icon, value);
            }
        }


        /// <summary>
        /// Gets or sets the MenuHeader
        /// </summary>
        public string MenuViewKey
        {
            get { return _viewKey; }
            set { this.SetValue(ref _viewKey, value); }
        }

        public void RefreshHeaders()
        {
            RaisePropertyChanged(nameof(Header));
            foreach (var item in Children)
            {
                RaisePropertyChanged(nameof(item.Header));
            }
        }

        #endregion
    }
}
