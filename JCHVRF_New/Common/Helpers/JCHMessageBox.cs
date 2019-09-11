using JCHVRF_New.Common.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JCHVRF_New.Common.Helpers
{
    public enum MessageType
    {
        Information,
        Error,
        Warning,
        Success
    }

    public static class JCHMessageBox
    {
        public static MessageBoxResult ShowWarning(string message = "", MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return Show(message, MessageType.Warning, buttons);
        }
        public static MessageBoxResult ShowError(string message = "", MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return Show(message, MessageType.Error, buttons);
        }
        public static MessageBoxResult ShowSuccess(string message = "", MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return Show(message, MessageType.Success, buttons);
        }

        public static MessageBoxResult Show(string message = "", MessageType type = MessageType.Information, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            JCHAlertDialog dialog = new JCHAlertDialog(message, type, buttons);
            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}
