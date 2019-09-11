using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Model
{
    public class TypeModel: INotifyPropertyChanged
    {
        public string  DisplayName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
