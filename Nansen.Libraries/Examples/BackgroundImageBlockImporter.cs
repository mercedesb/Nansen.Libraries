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

namespace Examples
{
	public class BackgroundImageBlockImporter : BlockExcelImporter<BackgroundImageBlock, BackgroundBlockRows>
	{
		protected override string CATEGORY_ERROR_KEY { get { return "Category failed"; } }
		protected override string CREATE_ERROR_KEY { get { return "Create failed"; } }
		protected override string UPDATE_ERROR_KEY { get { return "Update failed"; } }
		protected override string ERROR_FORMAT_STRING { get { return "{0} for {1}"; } }

		public BackgroundImageBlockImporter(List<string> errors, int containerRefId, string language = "", bool overwrite = false)
			: base(errors, containerRefId, language, 0, overwrite) 
		{ }

		protected override BackgroundImageBlock GetBlockFromRow(string[] blockValues)
		{
			string blockID = blockValues[(int)BackgroundBlockRows.BlockID];
			if (string.IsNullOrWhiteSpace(blockID))
				return null;

			var contentRef = new ContentReference(int.Parse(blockID));
			return _contentRepository.Get<BackgroundImageBlock>(contentRef);
		}

        protected override void HandleUpdate(BackgroundImageBlock clone, string[] blockValues)
        {
            SetContentData(clone, blockValues);
            _contentRepository.Save(clone as IContent, SaveAction.Publish);
        }

        protected override void SetContentName(BackgroundImageBlock content, string[] contentValues)
        {
            var blockContent = content as IContent;
            blockContent.Name = SetProperty(contentValues[(int)BackgroundBlockRows.Name], blockContent.Name);
        }
        
        protected override void SetContentData(BackgroundImageBlock block, string[] blockValues)
		{
			try
			{
				var blockContent = block as IContent;
				blockContent.Name = SetProperty(blockValues[(int)BackgroundBlockRows.Name], blockContent.Name);
				block.MainBody = SetProperty(new XhtmlString(blockValues[(int)BackgroundBlockRows.MainBody]), block.MainBody);
				block.BackgroundImage = SetProperty(GetImageUrl(blockValues[(int)BackgroundBlockRows.BackgroundImageID]), block.BackgroundImage);
				block.BackgroundImageMobile = SetProperty(GetImageUrl(blockValues[(int)BackgroundBlockRows.BackgroundMobileImageID]), block.BackgroundImageMobile);
				block.ForegroundImage = SetProperty(GetImageUrl(blockValues[(int)BackgroundBlockRows.ForegroundImageID]), block.ForegroundImage);
				block.PositionForegroundImageLeft = SetProperty(blockValues[(int)BackgroundBlockRows.ForegroundImageLeft].ToLowerInvariant() == bool.TrueString.ToLowerInvariant(), block.PositionForegroundImageLeft);
				block.ForegroundImageTopOffset = SetProperty(blockValues[(int)BackgroundBlockRows.ForegroundImageTopOffset], block.ForegroundImageTopOffset);
				block.ForegroundImageBottomOffset = SetProperty(blockValues[(int)BackgroundBlockRows.ForegroundImageBottomOffset], block.ForegroundImageBottomOffset);
				block.ForegroundImageEdgeOffset = SetProperty(blockValues[(int)BackgroundBlockRows.ForegroundImageEdgeOffset], block.ForegroundImageEdgeOffset);
			}
			catch (IndexOutOfRangeException)
			{
				//Trailing columns of row were left blank by user
				//no empty strings were inserted by the code
				//Should fail silently!!
			}
		}

        protected override void SetGlobalProperties(BackgroundImageBlock block, string[] blockValues)
        {
        }

		protected override bool CheckExcelHeaders(Stream fileStream)
		{
			var firstRow = ExcelUtility.GetExcelFirstRow(fileStream);
			Array values = Enum.GetValues(typeof(BackgroundBlockRows));

			for (int i = 0; i < values.Length; i++)
			{
				if (firstRow[i] != ((BackgroundBlockRows)values.GetValue(i)).GetDescriptionString())
					return false;
			}
			return true;
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
