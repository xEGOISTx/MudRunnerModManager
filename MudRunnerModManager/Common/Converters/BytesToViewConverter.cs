using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using Res = MudRunnerModManager.Lang.Resource;

namespace MudRunnerModManager.Common.Converters
{
	public class BytesToViewConverter : IValueConverter
	{
		private static readonly Dictionary<int, string> _sizeNames = new()
		{
			[0] = Res.Byte,
			[1] = Res.KByte,
			[2] = Res.MByte,
			[3] = Res.GByte,
			[4] = Res.TByte,
		};

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null || !value.GetType().IsPrimitive) 
				return string.Empty;

			decimal val = System.Convert.ToDecimal(value);
			string sizeName = string.Empty;

			for (int i = 0; i < _sizeNames.Count; i++)
			{
				if (val < 1024)
				{
					sizeName = _sizeNames[i];
					break;
				}
				else
					val = val / 1024;
			}

			if(sizeName == Res.GByte || sizeName == Res.TByte)
				return $"{Math.Round(System.Convert.ToDouble(val), 2)} {sizeName}";
			else
				return $"{Math.Floor(val)} {sizeName}";
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
