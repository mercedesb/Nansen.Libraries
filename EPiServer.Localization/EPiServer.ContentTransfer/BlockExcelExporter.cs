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
	public abstract class BlockExcelExporter<T> : IContentExporter where T : BlockData
	{
		public List<string> Errors { get; protected set; }
		protected System.Web.HttpResponse _response { get; set; }

		public BlockExcelExporter(List<string> errors, System.Web.HttpResponse response)
		{
			Errors = errors;
			_response = response;
		}

		public virtual void CreateFile()
		{
			var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
			var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
			var contentModelUsage = ServiceLocator.Current.GetInstance<IContentModelUsage>();

			// loading a block type
			var blockType = contentTypeRepository.Load<T>();

			// MB: get all distinct usages (most recent version) of that block type that are not in the trash
			IEnumerable<T> blocks = contentModelUsage.ListContentOfContentType(blockType)
														.Select(u => u.ContentLink.ToReferenceWithoutVersion())
														.Distinct()
														.Select(b => contentRepository.Get<T>(b))
														.Where(b => (b as IContent).ParentLink != ContentReference.WasteBasket);

			ExcelWriter.CreateExcelDocument(GetBlockDataTable(blocks), string.Format("{0}s-{1:yyyy-MM-dd}.xlsx", typeof(T).Name, DateTime.Now), _response);
		}

		protected abstract DataTable GetBlockDataTable(IEnumerable<T> blocks);

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
	}
}
