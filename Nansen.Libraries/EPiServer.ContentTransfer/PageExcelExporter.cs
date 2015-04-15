using EPiServer;
using EPiServer.ContentTransfer.Utility;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BlueBuffalo.Core.ContentTransfer
{
	public abstract class PageExcelExporter<T> : IContentExporter where T: PageData
	{
		public List<string> Errors { get; protected set; }
		protected System.Web.HttpResponse _response { get; set; }

		protected ContentReference _container { get; set; }

		protected IContentRepository _contentRepository;

		public PageExcelExporter(List<string> errors, ContentReference container, System.Web.HttpResponse response)
		{
			_contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

			Errors = errors;
			_container = container;
			_response = response;
		}

		public virtual void CreateFile()
		{
			try
			{
				IEnumerable<T> pages = _contentRepository.GetDescendents(_container).Select(cr => _contentRepository.Get<T>(cr));
				ExcelWriter.CreateExcelDocument(GetPageDataTable(pages), string.Format("{0}s-{1:yyyy-MM-dd}.xlsx", typeof(T).Name, DateTime.Now), _response);
			}
			catch (Exception ex)
			{
				Errors.Add(string.Format("Oops, something went wrong.  Please screenshot the following error and send to Nansen. <br/> {0} <br/> {1}", ex.Message, ex.StackTrace));
			}
		}

		protected abstract DataTable GetPageDataTable(IEnumerable<T> pages);

		protected virtual string GetContentIdsString(ContentArea contentArea)
		{
			try
			{
				if (contentArea == null || contentArea.Items == null)
					return string.Empty;

				var blockIds = contentArea.Items.Select(item => GetIDValue(item.ContentLink));
				return String.Join(",", blockIds);
			}
			catch (Exception ex)
			{
				Errors.Add(string.Format("Oops, something went wrong.  Please screenshot the following error and send to Nansen. <br/> {0} <br/> {1}", ex.Message, ex.StackTrace));
				return string.Empty;
			}
		}

		protected virtual string GetIDValue(ContentReference contentRef)
		{
			if (contentRef != null)
				return GetValue(contentRef.ID);

			return string.Empty;
		}

		protected virtual string GetValue<V>(V value)
		{
			if (value == null)
				return string.Empty;

			return value.ToString();
		}

		protected virtual string GetCategoriesString(string categoryIds)
		{
			string[] categoryIdsArr = categoryIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			string[] categoryNamesArr = new string[categoryIdsArr.Length];
			for (int i = 0; i < categoryIdsArr.Length; i++)
			{
				int categoryId;
				if (int.TryParse(categoryIdsArr[i], out categoryId))
					categoryNamesArr[i] = Category.GetRoot().FindChild(categoryId).Name;
			}
			return string.Join(",", categoryNamesArr);
		}
	}
}
