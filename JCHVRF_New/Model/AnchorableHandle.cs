using JCHVRF_New.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Controls;

namespace JCHVRF_New.Model
{
    public class AnchorableHandle : ModelBase
    {
        private string _title;

        public string Title
        {
            get { return this._title; }
            set { this.SetValue(ref _title, value); }
        }

        private double _floaterHeight;

        public double FloaterHeight
        {
            get { return this._floaterHeight; }
            set { this.SetValue(ref _floaterHeight, value); }
        }

        private double _floaterWidth;

        public double FloaterWidth
        {
            get { return this._floaterWidth; }
            set { this.SetValue(ref _floaterWidth, value); }
        }

        private double _floaterLeft;

        public double FloaterLeft
        {
            get { return this._floaterLeft; }
            set { this.SetValue(ref _floaterLeft, value); }
        }

        private double _floaterTop;

        public double FloaterTop
        {
            get { return this._floaterTop; }
            set { this.SetValue(ref _floaterTop, value); }
        }

        private bool _isPanevisible;

        public bool IsPaneVisible
        {
            get { return this._isPanevisible; }
            set { this.SetValue(ref _isPanevisible, value);
                RaisePropertyChanged("IsPaneVisibleAndDocked");
            }
        }

        private bool _canTogglePaneVisibility;

        public bool CanTogglePaneVisibility
        {
            get { return this._canTogglePaneVisibility; }
            set
            {
                this.SetValue(ref _canTogglePaneVisibility, value);
                IsPaneVisible = value;
            }
        }

        private bool _isPaneFloating;

        public bool IsPaneFloating
        {
            get { return this._isPaneFloating; }
            set { this.SetValue(ref _isPaneFloating, value);
                RaisePropertyChanged("IsPaneVisibleAndDocked");
            }
        }

        private bool _isPaneFloatingMinimized;

        public bool IsPaneFloatingMinimized
        {
            get { return this._isPaneFloatingMinimized; }
            set { this.SetValue(ref _isPaneFloatingMinimized, value); }
        }

        private bool _isPaneSelected;

        public bool IsPaneSelected
        {
            get { return this._isPaneSelected; }
            set { this.SetValue(ref _isPaneSelected, value); }
        }



        public bool IsPaneVisibleAndDocked { get {
                return IsPaneVisible && !IsPaneFloating;
            } }
    }
}
