/****************************** File Header ******************************\
File Name:	JCHUIService.cs
Date Created:	2/14/2019
Description:	
\*************************************************************************/

namespace JCHVRF_New.Common.UI.Extensions
{
    using FontAwesome.WPF;
    using System;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using static JCHVRF_New.Common.Controls.JCHIcon;

    public class JCHUIService : DependencyObject
    {
        // Using a DependencyProperty as the backing store for IconColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.RegisterAttached("IconColor", typeof(SolidColorBrush), typeof(JCHUIService), new PropertyMetadata(Brushes.Black));

        // Using a DependencyProperty as the backing store for IconHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.RegisterAttached("IconHeight", typeof(double), typeof(JCHUIService), new PropertyMetadata(Double.NaN));

        public static readonly DependencyProperty IconWidthProperty =
    DependencyProperty.RegisterAttached("IconWidth", typeof(double), typeof(JCHUIService), new PropertyMetadata(Double.NaN));

        // Using a DependencyProperty as the backing store for NotificationCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationCountProperty =
            DependencyProperty.RegisterAttached("NotificationCount", typeof(int), typeof(JCHUIService), new PropertyMetadata(0));

        /// <summary>
        /// The GetAwesomeIcon
        /// </summary>
        /// <param name="obj">The obj<see cref="DependencyObject"/></param>
        /// <returns>The <see cref="FontAwesomeIcon"/></returns>
        public static FontAwesomeIcon GetAwesomeIcon(DependencyObject obj)
        {
            return (FontAwesomeIcon)obj.GetValue(AwesomeIconProperty);
        }

        /// <summary>
        /// The SetAwesomeIcon
        /// </summary>
        /// <param name="obj">The obj<see cref="DependencyObject"/></param>
        /// <param name="value">The value<see cref="FontAwesomeIcon"/></param>
        public static void SetAwesomeIcon(DependencyObject obj, FontAwesomeIcon value)
        {
            obj.SetValue(AwesomeIconProperty, value);
        }

        // Using a DependencyProperty as the backing store for AwesomeIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AwesomeIconProperty =
            DependencyProperty.RegisterAttached("AwesomeIcon", typeof(FontAwesomeIcon), typeof(JCHUIService), new PropertyMetadata(FontAwesomeIcon.None));

        /// <summary>
        /// The GetImagePath
        /// </summary>
        /// <param name="obj">The obj<see cref="DependencyObject"/></param>
        /// <returns>The <see cref="ImageSource"/></returns>
        public static ImageSource GetImagePath(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImagePathProperty);
        }

        /// <summary>
        /// The SetImagePath
        /// </summary>
        /// <param name="obj">The obj<see cref="DependencyObject"/></param>
        /// <param name="value">The value<see cref="ImageSource"/></param>
        public static void SetImagePath(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImagePathProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImagePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.RegisterAttached("ImagePath", typeof(ImageSource), typeof(JCHUIService), new PropertyMetadata(null));

        /// <summary>
        /// The GetVectorResource
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <returns>The <see cref="Geometry"/></returns>
        public static Geometry GetVectorResource(UIElement element)
        {
            return (Geometry)element.GetValue(TypeProperty);
        }

        /// <summary>
        /// The SetVectorResource
        /// </summary>
        /// <param name="element">The element<see cref=""/></param>
        /// <param name="value">The value<see cref="Geometry"/></param>
        public static void SetVectorResource(UIElement element, Geometry value)
        {
            element.SetValue(TypeProperty, value);
        }

        // Using a DependencyProperty as the backing store for VectorResourceProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VectorResourceProperty =
            DependencyProperty.RegisterAttached("VectorResource", typeof(Geometry), typeof(JCHUIService), new PropertyMetadata(null));

        /// <summary>
        /// The GetType
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <returns>The <see cref="IconType"/></returns>
        public static IconType GetIconType(UIElement element)
        {
            return (IconType)element.GetValue(IconTypeProperty);
        }

        /// <summary>
        /// The SetType
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <param name="value">The value<see cref="IconType"/></param>
        public static void SetIconType(UIElement element, IconType value)
        {
            element.SetValue(IconTypeProperty, value);
        }

        // Using a DependencyProperty as the backing store for TypeProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.RegisterAttached("IconType", typeof(IconType), typeof(JCHUIService), new PropertyMetadata(IconType.FontAwesome));

        /// <summary>
        /// The GetIconColor
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <returns>The <see cref="SolidColorBrush"/></returns>
        public static SolidColorBrush GetIconColor(UIElement element)
        {
            return (SolidColorBrush)element.GetValue(IconColorProperty);
        }

        /// <summary>
        /// The GetIconHeight
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <returns>The <see cref="double"/></returns>
        public static double GetIconHeight(UIElement element)
        {
            return (double)element.GetValue(IconHeightProperty);
        }

        /// <summary>
        /// The GetIconWidth
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <returns>The <see cref="double"/></returns>
        public static double GetIconWidth(UIElement element)
        {
            return (double)element.GetValue(IconWidthProperty);
        }

        /// <summary>
        /// The GetNotificationCount
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <returns>The <see cref="int"/></returns>
        public static int GetNotificationCount(UIElement element)
        {
            return (int)element.GetValue(NotificationCountProperty);
        }

        /// <summary>
        /// The SetIconColor
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <param name="value">The value<see cref="SolidColorBrush"/></param>
        public static void SetIconColor(UIElement element, SolidColorBrush value)
        {
            element.SetValue(IconColorProperty, value);
        }

        /// <summary>
        /// The SetIconHeight
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <param name="value">The value<see cref="double"/></param>
        public static void SetIconHeight(UIElement element, double value)
        {
            element.SetValue(IconHeightProperty, value);
        }

        /// <summary>
        /// The SetIconWidth
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <param name="value">The value<see cref="double"/></param>
        public static void SetIconWidth(UIElement element, double value)
        {
            element.SetValue(IconWidthProperty, value);
        }

        /// <summary>
        /// The SetNotificationCount
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <param name="value">The value<see cref="int"/></param>
        public static void SetNotificationCount(UIElement element, int value)
        {
            element.SetValue(NotificationCountProperty, value);
        }

        /// <summary>
        /// The GetGlyphVisibility
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <returns>The <see cref="int"/></returns>
        public static Visibility GetGlyphVisibility(UIElement element)
        {
            return (Visibility)element.GetValue(GlyphVisibilityProperty);
        }

        /// <summary>
        /// The SetGlyphVisibility
        /// </summary>
        /// <param name="element">The element<see cref="UIElement"/></param>
        /// <param name="value">The value<see cref="int"/></param>
        public static void SetGlyphVisibility(UIElement element, Visibility value)
        {
            element.SetValue(GlyphVisibilityProperty, value);
        }



        public static Thickness GetIconPadding(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(IconPaddingProperty);
        }

        public static void SetIconPadding(DependencyObject obj, Thickness value)
        {
            obj.SetValue(IconPaddingProperty, value);
        }

        // Using a DependencyProperty as the backing store for IconPadding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconPaddingProperty =
            DependencyProperty.RegisterAttached("IconPadding", typeof(Thickness), typeof(JCHUIService), new PropertyMetadata(new Thickness(0)));
        
        // Using a DependencyProperty as the backing store for GlyphVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GlyphVisibilityProperty =
            DependencyProperty.Register("GlyphVisibility", typeof(Visibility), typeof(JCHUIService), new PropertyMetadata(Visibility.Collapsed));        
    }
}