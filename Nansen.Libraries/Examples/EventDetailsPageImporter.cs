using EPiServer;
using EPiServer.ContentTransfer;
using EPiServer.ContentTransfer.Utility;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Examples.Models;
using Examples.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Examples
{
	public class EventDetailsPageImporter : PageExcelImporter<EventDetailsPage, EventDetailsPageRows>
	{
		protected override string CATEGORY_ERROR_KEY { get { return "Category failed"; } }
		protected override string CREATE_ERROR_KEY { get { return "Create failed"; } }
		protected override string UPDATE_ERROR_KEY { get { return "Update failed"; } }
		protected const string CREATE_ERROR_FORMAT_STRING = "{0}: Name {1}";
		protected override string ERROR_FORMAT_STRING { get { return "{0}: EPiServer Id {1}"; } }

		public EventDetailsPageImporter(List<string> errors, int containerRefId, string language, bool overwrite = false)
			: base(errors, containerRefId, language, 1, overwrite)
		{ }

		protected override EventDetailsPage GetPageFromRow(string[] pageValues)
		{
			return _pages.FirstOrDefault(p => p.ContentLink.ID.ToString() == pageValues[(int)EventDetailsPageRows.Id]);
		}

		protected override void HandleUpdate(EventDetailsPage page, string[] pageValues)
		{
			SetContentData(page, pageValues);
			if (pageValues.Length > (int)EventDetailsPageRows.ForcePublish
				&& !string.IsNullOrWhiteSpace(pageValues[(int)EventDetailsPageRows.ForcePublish])
				&& pageValues[(int)EventDetailsPageRows.ForcePublish].ToLowerInvariant() == bool.TrueString.ToLowerInvariant())
				_contentRepository.Save(page, SaveAction.Publish);
			else
				_contentRepository.Save(page, SaveAction.CheckIn);
		}

		protected override void SetContentName(EventDetailsPage page, string[] pageValues)
		{
			page.Name = SetProperty(pageValues[(int)EventDetailsPageRows.Name], page.Name);
		}

		protected override void SetContentData(EventDetailsPage page, string[] pageValues)
		{
			try
			{
				//Set Global properties
				if (page.Language.TwoLetterISOLanguageName == Settings.MASTER_LANGUAGE)
					SetGlobalProperties(page, pageValues); 
				
				page.Name = SetProperty(pageValues[(int)EventDetailsPageRows.Name], page.Name);
				page.Title = SetProperty(pageValues[(int)EventDetailsPageRows.Title], page.Title);
				page.Preamble = SetProperty(pageValues[(int)EventDetailsPageRows.Preamble], page.Preamble);
				page.MainBody = SetProperty(new XhtmlString(pageValues[(int)EventDetailsPageRows.MainBody]), page.MainBody);
				page.MainImage = SetProperty(GetImageUrl(pageValues[(int)EventDetailsPageRows.MainImageID]), page.MainImage);

				page.LocationName = SetProperty(pageValues[(int)EventDetailsPageRows.LocationName], page.LocationName);

				if (!string.IsNullOrWhiteSpace(pageValues[(int)EventDetailsPageRows.StartPublishDate]))
					page.StartPublish = SetProperty(DateTime.FromOADate(Double.Parse(pageValues[(int)EventDetailsPageRows.StartPublishDate])), page.StartPublish);
				if (!string.IsNullOrWhiteSpace(pageValues[(int)EventDetailsPageRows.StopPublishDate]))
					page.StopPublish = SetProperty(DateTime.FromOADate(Double.Parse(pageValues[(int)EventDetailsPageRows.StopPublishDate])), page.StopPublish);

				page.MetaNoIndex = SetProperty(pageValues[(int)EventDetailsPageRows.MetaNoIndex].ToLowerInvariant() == bool.TrueString.ToLowerInvariant(), page.MetaNoIndex);
				page.MetaNoFollow = SetProperty(pageValues[(int)EventDetailsPageRows.MetaNoFollow].ToLowerInvariant() == bool.TrueString.ToLowerInvariant(), page.MetaNoFollow);
				page.MetaTitle = SetProperty(pageValues[(int)EventDetailsPageRows.MetaTitle], page.MetaTitle);
				page.MetaKeywords = SetProperty(pageValues[(int)EventDetailsPageRows.MetaKeywords], page.MetaKeywords);
				page.MetaDescription = SetProperty(pageValues[(int)EventDetailsPageRows.MetaDescription], page.MetaDescription);
				page.MetaAuthor = SetProperty(pageValues[(int)EventDetailsPageRows.MetaAuthor], page.MetaAuthor);
			}
			catch (IndexOutOfRangeException)
			{
				//Trailing columns of row were left blank by user
				//no empty strings were inserted by the code
				//Should fail silently!!
			}
		}

		protected override void SetGlobalProperties(EventDetailsPage page, string[] pageValues)
		{
			try
			{
				page.EventCategory = SetProperty(GetCategories(pageValues[(int)EventDetailsPageRows.EventCategory], pageValues[(int)EventDetailsPageRows.Id]), page.EventCategory);
				if (!string.IsNullOrWhiteSpace(pageValues[(int)EventDetailsPageRows.StartDate]))
					page.Start = SetProperty(DateTime.FromOADate(Double.Parse(pageValues[(int)EventDetailsPageRows.StartDate])), page.Start);
				if (!string.IsNullOrWhiteSpace(pageValues[(int)EventDetailsPageRows.StartTime]) && page.Start != DateTime.MinValue)
				{
					DateTime startTime = DateTime.FromOADate(Double.Parse(pageValues[(int)EventDetailsPageRows.StartTime]));
					page.Start = new DateTime(page.Start.Year, page.Start.Month, page.Start.Day, startTime.Hour, startTime.Minute, 0);
				}
				if (!string.IsNullOrWhiteSpace(pageValues[(int)EventDetailsPageRows.EndDate]))
					page.End = SetProperty(DateTime.FromOADate(Double.Parse(pageValues[(int)EventDetailsPageRows.EndDate])), page.End);
				if (!string.IsNullOrWhiteSpace(pageValues[(int)EventDetailsPageRows.EndTime]))
				{
					DateTime endTime = DateTime.FromOADate(Double.Parse(pageValues[(int)EventDetailsPageRows.EndTime]));
					page.End = new DateTime(page.End.Year, page.End.Month, page.End.Day, endTime.Hour, endTime.Minute, 0);
				}

				page.TimeZone = SetProperty(pageValues[(int)EventDetailsPageRows.Timezone], page.TimeZone);

				SetRelatedEvents(pageValues[(int)EventDetailsPageRows.RelatedEvents], page);

				page.Address = SetProperty(pageValues[(int)EventDetailsPageRows.Address], page.Address);
				page.City = SetProperty(pageValues[(int)EventDetailsPageRows.City], page.City);
				page.State = SetProperty(pageValues[(int)EventDetailsPageRows.State], page.State);
				page.Zip = SetProperty(pageValues[(int)EventDetailsPageRows.Zip], page.Zip);

				page.VisibleInMenu = true;
				page.IsSearchable = true;
			}
			catch (IndexOutOfRangeException)
			{
				//Trailing columns of row were left blank by user
				//no empty strings were inserted by the code
				//Should fail silently!!
			}
		}

		protected override bool CheckExcelHeaders(Stream fileStream)
		{
			var firstRow = ExcelUtility.GetExcelFirstRow(fileStream);
			Array values = Enum.GetValues(typeof(EventDetailsPageRows));

			for (int i = 0; i < values.Length; i++)
			{
				if (firstRow[i] != ((EventDetailsPageRows)values.GetValue(i)).GetDescriptionString())
					return false;
			}
			return true;
		}

		protected virtual void SetRelatedEvents(string relatedEventsIds, EventDetailsPage page)
		{
			string[] epiIds = relatedEventsIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			var pages = epiIds.Select(id => _pages.FirstOrDefault(p => p.ContentLink.ID.ToString() == id)).Where(p => p != null);
			IEnumerable<string> ids = pages.Select(p => p.ContentLink.ID.ToString());

			var missingPages = epiIds.Except(pages.Select(p => p.ContentLink.ID.ToString()));
			foreach (var missing in missingPages)
			{
				Errors.Add(string.Format("Could not add related event.  There is no page with EPiServer Id: {0}", missing));
			}

			// If no id's added, escape
			if (!ids.Any()) return;

			//If BottomContent is empty, set prop to new writable clode
			if (page.RelatedEvents == null)
			{
				page.RelatedEvents = new ContentArea().CreateWritableClone();
			}

			SetContentAreaBlocks(ids, page.RelatedEvents, Errors);
		}

		protected virtual Url GetImageUrl(string imageId)
		{
			if (!string.IsNullOrWhiteSpace(imageId))
			{
				var contentRef = new ContentReference(int.Parse(imageId));
				return ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(contentRef);
			}
			return string.Empty;
		}
	}
}
