using System.ComponentModel;

namespace JCHVRF_New.Model
{
    using FontAwesome.WPF;
    using JCHVRF_New.Common.Helpers;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    #region Interfaces

    public interface ICanvasListItems
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
    public class CanvasItemChild : ModelBase, ICanvasListItems, INotifyPropertyChanged
    {
        #region Fields
        private ObservableCollection<CanvasItemChild> _children;

        private string _header;

        private string _menuHeader;

        private string _icon;
        private object _source;

        //new property:
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        #region Constructors
        public CanvasItemChild(string header,string menuheader, string icon, object source)
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
            get { return _icon; }
            set
            {
                this.SetValue(ref _icon, value);
            }
        }
        public string MenuHeader
        {
            get { return _header; }
        }

        public object Source
        {
            get { return _source; }
        }

        #endregion

    }

    public class CanvasListItem: ModelBase, ICanvasListItems
    {
        #region Fields
        private ObservableCollection<CanvasItemChild> _children;

        private string _header;

        //private FontAwesomeIcon _icon;

        #endregion

        #region Constructors
        public CanvasListItem(string header)
        {
            _header = header;
           // _icon = icon;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Children
        /// </summary>
        public ObservableCollection<CanvasItemChild> Children
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
