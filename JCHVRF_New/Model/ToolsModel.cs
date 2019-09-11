using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCHVRF_New.Common.Helpers;
using System.Threading.Tasks;
using FontAwesome.WPF;

namespace JCHVRF_New.Model
{
    public class ToolsModel : ModelBase
    {
        private string header;

        public string Header
        {
            get { return this.header; }
            set { this.SetValue(ref header, value); }
        }

        private string content;

        public string Content
        {
            get { return this.content; }
            set { this.SetValue(ref content, value); }
        }

        private string bottom;

        public string Bottom
        {
            get { return this.bottom; }
            set { this.SetValue(ref bottom, value); }
        }

        private FontAwesomeIcon icon;

        public FontAwesomeIcon Icon
        {
            get { return this.icon; }
            set { this.SetValue(ref icon, value); }
        }

    }
}
