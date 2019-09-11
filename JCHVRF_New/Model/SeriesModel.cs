using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Model
{
    public class SeriesModel
    {
        public string DisplayName { get; set; }
        public string SelectedValues { get; set; }
        public string OduImagePath { get; set; }
        public string ExchImagePath { get; set; }

        public bool IsSelected { get; set; }

        public string FullModelName { get; set; }

    }
}
