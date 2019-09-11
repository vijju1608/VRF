using FontAwesome.WPF;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JCHVRF_New.Common.Controls
{
    public class JCHIcon : Control
    {
        public enum IconType
        {
            FontAwesome,
            Vector,
            Image
        }

        static JCHIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JCHIcon), new FrameworkPropertyMetadata(typeof(JCHIcon)));
        }

        public FontAwesomeIcon AwesomeIcon
        {
            get { return (FontAwesomeIcon)GetValue(AwesomeIconProperty); }
            set { SetValue(AwesomeIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AwesomeIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AwesomeIconProperty =
            DependencyProperty.Register("AwesomeIcon", typeof(FontAwesomeIcon), typeof(JCHIcon), new PropertyMetadata(FontAwesomeIcon.None));
        
        public IconType Type
        {
            get { return (IconType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(IconType), typeof(JCHIcon), new PropertyMetadata(IconType.FontAwesome));

        public Geometry VectorResource
        {
            get { return (Geometry)GetValue(VectorResourceProperty); }
            set { SetValue(VectorResourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VectorResource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VectorResourceProperty =
            DependencyProperty.Register("VectorResource", typeof(Geometry), typeof(JCHIcon), new PropertyMetadata(null));

        public ImageSource ImagePath
        {
            get { return (ImageSource)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImagePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register("ImagePath", typeof(ImageSource), typeof(JCHIcon), new PropertyMetadata(null));



    }
}