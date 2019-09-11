using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCBase.UI;
using JCHVRF_New.Common.Helpers;

namespace JCHVRF_New.Model
{
    public class AccessoryModel: ModelBase, INotifyPropertyChanged
    {
        public string SelectedType { get; set; }
        public string SelectedModelName { get; set; }        
        public string Type { get; set; }
        public string Model { get; set; }
        public List<string> ModelName { get; set; }
        public string Description { get; set; }
        public int MaxCount { get; set; }
        private int _count;
        public int Count
        {
            get { return _count; }
            set
            {
                this.SetValue(ref _count, value);
                if (MaxCount < _count && MaxCount != 0)
                {
                    JCHMessageBox.ShowWarning("Count can not be greater then maxnumber ");
                    _count = MaxCount;
                }
                else if (_count < 0)
                {
                    JCHMessageBox.ShowWarning("Count can not below zero!");
                    _count = MaxCount;
                }
            }
        }
        public bool IsSelect { get; set; }
        public bool IsApplyToSimilarUnit { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
