using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples
{
	public class Constants
	{
		public const string ZIP_CODE_REGEX = @"(^\d{5}(-\d{4})?$)|(^[ABCEGHJKLMNPRSTVXY]{1}\d{1}[A-Z]{1}\s*\d{1}[A-Z]{1}\d{1}$)";
	}

	public class Tabs
	{
		public const string MetaData = "MetaData";
	}

	public enum EventDetailsPageRows
	{
		[Description("EPiServer Id")]
		Id = 0,

		[Description("Name")]
		Name,

		[Description("Title")]
		Title,

		[Description("Preamble")]
		Preamble,

		[Description("Main body")]
		MainBody,

		[Description("Main image Id")]
		MainImageID,

		[Description("Event Category")]
		EventCategory,

		[Description("Start Date")]
		StartDate,

		[Description("Start Time")]
		StartTime,

		[Description("End Date")]
		EndDate,

		[Description("End Time")]
		EndTime,

		[Description("Time Zone")]
		Timezone,

		[Description("Location Name")]
		LocationName,

		[Description("Location address")]
		Address,

		[Description("Location city")]
		City,

		[Description("Location state (abbrev)")]
		State,

		[Description("Location zip")]
		Zip,

		[Description("Related Events Ids")]
		RelatedEvents,

		[Description("Start publish date")]
		StartPublishDate,

		[Description("Stop publish date")]
		StopPublishDate,

		[Description("Meta NoIndex")]
		MetaNoIndex,

		[Description("Meta NoFollow")]
		MetaNoFollow,

		[Description("Meta Title")]
		MetaTitle,

		[Description("Meta Keywords")]
		MetaKeywords,

		[Description("Meta Description")]
		MetaDescription,

		[Description("Meta Author")]
		MetaAuthor,

		[Description("Force Publish?")]
		ForcePublish
	}

	public enum BackgroundBlockRows
	{
		[Description("Block ID")]
		BlockID = 0,

		[Description("Name")]
		Name,

		[Description("Main body")]
		MainBody,

		[Description("Background Image ID")]
		BackgroundImageID,

		[Description("Background image mobile ID")]
		BackgroundMobileImageID,

		[Description("Foreground image ID")]
		ForegroundImageID,

		[Description("Position foreground image left")]
		ForegroundImageLeft,

		[Description("Foreground image top offset")]
		ForegroundImageTopOffset,

		[Description("Foreground image bottom offset")]
		ForegroundImageBottomOffset,

		[Description("Foreground image offset from edge")]
		ForegroundImageEdgeOffset
	}
}
