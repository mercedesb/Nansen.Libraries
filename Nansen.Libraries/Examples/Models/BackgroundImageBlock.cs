using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using System.ComponentModel.DataAnnotations;

namespace Examples.Models
{
	[ContentType(GUID = "FA183632-A914-4159-AA90-29193BBA3D70", DisplayName = "Background Image block")]
	public class BackgroundImageBlock : BlockData
	{
		[CultureSpecific]
		[Display(Name = "Main body", Order = 100)]
		public virtual XhtmlString MainBody { get; set; }

		[CultureSpecific]
		[Display(Name = "Background image", Order = 150)]
		[UIHint(UIHint.Image)]
		public virtual Url BackgroundImage { get; set; }

		[CultureSpecific]
		[Display(Name = "Background image mobile", Order = 160)]
		[UIHint(UIHint.Image)]
		public virtual Url BackgroundImageMobile { get; set; }

		[CultureSpecific]
		[Display(Name = "Mobile image link", Order = 160)]
		public virtual Url MobileImageLink { get; set; }

		[CultureSpecific]
		[Display(Name = "Foreground image", Order = 170)]
		[UIHint(UIHint.Image)]
		public virtual Url ForegroundImage { get; set; }

		[CultureSpecific]
		[Display(Name = "Position foreground image left", Order = 171)]
		public virtual bool PositionForegroundImageLeft { get; set; }

		[CultureSpecific]
		[Display(Name = "Foreground image top offset", Order = 172)]
		public virtual string ForegroundImageTopOffset { get; set; }

		[CultureSpecific]
		[Display(Name = "Foreground image bottom offset", Order = 173)]
		public virtual string ForegroundImageBottomOffset { get; set; }

		[CultureSpecific]
		[Display(Name = "Foreground image offset from edge", Order = 174)]
		public virtual string ForegroundImageEdgeOffset { get; set; }
	}
}
