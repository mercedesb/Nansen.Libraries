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
	public abstract class BlockExcelExporter<T> : BaseContentExcelExporter<T> where T : BlockData
	{
		public List<string> Errors { get; protected set; }
		protected System.Web.HttpResponse _response { get; set; }

		public BlockExcelExporter(List<string> errors, System.Web.HttpResponse response)
			: base(errors, response) { }

		public override void CreateFile()
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

			ExcelWriter.CreateExcelDocument(GetContentDataTable(blocks), string.Format("{0}s-{1:yyyy-MM-dd}.xlsx", typeof(T).Name, DateTime.Now), _response);
		}
	}
}
