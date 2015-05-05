using EPiServer;
using EPiServer.ContentTransfer.Utility;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EPiServer.ContentTransfer
{
	public abstract class PageExcelExporter<T> : BaseContentExcelExporter<T> where T: PageData
	{
		protected ContentReference _container { get; set; }

		protected IContentRepository _contentRepository;
		protected CategoryRepository _categoryRepository;

		public PageExcelExporter(List<string> errors, ContentReference container, System.Web.HttpResponse response)
			: base(errors, response)
		{
			_contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
			_categoryRepository = ServiceLocator.Current.GetInstance<CategoryRepository>();
			_container = container;
		}

		protected override IEnumerable<T> GetContentData()
		{
			return _contentRepository.GetDescendents(_container).Select(cr => _contentRepository.Get<T>(cr));
		}

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

		protected virtual string GetCategoriesString(string categoryIds)
		{
			string[] categoryIdsArr = categoryIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			string[] categoryNamesArr = new string[categoryIdsArr.Length];
			for (int i = 0; i < categoryIdsArr.Length; i++)
			{
				int categoryId;
				if (int.TryParse(categoryIdsArr[i], out categoryId))
					categoryNamesArr[i] = _categoryRepository.GetRoot().FindChild(categoryId).Name;
			}
			return string.Join(",", categoryNamesArr);
		}
	}
}
