using EPiServer.ContentTransfer.Utility;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Data;

namespace EPiServer.ContentTransfer
{
	public abstract class MediaExcelExporter<T> : BaseContentExcelExporter<T> where T : MediaData
	{
		protected IEnumerable<ContentReference> _containers { get; set; }

		public MediaExcelExporter(List<string> errors, IEnumerable<ContentReference> containers, System.Web.HttpResponse response)
		 : base (errors, response)
		{
			_containers = containers;
		}
	}
}
