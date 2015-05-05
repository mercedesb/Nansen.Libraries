using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Utility.Extensions
{
	public static class EnumExtensions
	{
		/// <summary>
		/// Will get the <seealso cref="DescriptionAttribute"/> assosiated with the enum.
		/// <para>Will return the enum string value if no description is found</para>
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetDescriptionString(this Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());
			
			var descAttributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
			if (descAttributes != null && descAttributes.Length > 0)
				return descAttributes[0].Description;

			return value.ToString();
		}
	}
}
