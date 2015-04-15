using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Filters;

namespace EPiServer.Localization.Models
{
	/// <summary>
	///     A container to hold translations for categories.
	/// </summary>
	/// <remarks>
	///     Only the first container will be used in the xml, subs will only be used for ordering.
	/// </remarks>
	[ContentType(GUID = "F95F6943-4ED8-4080-A3C1-D1E903512DB0", AvailableInEditMode = true, Description = "Container to hold translations for categories", DisplayName = "Category translation container", GroupName = "Localization")]
	[AvailableContentTypes(Include = new[] { typeof(TranslationItem), typeof(CategoryTranslationContainer) })]
	public class CategoryTranslationContainer : BaseLocalizationPage
	{

		[Ignore]
		public override string OriginalText
		{
			get
			{
				return Name;
			}
			set
			{
				Name = value;
			}
		}
		
		/// <summary>
		/// Sets the default property values on the page data.
		/// </summary>
		/// <param name="contentType">The type of content.</param>
		/// <example>
		///   <code source="../CodeSamples/EPiServer/Core/PageDataSamples.aspx.cs" region="DefaultValues" />
		/// </example>
		public override void SetDefaultValues(ContentType contentType)
		{
			base.SetDefaultValues(contentType);
			this[MetaDataProperties.PageChildOrderRule] = FilterSortOrder.Alphabetical;
			this.VisibleInMenu = false;
		}

		public override void SetPageData(System.Xml.XmlNode node)
		{
			this.Name = node.Name;
		}
	}
}
