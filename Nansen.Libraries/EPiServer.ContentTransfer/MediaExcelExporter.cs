using EPiServer.ContentTransfer.Utility;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Data;

namespace BlueBuffalo.Core.ContentTransfer
{
	public abstract class MediaExcelExporter<T> : IContentExporter where T : MediaData
	{
		public List<string> Errors { get; protected set; }
		protected System.Web.HttpResponse _response { get; set; }

		protected IEnumerable<ContentReference> _containers { get; set; }

		public MediaExcelExporter(List<string> errors, IEnumerable<ContentReference> containers, System.Web.HttpResponse response)
		{
			Errors = errors;
			_containers = containers;
			_response = response;
		}

		public virtual void CreateFile()
		{
			IEnumerable<T> mediaFiles = GetMediaData();
			
			try
			{
				ExcelWriter.CreateExcelDocument(GetMediaDataTable(mediaFiles), string.Format("{0}s-{1:yyyy-MM-dd}.xlsx", typeof(T).Name, DateTime.Now), _response);
			}
			catch (Exception ex)
			{
				Errors.Add(string.Format("Oops, something went wrong.  Please screenshot the following error and send to Nansen. <br/> {0} <br/> {1}", ex.Message, ex.StackTrace));
			}
		}

		protected abstract IEnumerable<T> GetMediaData();
		
		protected abstract DataTable GetMediaDataTable(IEnumerable<T> mediaFiles);

		protected virtual string GetValue<V>(V value)
		{
			if (value == null)
				return string.Empty;

			return value.ToString();
		}
	}
}
