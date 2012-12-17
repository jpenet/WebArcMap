using System;
using System.Linq;

namespace Silverlight.Helper.General
{
	public static class Utilities
	{
		/// <summary>
		/// Verify if it is a meaningfull field
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool IsValidField(string fieldName)
		{
			string[] invalidNames = { "OBJECTID" };
			if (invalidNames.FirstOrDefault(n => n.Equals(fieldName.ToUpper())) == null)
				return true;
			else
				return false;
		}

		public static bool IsValidField(string fieldName, string objectId)
		{
			string[] invalidNames = { objectId };
			if (invalidNames.FirstOrDefault(n => n.Equals(fieldName)) == null)
				return true;
			else
				return false;
		}


		/// <summary>
		/// Convert value of a feature to a string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string FormatField(object value)
		{
			if (value.GetType() == typeof(string))
				return (string)value;
			if (value.GetType() == typeof(int) || value.GetType() == typeof(Int16) || value.GetType() == typeof(Int32))
				return ((int)value).ToString("########");
			if (value.GetType() == typeof(double))
				return ((double)value).ToString("##########.0000");
			return value.ToString();
		}
	}
}
