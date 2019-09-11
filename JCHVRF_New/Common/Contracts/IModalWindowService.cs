using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Common.Contracts
{
    public interface IModalWindowService
    {
        void ShowView(string viewKey, string title = "", NavigationParameters parameters = null, bool showAsDialog = true, double width = 0, double height = 0);

        void Close(string viewKey);

        NavigationParameters GetParameters(string viewKey);
    }
}
