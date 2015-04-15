using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace EPiServer.ContentTransfer.Utility
{
	public static class ObjectExtensions
	{
		public static DataTable ToDataTable<T>(this IEnumerable<T> source, PropertyInfo[] properties = null)
		{
			DataTable dt = new DataTable();

			PropertyInfo[] propertyArray;
			if (properties != null)
			{
				propertyArray = properties;
			}
			else
			{
				propertyArray = typeof(T).GetProperties();
			}

			foreach (PropertyInfo info in propertyArray)
			{
				if (info.Name == "Item")
					continue;

				dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));
			}

			AddRowsToDataTable(source, dt, propertyArray);
			return dt;
		}

		public static DataTable ToDataTable<T>(this IEnumerable<T> source, PropertyInfo[] properties, string[] headers = null)
		{
			if (headers == null || properties.Length != headers.Length)
				return ToDataTable(source, properties);

			DataTable dt = new DataTable();
			for (int i = 0; i < headers.Length; i++)
			{
				dt.Columns.Add(new DataColumn(headers[i], GetNullableType(properties[i].PropertyType)));
			}

			AddRowsToDataTable(source, dt, properties);
			return dt;
		}

		private static void AddRowsToDataTable<T>(IEnumerable<T> source, DataTable dt, PropertyInfo[] properties)
		{
			foreach (T obj in source)
			{
				DataRow row = dt.NewRow();
				foreach (PropertyInfo info in properties)
				{
					if (info.Name == "Item")
						continue;

					if (!IsNullableType(info.PropertyType))
						row[info.Name] = info.GetValue(obj, null);
					else
						row[info.Name] = (info.GetValue(obj, null) ?? DBNull.Value);
				}
				dt.Rows.Add(row);
			}
		}

		private static Type GetNullableType(this Type t)
		{
			Type returnType = t;
			if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				returnType = Nullable.GetUnderlyingType(t);
			}
			return returnType;
		}

		private static bool IsNullableType(this Type type)
		{
			return (type == typeof(string) ||
					type.IsArray ||
					(type.IsGenericType &&
					 type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
		}
	}
}
