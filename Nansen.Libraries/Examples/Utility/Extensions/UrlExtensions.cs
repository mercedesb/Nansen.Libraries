using EPiServer;
using EPiServer.Core;
using EPiServer.Web;

namespace Examples.Utility.Extensions
{
	public static class UrlExtensions
	{
		public static ContentReference GetContentReference(this Url url)
		{
			if (url != null)
				return PermanentLinkUtility.GetContentReference(new UrlBuilder(url));

			return null;
		}
	}
}
