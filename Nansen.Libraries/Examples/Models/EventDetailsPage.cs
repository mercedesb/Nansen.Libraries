using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using System;
using System.ComponentModel.DataAnnotations;

namespace Examples.Models
{
	[ContentType(GUID = "9B6744AB-4180-4D61-A3D6-1B794C11B402", DisplayName = "Event details page")]
	public class EventDetailsPage : PageData
	{
		[Display(Order = 1)]
		public virtual string Title
		{
			get
			{
				return this.GetPropertyValue(p => p.Title, this.Name);
			}
			set
			{
				this.SetPropertyValue(p => p.Title, value);
			}
		}

		[Display(Name = "Preamble", Description = "Preamble for page", Order = 2)]
		[UIHint(UIHint.Textarea, PresentationLayer.Edit)]
		public virtual string Preamble { get; set; }

		[Display(Name = "Main body", Description = "Main body for page", Order = 3)]
		public virtual XhtmlString MainBody { get; set; }

		[Display(Name = "Main image", Description = "Main image", Order = 4)]
		[UIHint(UIHint.Image)]
		public virtual Url MainImage { get; set; }

		[Display(Name = "Is Searchable", Order = 5)]
		public virtual bool IsSearchable
		{
			get;
			set;
		}
		public override void SetDefaultValues(EPiServer.DataAbstraction.ContentType contentType)
		{
			base.SetDefaultValues(contentType);
			this.IsSearchable = true;
		}

		#region MetaData
		[Display(GroupName = Tabs.MetaData, Order = 410, Name = "Meta NoIndex")]
		public virtual bool MetaNoIndex { get; set; }

		[Display(GroupName = Tabs.MetaData, Order = 420, Name = "Meta NoFollow")]
		public virtual bool MetaNoFollow { get; set; }

		[Display(GroupName = Tabs.MetaData, Order = 425, Name = "Meta Title")]
		[CultureSpecific]
		public virtual string MetaTitle { get; set; }

		[Display(GroupName = Tabs.MetaData, Order = 430, Name = "Meta Keywords")]
		[UIHint(UIHint.Textarea)]
		[CultureSpecific]
		public virtual string MetaKeywords { get; set; }

		[Display(GroupName = Tabs.MetaData, Order = 440, Name = "Meta Description")]
		[UIHint(UIHint.Textarea)]
		[CultureSpecific]
		public virtual string MetaDescription { get; set; }

		[Display(GroupName = Tabs.MetaData, Order = 460, Name = "Meta Author")]
		[CultureSpecific]
		public virtual string MetaAuthor { get; set; }
		#endregion
		
		[Display(Name = "Event Category", Order = 8)]
		public virtual CategoryList EventCategory { get; set; }
		
		[Display(Name = "Start Date and Time", Order = 10)]
		[Required]
		public virtual DateTime Start { get; set; }

		[Display(Name = "End Date and Time", Order = 20)]
		public virtual DateTime End { get; set; }

		[Display(Name = "Time Zone", Order = 25)]
		public virtual string TimeZone { get; set; }

		[CultureSpecific]
		[Display(Name = "Location Name", Order = 40)]
		public virtual string LocationName { get; set; }

		[Display(Name = "Location address", Order = 50)]
		[Required]
		public virtual string Address { get; set; }

		[Display(Name = "Location city", Order = 60)]
		[Required]
		public virtual string City { get; set; }

		[Display(Name = "Location state (abbrev)", Order = 70)]
		[Required]
		public virtual string State { get; set; }

		[Display(Name = "Location zip", Order = 80)]
		[RegularExpression(Examples.Constants.ZIP_CODE_REGEX)]
		public virtual string Zip { get; set; }

		[Display(Name = "Latitude (to be calculated)", Order = 90)]
		public virtual double Latitude { get; set; }

		[Display(Name = "Longitude (to be calculated)", Order = 100)]
		public virtual double Longitude { get; set; }

		[Display(Name = "Related Events", Order = 110)]
		[AllowedTypes(typeof(EventDetailsPage))]
		public virtual ContentArea RelatedEvents { get; set; }
	}
}
