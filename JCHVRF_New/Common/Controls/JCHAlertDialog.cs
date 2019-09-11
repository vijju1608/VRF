using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace JCHVRF_New.Common.Controls
{
    public class JCHAlertDialog : JCHModalWindow
    {
        public JCHAlertDialog() : base()
        {
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.WidthAndHeight;
            Topmost = true;
        }

        public DelegateCommand<string> SelectionCommand
        {
            get { return (DelegateCommand<string>)GetValue(SelectionCommandProperty); }
            set { SetValue(SelectionCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionCommandProperty =
            DependencyProperty.Register("SelectionCommand", typeof(DelegateCommand<string>), typeof(JCHAlertDialog), new PropertyMetadata(null));


        public JCHAlertDialog(string message = "", MessageType type = MessageType.Information, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            Message = message;    
            Type = type;
            SelectionCommand = new DelegateCommand<string>(OnClick);

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    IsOkVisible = true;
                    break;
                case MessageBoxButton.OKCancel:
                    IsOkVisible = true;
                    IsCancelVisible = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    AreYesNoVisible = true;
                    IsCancelVisible = true;
                    break;
                case MessageBoxButton.YesNo:
                    AreYesNoVisible = true;
                    break;
                default:
                    break;
            }
        }

        public bool AreYesNoVisible
        {
            get { return (bool)GetValue(AreYesNoVisibleProperty); }
            set { SetValue(AreYesNoVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AreYesNoVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AreYesNoVisibleProperty =
            DependencyProperty.Register("AreYesNoVisible", typeof(bool), typeof(JCHAlertDialog), new PropertyMetadata(false));
        
        public bool IsCancelVisible
        {
            get { return (bool)GetValue(IsCancelVisibleProperty); }
            set { SetValue(IsCancelVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCancelVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCancelVisibleProperty =
            DependencyProperty.Register("IsCancelVisible", typeof(bool), typeof(JCHAlertDialog), new PropertyMetadata(false));


        public bool IsOkVisible
        {
            get { return (bool)GetValue(IsOkVisibleProperty); }
            set { SetValue(IsOkVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOkVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOkVisibleProperty =
            DependencyProperty.Register("IsOkVisible", typeof(bool), typeof(JCHAlertDialog), new PropertyMetadata(false));
        
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(JCHAlertDialog), new PropertyMetadata(string.Empty));

        public MessageType Type
        {
            get { return (MessageType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(MessageType), typeof(JCHAlertDialog), new PropertyMetadata(MessageType.Information));


        public MessageBoxResult Result
        {
            get { return (MessageBoxResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Result.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(MessageBoxResult), typeof(JCHAlertDialog), new PropertyMetadata(MessageBoxResult.None));

        private void OnClick(string sender)
        {
            switch (sender)
            {
                case "Ok":
                    Result = MessageBoxResult.OK;
                    break;
                case "Yes":
                    Result = MessageBoxResult.Yes;
                    break;
                case "No":
                    Result = MessageBoxResult.No;
                    break;
                case "Cancel":
                    Result = MessageBoxResult.Cancel;
                    break;
            }
            this.Close();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
