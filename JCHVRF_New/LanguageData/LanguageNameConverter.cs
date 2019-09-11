/*
 * RePaver
 * 
 * Copyright (c)2009, Daniel McGaughran
 * 
 * Licence:	CodeProject Open Licence (CPOL) 1.2
 *			Please refer to 'Licence.txt' for further details of this licence.
 *			Alternatively, the licence may be viewed at:
 *			http://www.codeproject.com/info/cpol10.aspx
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using JCHVRF_New.LanguageData;
//using RePaverModel.Entities;

namespace JCHVRF_New.Model
{
	[ValueConversion(typeof(string), typeof(string))]
	public class LanguageNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string currentLang = value as string;

			string langName = UILanguageDefn.GetLanguageName(currentLang);

			return langName;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}


	}
}
