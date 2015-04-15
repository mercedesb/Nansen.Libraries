using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace BlueBuffalo.Core.ContentTransfer
{
	public abstract class PageXMLImporter<T> : IContentImporter where T : PageData
	{
		protected abstract string ERROR_FORMAT_STRING { get; }

		public List<string> Errors { get; protected set; }
		public int Created { get; protected set; }
		public int Updated { get; protected set; }
		public int Failed { get; protected set; }

		protected IEnumerable<T> _pages;
		protected IContentRepository _contentRepository;

		public PageXMLImporter(List<string> errors)
		{
			Errors = errors;
			_contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

			Created = 0;
			Updated = 0;
			Failed = 0;
		}

		public virtual void Run(Stream fileStream)
		{
			try
			{
				XmlDocument xDocument = new XmlDocument();

				using (fileStream)
				{
					fileStream.Position = 0;
					xDocument.Load(fileStream);
					
				}
				XmlNode root = xDocument.SelectSingleNode("*");
				ReadXML(root, ContentReference.RootPage);
				
			}
			catch(XmlException ex)
			{
				Errors.Add("Malformed xml");
			}
		}

		protected abstract void ReadXML(XmlNode node, ContentReference parentLink);
		protected abstract T GetPageFromNode(XmlNode node);
		protected abstract ContentReference CreatePage(XmlNode node, ContentReference parentLink);
		protected abstract ContentReference UpdatePage(T page, XmlNode node);

		protected virtual string GetPathFromNode(XmlNode node)
		{
			if (node is XmlElement && node.ParentNode != null)
				    return string.Format("{0}/{1}", GetPathFromNode(node.ParentNode), node.Name);
			else
				return string.Empty;
		}
	}
}
