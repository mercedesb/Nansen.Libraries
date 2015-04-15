using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EPiServer.ContentTransfer.Utility
{
	/// <summary>
	/// Will parse an excel file and put it's content in a hasset
	/// </summary>
	public class ExcelReader
	{
		private Dictionary<string, string[]> _excelData;
		private int _columnKeyIndex;
		private string _fileName;
		private Stream _fileStream;
		private int _skipRowCount;
		public ExcelReader(string fileName, int columnKeyIndex, bool skipFirstRow)
		{
			if (!File.Exists(fileName))
				throw new FileNotFoundException("Can't find file: " + fileName);

			this._fileName = fileName;
			this._columnKeyIndex = columnKeyIndex;
			this._skipRowCount = skipFirstRow ? 1 : 0;
		}

		public ExcelReader(Stream fileStream, int columnKeyIndex, bool skipFirstRow)
		{
			this._fileStream = fileStream;
			this._columnKeyIndex = columnKeyIndex;
			this._skipRowCount = skipFirstRow ? 1 : 0;
		}

		/// <summary>
		/// Will parse the excel file
		/// </summary>
		/// <param name="rowCallback">A callback to execute with the current rows data when itterating over each row</param>
		/// <returns>A list of rows as string[]</returns>
		public Dictionary<string, string[]> ReadToEnd(Action<IEnumerable<string>> rowCallback = null)
		{
			var excelFile = _fileStream == null ? this.ParseExcelFile(_fileName, rowCallback) : this.ParseExcelFile(_fileStream, rowCallback);
			var rows = excelFile.Skip(_skipRowCount)
						 .GroupBy(p => p[_columnKeyIndex])
						 .ToDictionary(p => p.Key, s => s.FirstOrDefault());
			return this._excelData = rows;
		}

		public string[] ReadFirstRow()
		{
			using (SpreadsheetDocument excelDoc = SpreadsheetDocument.Open(_fileStream, false))
			{
				WorkbookPart workbookPart = excelDoc.WorkbookPart;
				WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
				using (OpenXmlReader reader = OpenXmlReader.Create(worksheetPart))
				{
					while (reader.Read())
					{
						if (reader.ElementType == typeof(Row))
						{
							reader.ReadFirstChild();
							var rowData = GetRowData(workbookPart, reader).ToArray();
							if (rowData == null)
								continue;

							return rowData;
						}
					}
				}
			}
			return null;
		}
		/// <summary>
		/// Will get duplicates from the excelfile.
		/// <para>WARNING! Might take a while!</para>
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> FindDuplicates()
		{
			var rows = this.ParseExcelFile(_fileName, null)
						 .Skip(_skipRowCount)
			.GroupBy(p => p[_columnKeyIndex])
			.Where(p => p.Count() > 1)
			.Select(p => p.Key);
			return rows;
		}

		/// <summary>
		/// Will parse the excelfile and put it's content in a HashSet
		/// </summary>
		/// <param name="fileName">The absolute path to the excel file</param>
		/// <returns>A list of string arrays</returns>
		private IEnumerable<string[]> ParseExcelFile(Stream fileStream, Action<IEnumerable<string>> rowCallback)
		{
			return ParseExcelFile(SpreadsheetDocument.Open(fileStream, false), rowCallback);
		}

		/// <summary>
		/// Will parse the excelfile and put it's content in a HashSet
		/// </summary>
		/// <param name="fileName">The absolute path to the excel file</param>
		/// <returns>A list of string arrays</returns>
		private IEnumerable<string[]> ParseExcelFile(string fileName, Action<IEnumerable<string>> rowCallback)
		{
			return ParseExcelFile(SpreadsheetDocument.Open(fileName, false), rowCallback);
		}

		/// <summary>
		/// Will parse the excelfile and put it's content in a HashSet
		/// </summary>
		/// <param name="fileName">The absolute path to the excel file</param>
		/// <returns>A list of string arrays</returns>
		private IEnumerable<string[]> ParseExcelFile(SpreadsheetDocument doc, Action<IEnumerable<string>> rowCallback)
		{
			//using (var package = Package.Open(excelStream, FileMode.OpenOrCreate, FileAccess.ReadWrite))
			using (SpreadsheetDocument excelDoc = doc)
			{
				WorkbookPart workbookPart = excelDoc.WorkbookPart;

				// get first part that actually has rows
				WorksheetPart worksheetPart = workbookPart.WorksheetParts.FirstOrDefault(wsPart =>
					wsPart.Worksheet != null &&
					wsPart.Worksheet.Elements<SheetData>().Any() &&
					wsPart.Worksheet.Elements<SheetData>().First().Elements<Row>().Any());

				// still make sure there is at least one worksheet part (we'll just end up with an empty enumerable)
				if (workbookPart == null)
					worksheetPart = workbookPart.WorksheetParts.First();

				using (OpenXmlReader reader = OpenXmlReader.Create(worksheetPart))
				{
					while (reader.Read())
					{
						if (reader.ElementType == typeof(Row))
						{
							reader.ReadFirstChild();
							var rowData = GetRowData(workbookPart, reader).ToArray();
							if (rowData == null)
								continue;
							if (rowCallback != null)
								rowCallback(rowData);

							yield return rowData;
						}
					}
				}
			}
		}

		/// <summary>
		/// Will itterate over the row to get it's cell data
		/// </summary>
		/// <param name="workbookPart">the workbook part</param>
		/// <param name="reader">the reader where element type has to be typeof Row</param>
		/// <returns>A list of cell values</returns>
		private IEnumerable<string> GetRowData(WorkbookPart workbookPart, OpenXmlReader reader)
		{
			int column = 0;
			List<string> rowData = new List<string>();
			do
			{
				if (reader.ElementType != typeof(Cell))
					continue;

				Cell cell = reader.LoadCurrentElement() as Cell;

				//MB: need this check since OpenXML does not return empty cells
				int readersColumnReference = GetColumnNumber(cell.CellReference);
				while (column < readersColumnReference)
				{
					rowData.Add(string.Empty);
					column++;
				}

				rowData.Add(GetCellValue(cell, workbookPart));
				column++;
			} while (reader.ReadNextSibling());
			return rowData;
		}

		private static int GetColumnNumber(string cellReference)
		{
			// Create a regular expression to match the column name portion of the cell name.
			System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[A-Za-z]+");
			System.Text.RegularExpressions.Match match = regex.Match(cellReference);
			string name = match.Value.ToUpperInvariant();

			int index = 0;
			int pow = 1;
			for (int i = name.Length - 1; i >= 0; i--)
			{

				index += (name[i] - '@') * pow;
				pow *= 26;
			}

			return index - 1;
		}

		/// <summary>
		/// Will parse the cell and get its value from the work books StringTable
		/// </summary>
		/// <param name="theCell"></param>
		/// <param name="wbPart"></param>
		/// <returns></returns>
		private string GetCellValue(Cell theCell, WorkbookPart wbPart)
		{
			if (theCell == null)
				return string.Empty;
			string value = theCell.InnerText;
			if (theCell.DataType == null)
				return value;
			SharedStringTablePart stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
			if (stringTable == null)
				return value;
			switch (theCell.DataType.Value)
			{
				case CellValues.SharedString:
					return stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
				case CellValues.Boolean:
					switch (value)
					{
						case "0":
							return "FALSE";
						default:
							return "TRUE";
					}
			}
			return value;
		}
		/// <summary>
		/// A list of excel rows
		/// </summary>
		public Dictionary<string, string[]> ExcelData
		{
			get
			{
				if (_excelData == null)
					_excelData = this.ReadToEnd();
				return _excelData;
			}
		}
	}
}
