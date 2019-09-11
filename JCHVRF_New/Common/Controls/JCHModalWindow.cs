using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace JCHVRF_New.Common.Controls
{
    public class JCHModalWindow : Window
    {
        private Window cover;

        public JCHModalWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            ResizeMode = ResizeMode.NoResize;
            HeaderAreaMouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(OnHeaderMousePressed);
            Cover.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(a => a.IsActive && a.Topmost);
            Cover.Show();
            this.Owner = Cover;
        }

        private void OnHeaderMousePressed(MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private Window Cover
        {
            get
            {
                if (this.cover == null)
                {
                    cover = new Window()
                    {
                        WindowStyle = WindowStyle.None,
                        AllowsTransparency = true,
                        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight,
                        MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth,
                        WindowState = WindowState.Maximized,
                        Background = Brushes.Black,
                        Opacity = 0.5,
                        IsHitTestVisible = false,
                        ShowInTaskbar = false
                    };
                }
                return cover;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Cover.Close();
        }


        public DelegateCommand<MouseButtonEventArgs> HeaderAreaMouseDownCommand { get; set; }

    }
}