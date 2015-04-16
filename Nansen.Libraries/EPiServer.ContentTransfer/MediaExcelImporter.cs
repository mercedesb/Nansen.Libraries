using EPiServer;
using EPiServer.ContentTransfer.Utility;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.IO;

namespace EPiServer.ContentTransfer
{
	public abstract class MediaExcelImporter<T, TEnum> : IContentImporter
		where T : MediaData
		where TEnum : struct
	{
		public List<string> Errors { get; protected set; }
		public int Created { get; protected set; }
		public int Updated { get; protected set; }
		public int Failed { get; protected set; }

		protected IContentRepository _contentRepository;

		public MediaExcelImporter(List<string> errors)
		{
			Errors = errors;
			_contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
			Created = 0;
			Updated = 0;
			Failed = 0;
		}

		public virtual void Run(Stream fileStream)
		{
			if (!CheckExcelHeaders(fileStream))
			{
				Errors.Add("Excel doc headers does not match what's expected.");
			}
			else
			{
				HandleRowsFromExcel(fileStream);
			}
		}

		protected virtual void HandleRowsFromExcel(Stream fileStream)
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

			foreach (var media in excelData)
			{
				HandleRow(media);
			}
		}

		protected virtual void HandleRow(KeyValuePair<string, string[]> mediaRow)
		{
			string[] mediaValues = mediaRow.Value;
			T media = GetMediaFromRow(mediaValues);

			try
			{
				if (media != null)
				{
					UpdateMedia(media, mediaValues);
				}
				else
				{
					Failed++;
					Errors.Add(string.Format("Update failed for media ID: {0} A media item with this ID does not exist", mediaValues[0]));
				}
			}
			catch (ContentNotFoundException cnfex)
			{
				Errors.Add(string.Format("Update failed for media ID: {0} A media item with this ID does not exist", mediaValues[0]));
			}
		}

		protected abstract T GetMediaFromRow(string[] mediaValues);
		protected abstract void UpdateMedia(T media, string[] mediaValues);
		protected abstract void SetMediaData(T media, string[] mediaValues);

		protected virtual V SetProperty<V>(V excelValue, V defaultValue)
		{
			if (excelValue.ToString() == string.Empty)
				return defaultValue;
			else
				return excelValue;
		}

		protected abstract bool CheckExcelHeaders(Stream fileStream);
	}
}
