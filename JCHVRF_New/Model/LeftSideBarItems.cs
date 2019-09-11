

using System.ComponentModel;
using JCHVRF.Model;

namespace JCHVRF_New.Model
{
    using FontAwesome.WPF;
    using JCHVRF_New.Common.Helpers;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    #region Interfaces

    public interface ILeftSideBarItem
    {
        #region Properties

        /// <summary>
        /// Gets the Header
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Gets the MenuHeader
        /// </summary>
        string MenuHeader { get; }

        #endregion
    }

    #endregion
    public class LeftSideBarChild : ModelBase, ILeftSideBarItem, INotifyPropertyChanged
    {
        #region Fields
        private ObservableCollection<LeftSideBarChild> _children;

        private string _header;

        private string _menuHeader;

        private string _icon;
        private SystemBase _source;
        private bool _isEditable;

        //new property:
        private bool _isSelected;
        private bool _isSideBarVisible = true;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged(); }
        }
       
        public bool IsSideBarVisible
        {
            get { return _isSideBarVisible; }
            set { this.SetValue(ref _isSideBarVisible, value); }
        }
        public bool IsEditable
        {
            get { return _isEditable; }
            set { this.SetValue(ref _isEditable, value); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        #region Constructors
        public LeftSideBarChild(string header,string menuheader, string icon, SystemBase source)
        {
            _header = header;
            _menuHeader = menuheader;
            _icon = icon;
            _source = source;
        }

        public string Header
        {
            get { return _header; }
            set
            {
                this.SetValue(ref _header, value);
                RaisePropertyChanged("MenuHeader");
            }
        }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        public string Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                this.SetValue(ref _icon, value);
            }
        }
        public string MenuHeader
        {
            get { return _header; }
        }

        public SystemBase Source
        {
            get
            {
              
                return _source;
            }
            set
            {
                this.SetValue(ref _source, value);
                Icon = _source.StatusIcon;
            }
        }



        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }




            if (!typeof(LeftSideBarChild).IsAssignableFrom(other.GetType()))
            {
                return false;
            }

            LeftSideBarChild systemBase = (LeftSideBarChild)other;

            return this.Source.Equals(systemBase.Source);
        }


        public override int GetHashCode()
        {
            return this.Source.GetHashCode();
        }


        #endregion

    }

    public class LeftSideBarItem: ModelBase, ILeftSideBarItem
    {
        #region Fields
        private ObservableCollection<LeftSideBarChild> _children;

        private string _header;

        //private FontAwesomeIcon _icon;

        #endregion

        #region Constructors
        public LeftSideBarItem(string header)
        {
            _header = header;
           // _icon = icon;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Children
        /// </summary>
        public ObservableCollection<LeftSideBarChild> Children
        {
            get { return _children; }
            set { this.SetValue(ref _children, value); }
        }

        /// <summary>
        /// Gets or sets the Header
        /// </summary>
        public string Header
        {
            get { return _header; }
            set
            {
                this.SetValue(ref _header, value);
                RaisePropertyChanged("MenuHeader");
            }
        }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        //public FontAwesomeIcon Icon
        //{
        //    get { return _icon; }
        //    set
        //    {
        //        this.SetValue(ref _icon, value);
        //    }
        //}

        /// <summary>
        /// Gets the MenuHeader
        /// </summary>
        public string MenuHeader
        {
            get { return _header; }
        }

        #endregion
    }
}
