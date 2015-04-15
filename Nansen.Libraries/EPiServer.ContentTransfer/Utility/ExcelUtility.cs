using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentTransfer.Utility
{
	public static class ExcelUtility
	{
		public static Dictionary<string, string[]> GetExcelData(Stream fileStream, int idColumn)
		{
			return GetExcelData(fileStream, idColumn, true);
		}

		public static Dictionary<string, string[]> GetExcelData(Stream fileStream, int idColumn, bool skipFirstRow)
		{
			ExcelReader reader = new ExcelReader(fileStream, idColumn, skipFirstRow);
			reader.ReadToEnd();
			return reader.ExcelData;
		}
	}
}
