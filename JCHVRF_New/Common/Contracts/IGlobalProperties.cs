using JCHVRF.Model.NextGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Common.Contracts
{
    public interface IGlobalProperties
    {
        string ProjectTitle { get; set; }

        //Bug 4253
        Dictionary<int, HashSet<int>> DefaultAccessoryDictionary { get; set; }
        //Bug 4253

        ObservableCollection<Notification> Notifications { get; }
    }
}
