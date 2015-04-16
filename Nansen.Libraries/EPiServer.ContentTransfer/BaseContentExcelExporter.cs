using EPiServer.ContentTransfer.Utility;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Data;

namespace EPiServer.ContentTransfer
{
	public abstract class BaseContentExcelExporter<T> : IContentExporter where T : ContentData
	{
		public List<string> Errors { get; protected set; }
		protected System.Web.HttpResponse _response { get; set; }

		public BaseContentExcelExporter(List<string> errors, System.Web.HttpResponse response)
		{
			Errors = errors;
			_response = response;
		}

		public virtual void CreateFile()
		{
			IEnumerable<T> content = GetContentData();

			try
			{
				ExcelWriter.CreateExcelDocument(GetContentDataTable(content), string.Format("{0}s-{1:yyyy-MM-dd}.xlsx", typeof(T).Name, DateTime.Now), _response);
			}
			catch (Exception ex)
			{
				Errors.Add(string.Format("Oops, something went wrong.  Please screenshot the following error and send to Nansen. <br/> {0} <br/> {1}", ex.Message, ex.StackTrace));
			}
		}

		protected abstract IEnumerable<T> GetContentData();
		protected abstract DataTable GetContentDataTable(IEnumerable<T> content);

		protected virtual string GetValue<V>(V value)
		{
			if (value == null)
				return string.Empty;

			return value.ToString();
		}

		protected virtual string GetIDValue(ContentReference contentRef)
		{
			if (contentRef != null)
				return GetValue(contentRef.ID);

			return string.Empty;
		}
	}
}
