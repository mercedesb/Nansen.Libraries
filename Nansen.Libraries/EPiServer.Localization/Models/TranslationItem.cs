using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace EPiServer.Localization.Models
{
	/// <summary>
	///     The translation PageType.
	/// </summary>
	[ContentType(GUID = "A691F851-6C6E-4C06-B62E-8FBC5A038A68", AvailableInEditMode = true,
		Description = "Translation", DisplayName = "Translation", GroupName = "Localization")]
	[AvailableContentTypes(Exclude = new[] { typeof(PageData) })]
	public class TranslationItem : BaseLocalizationPage
	{
		private const string NAME_ATTRIBUTE = "name";
		private const string DESC_ATTRIBUTE = "description";

		#region Public Properties

		/// <summary>
		///     Gets or sets the original text.
		/// </summary>
		/// <remarks>
		///     To reference this use the text without spaces and special characters, all lower case.
		///     So if the OriginalText is e.g. Text One, the key in the xml will be textone.
		///     Note that this is only for normal translations. For translations beneath a
		///     <see
		///         cref="CategoryTranslationContainer" />
		///     the OriginalText text will be used, as the generated xml is different for Category translations.
		///     You can display the translated category name on a page by using the LocalizedDescription property of the Category.
		/// </remarks>
		[Display(GroupName = SystemTabNames.Content, Description = "The text to translate.", Name = "XML node name or Original text", Order = 10)]
		[Required(AllowEmptyStrings = false)]
		public override string OriginalText { get; set; }

		/// <summary>
		///     Gets or sets the translation.
		/// </summary>
		[Display(GroupName = SystemTabNames.Content, Description = "The translation of the original text.",
			Name = "Translation", Order = 20)]
		[CultureSpecific]
		[Required(AllowEmptyStrings = false)]
		public virtual string Translation { get; set; }

		/// <summary>
		///     Gets the translated values for this item.
		/// </summary>
		public Dictionary<string, string> TranslatedValues
		{
			get
			{
				IEnumerable<PageData> allLanguages;
				if (this.PageLink.ID != 0)
					allLanguages = ContentRepo.GetLanguageBranches<PageData>(this.PageLink);
				else
					allLanguages = Enumerable.Empty<PageData>(); //not published yet

				return
					new Dictionary<string, string>(
						allLanguages.ToDictionary(
							language => new CultureInfo(language.LanguageID).NativeName,
							language => ((TranslationItem)language).Translation));
			}
		}

		/// <summary>
		///     Gets the missing translations for this item.
		/// </summary>
		public ReadOnlyCollection<string> MissingValues
		{
			get
			{
				IEnumerable<CultureInfo> availableLanguages =
					ContentRepo.GetLanguageBranches<PageData>(ContentReference.StartPage)
							   .Select(pageData => pageData.Language)
							   .ToList();

				IEnumerable<PageData> allLanguages;
				if (this.PageLink.ID != 0)
				{
					allLanguages = ContentRepo.GetLanguageBranches<PageData>(this.PageLink);
				}
				else //page has not been published yet
				{
					allLanguages = Enumerable.Empty<PageData>();
				}

				return new ReadOnlyCollection<string>(
					(from availableLanguage in availableLanguages
					 where allLanguages.FirstOrDefault(p => p.Language.Equals(availableLanguage)) == null
					 select availableLanguage.NativeName).ToList());
			}
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// Sets the default property values on the page data.
		/// </summary>
		/// <param name="contentType">
		/// The type of content.
		/// </param>
		public override void SetDefaultValues(ContentType contentType)
		{
			base.SetDefaultValues(contentType);

			this.VisibleInMenu = false;
		}

		public override void SetPageData(System.Xml.XmlNode node)
		{
			string name = string.Empty;
			if (node.Attributes[NAME_ATTRIBUTE] != null && !string.IsNullOrEmpty(node.Attributes[NAME_ATTRIBUTE].InnerText))
				name = node.Attributes[NAME_ATTRIBUTE].InnerText;
			else
				name = node.Name;

			this.Name = name;
			this.OriginalText = name;

			string translation = string.Empty;
			if (!string.IsNullOrEmpty(node.InnerText))
				translation = node.InnerText;
			else if (node.Attributes[DESC_ATTRIBUTE] != null && !string.IsNullOrEmpty(node.Attributes[DESC_ATTRIBUTE].InnerText))
				translation = node.Attributes[DESC_ATTRIBUTE].InnerText;

			this.Translation = translation;
		}

		#endregion
	}
}
