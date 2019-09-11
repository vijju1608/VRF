using JCBase.Utility;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using language = JCHVRF_New.LanguageData.LanguageViewModel;
namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for InputPipelength.xaml
    /// </summary>
    public partial class FindZeroLength : Window, IClosable
    {
        string ut_length = string.Empty;

        public FindZeroLength(double Length,int totalZeroLengthPipes)
        {
            SizeChanged += (o, e) =>
            {
                var r = SystemParameters.WorkArea;
                Left = r.Right - 450 - ActualWidth;
                Top = r.Bottom - 450 - ActualHeight;
            };
            InitializeComponent();
            ut_length = JCHVRF.Model.SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            ((FindZeroLengthViewModel)this.DataContext).ResultTextZeroPipeLength = language.Current.GetMessage("TOTAL") + totalZeroLengthPipes + language.Current.GetMessage("PIPE_LENGTH");
            ((FindZeroLengthViewModel)this.DataContext).PipeLength = Length.ToString("n2");//Unit.ConvertToControl(Length, UnitType.LENGTH_M, ut_length).ToString("n2");
            //FocusManager.SetFocusedElement(spFindZero, FindZero);
            //Loaded += (o, e) =>
            //{
            //    FindZero.Focus();
            //};

        }
        public void RequestClose()
        {
            this.Close();
        }

        private void FindZero_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.All(Char.IsLetter);         
        }

        private void FindNext_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((FindZeroLengthViewModel)this.DataContext).OnNextClickFindZeroLength();
            }
        }
    }
}
