﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JCHVRF_New.Common.Controls
{


    public class JCHButton : Button
    {
        public enum ButtonType
        {
            Primary,
            Secondary,
            PrimaryOnColor
        }
        public enum PaddingType
        {
            Large,
            Medium,
            Small,
            XS
        }

        static JCHButton()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(JCHButton), new FrameworkPropertyMetadata(typeof(JCHButton)));
        }



        public ButtonType Type
        {
            get { return (ButtonType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(ButtonType), typeof(JCHButton), new PropertyMetadata(ButtonType.Secondary));
        
        public PaddingType PaddingMode
        {
            get { return (PaddingType)GetValue(PaddingModeProperty); }
            set { SetValue(PaddingModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaddingMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaddingModeProperty =
            DependencyProperty.Register("PaddingMode", typeof(PaddingType), typeof(JCHButton), new PropertyMetadata(PaddingType.Small));        
    }
}
