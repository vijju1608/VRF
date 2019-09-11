/****************************** File Header ******************************\
File Name:	ViewModelBase.cs
Date Created:	2/6/2019
Description:	Base class for all ViewModels in the apllication.
               Added more support Regions and Navigation.
\*************************************************************************/

namespace JCHVRF_New.Common.Helpers
{
    using System;
    using Prism;
    using Prism.Regions;

    public class ViewModelBase : ModelBase, IRegionMemberLifetime, INavigationAware, IActiveAware
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether KeepAlive
        /// It is related to region and navigation, 
        /// if it returns false, On Navigating Away to a different View
        /// This will also be removed from the region. And next time will instantiate again.
        /// </summary>
        public virtual bool KeepAlive
        {
            get { return false; }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                SetValue(ref _isActive, value);
                  RaiseIsActiveChanged();
            }

        }

        protected virtual void RaiseIsActiveChanged()
        {
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IsActiveChanged;

        private bool _isBusy;

        public bool IsBusy
        {
            get { return this._isBusy; }
            set { this.SetValue(ref _isBusy, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// If this returns true, then existing view will be opened if exists in the region.
        /// Otherwise a new view will be added everytime.
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// The OnNavigatedFrom
        /// </summary>
        /// <param name="navigationContext">The navigationContext<see cref="NavigationContext"/></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// The OnNavigatedTo
        /// </summary>
        /// <param name="navigationContext">The navigationContext<see cref="NavigationContext"/></param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        #endregion
    }
}
