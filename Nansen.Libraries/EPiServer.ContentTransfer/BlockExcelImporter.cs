using EPiServer.ContentTransfer.Utility;
using EPiServer.Core;
using System.Collections.Generic;
using System.IO;

namespace EPiServer.ContentTransfer
{
	public abstract class BlockExcelImporter<T, TEnum> : BaseContentExcelImporter<T, TEnum> where T : BlockData where TEnum : struct
	{
		public BlockExcelImporter(List<string> errors, int containerRefId, string language = "", int idColumnIndex = 0, bool overwrite = false)
		: base(errors, containerRefId, language, idColumnIndex, overwrite)
		{ }
		
		protected override void HandleRowsFromExcel(Stream fileStream)
		{
			//Get the file
			Dictionary<string, string[]> excelData;
			try
			{
				excelData = ExcelUtility.GetExcelData(fileStream, 0);
			}
			catch
			{
				Errors.Add("The file could not be found on the server.");
				return;
			}

			int index = 1; 
			foreach (var block in excelData)
			{
				HandleRow(block, index);
				index++;
			}
		}

		protected virtual void HandleRow(KeyValuePair<string, string[]> blockRow, int index)
		{
			string[] blockValues = blockRow.Value;
			T block = GetBlockFromRow(blockValues);

			try
			{
				if (block == null)
				{
					try
					{
						CreateContent(blockValues);
					}
					catch
					{
						Errors.Add(string.Format(ERROR_FORMAT_STRING, CREATE_ERROR_KEY, "row " + index + " in the excel doc."));
						Failed++;
					}
				}
				else
				{
					try
					{
						UpdateContent(block as IContent, blockValues);
					}
					catch
					{
						Errors.Add(string.Format(ERROR_FORMAT_STRING, UPDATE_ERROR_KEY, blockValues[_idColumnIndex]));
						Failed++;
					}
				}
			}
			catch (ContentNotFoundException)
			{
				Errors.Add(string.Format("Update failed for block ID: {0} A block with this ID does not exist", blockValues[_idColumnIndex]));
			}
		}

		protected abstract T GetBlockFromRow(string[] blockValues);
	}
}
