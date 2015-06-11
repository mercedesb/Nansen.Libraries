using EPiServer.ContentTransfer;
using EPiServer.Core;
using Examples.Models;
using Examples.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Data;

namespace Examples
{
	public class BackgroundImageBlockExporter : BlockExcelExporter<BackgroundImageBlock>
	{
		public BackgroundImageBlockExporter(List<string> errors, System.Web.HttpResponse response)
			: base(errors, response) { }

		protected override DataTable GetContentDataTable(IEnumerable<BackgroundImageBlock> blocks)
		{
			DataTable dt = new DataTable();
			Array values = Enum.GetValues(typeof(BackgroundBlockRows));
			foreach (BackgroundBlockRows val in values)
			{
				dt.Columns.Add(new DataColumn(val.GetDescriptionString(), typeof(string)));
			}

			foreach (var block in blocks)
			{
				DataRow row = dt.NewRow();
				IContent content = block as IContent;

				row[BackgroundBlockRows.BlockID.GetDescriptionString()] = content.ContentLink.ID.ToString();
				row[BackgroundBlockRows.Name.GetDescriptionString()] = content.Name;
				row[BackgroundBlockRows.MainBody.GetDescriptionString()] = GetValue(block.MainBody);
				row[BackgroundBlockRows.BackgroundImageID.GetDescriptionString()] = GetIDValue(block.BackgroundImage.GetContentReference());
				row[BackgroundBlockRows.BackgroundMobileImageID.GetDescriptionString()] = GetIDValue(block.BackgroundImageMobile.GetContentReference());
				row[BackgroundBlockRows.ForegroundImageID.GetDescriptionString()] = GetIDValue(block.ForegroundImage.GetContentReference());
				row[BackgroundBlockRows.ForegroundImageLeft.GetDescriptionString()] = GetValue(block.PositionForegroundImageLeft);
				row[BackgroundBlockRows.ForegroundImageTopOffset.GetDescriptionString()] = block.ForegroundImageTopOffset;
				row[BackgroundBlockRows.ForegroundImageBottomOffset.GetDescriptionString()] = block.ForegroundImageBottomOffset;
				row[BackgroundBlockRows.ForegroundImageEdgeOffset.GetDescriptionString()] = block.ForegroundImageEdgeOffset;

				dt.Rows.Add(row);
			}

			return dt;
		}
	}
}
