using EPiServer;
using EPiServer.ContentTransfer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Localization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Examples
{
	public class TranslationItemImporter : PageXMLImporter<BaseLocalizationPage>
	{
		protected const string REGEX_REPLACE = @"[^A-Za-z0-9]+"; 
		
		protected const string LANGUAGES_NODE_NAME = "languages";
		protected const string LANGUAGE_NODE_NAME = "language";
		protected const string CATEGORIES_NODE_NAME = "categories";
		protected const string CATEGORY_NODE_NAME = "category";

		protected const string CREATE_ERROR_KEY = "Create failed";
		protected const string UPDATE_ERROR_KEY = "Update failed";
		protected override string ERROR_FORMAT_STRING { get { return "{0} for translation {1}"; } }

		protected virtual TranslationContainer RootTranslationContainer { get; set; }

		protected virtual CategoryTranslationContainer CategoryContainer { get; set; }

		protected virtual string _translationsContainerName;
		protected virtual string _categoryContainerName;

		private string _lang;

		public TranslationItemImporter(List<string> errors, ContentReference translationContainer = null, 
			string translationsContainerName = "Translations", string categoryContainerName = "Category Translations")
			: base(errors)
		{
			if (translationContainer != null)
				RootTranslationContainer = _contentRepository.Get<TranslationContainer>(translationContainer);

			_translationsContainerName = translationsContainerName;
			_categoryContainerName = categoryContainerName;
		}

		protected virtual void SetLanguage(string languageId)
		{
			_lang = languageId;
		}

		protected virtual void SetUpContainers()
		{
			SetInitialTranslationContainer(); //creates translation container if it doesn't exist
			TranslationContainer currTranslationContainer = _contentRepository.Get<TranslationContainer>(RootTranslationContainer.ContentLink, new LanguageSelector(_lang));
			if (currTranslationContainer == null)
				RootTranslationContainer = CreateLanguageBranch<TranslationContainer>(RootTranslationContainer.ContentLink, true);
			else
				RootTranslationContainer = currTranslationContainer;

			SetInitialCategoryContainer();
			CategoryTranslationContainer currCategoryContainer = _contentRepository.Get<CategoryTranslationContainer>(CategoryContainer.ContentLink, new LanguageSelector(_lang));
			if (currCategoryContainer == null)
				CategoryContainer = CreateLanguageBranch<CategoryTranslationContainer>(CategoryContainer.ContentLink, true);
			else
				CategoryContainer = currCategoryContainer;
		}

		protected virtual void SetInitialTranslationContainer()
		{
			if (RootTranslationContainer != null)
				return;

			var container = _contentRepository.GetChildren<TranslationContainer>(ContentReference.RootPage).FirstOrDefault();
			if (container == null)
				CreateTranslationContainer();
			else
				RootTranslationContainer = container;
		}

		protected virtual void CreateTranslationContainer()
		{
			TranslationContainer temp = _contentRepository.GetDefault<TranslationContainer>(ContentReference.RootPage);
			temp.Name = _translationsContainerName;
			ContentReference pageReference = _contentRepository.Save(temp, SaveAction.Publish);
			RootTranslationContainer = _contentRepository.Get<TranslationContainer>(pageReference);
		}

		protected virtual void SetInitialCategoryContainer()
		{
			var container = _contentRepository.GetChildren<CategoryTranslationContainer>(RootTranslationContainer.ContentLink).FirstOrDefault();
			if (container == null)
				CreateCategoryContainer();
			else
				CategoryContainer = container;
		}

		protected virtual void CreateCategoryContainer()
		{
			CategoryTranslationContainer temp = _contentRepository.GetDefault<CategoryTranslationContainer>(RootTranslationContainer.ContentLink);
			temp.Name = _categoryContainerName;
			ContentReference pageReference = _contentRepository.Save(temp, SaveAction.Publish);
			CategoryContainer = _contentRepository.Get<CategoryTranslationContainer>(pageReference);
		}

		protected virtual void SetPages()
		{
			_pages = _contentRepository.GetDescendents(ContentReference.RootPage)
					.Select(cr => _contentRepository.Get<BaseLocalizationPage>(cr))
					.Where(p => !_contentRepository.GetAncestors(p.ContentLink)
							.Select(c => c.ContentLink.ID)
							.Contains(ContentReference.WasteBasket.ID)
						);
		}

		protected override void ReadXML(XmlNode node, ContentReference parentLink)
		{
			if (node is XmlElement)
			{
				ContentReference savedPage = HandleNode(node, parentLink);

				if (node.HasChildNodes)
					ReadXML(node.FirstChild, savedPage);

				if (node.NextSibling != null)
					ReadXML(node.NextSibling, parentLink);
			}
			else if (node is XmlText)
			{ }
			else if (node is XmlComment)
			{ }
		}

		protected virtual ContentReference HandleNode(XmlNode node, ContentReference parentLink)
		{
			if (node.Name == LANGUAGES_NODE_NAME || node.Name == CATEGORIES_NODE_NAME) { } //do nothing, we've already created the CategoryTranslationContainer
			else if (node.Name == LANGUAGE_NODE_NAME)
			{
				SetLanguage(node.Attributes["id"].Value);
				SetUpContainers();
				SetPages();
				
				return RootTranslationContainer.ContentLink;
			}
			else
			{
				try
				{
					BaseLocalizationPage item = GetPageFromNode(node);

					if (node.Name == CATEGORY_NODE_NAME && node.Attributes["name"] != null)
						parentLink = CategoryContainer.ContentLink;

					if (item == null)
						return CreatePage(node, parentLink);
					else
						return UpdatePage(item, node);
				}
				catch (InvalidOperationException) //exception already logged and thrown to continue execution 
				{ }
			}
			return parentLink;
		}

		protected override BaseLocalizationPage GetPageFromNode(XmlNode node)
		{
			try
			{
				BaseLocalizationPage item = _pages.SingleOrDefault(ti => GetPathFromNode(node) == ti.LookupKey);
				return item;
			}
			catch (InvalidOperationException ex)
			{
				Errors.Add(string.Format("Could not create/update {0}. More than one translation exists", GetPathFromNode(node)));
				Failed++;
				throw ex;
			}
		}

		protected override ContentReference CreatePage(XmlNode node, ContentReference parentLink)
		{
			try
			{
				if (!node.HasChildNodes)
					return CreatePage<TranslationItem>(node, parentLink);
				if (node.HasChildNodes && node.ChildNodes.Count == 1 && node.ChildNodes[0] is XmlText)
					return CreatePage<TranslationItem>(node, parentLink);
				else
					return CreatePage<TranslationContainer>(node, parentLink);
			}
			catch (Exception)
			{
				Failed++;
				Errors.Add(string.Format("Create page failed for {0}/{1}", _contentRepository.Get<BaseLocalizationPage>(parentLink).LookupKey, node.Name));
				return parentLink;
			}
		}

		protected virtual ContentReference CreatePage<T>(XmlNode node, ContentReference parentLink) where T : PageData, IXmlImportable
		{
			T createdPage = _contentRepository.GetDefault<T>(parentLink, new LanguageSelector(_lang));
			createdPage.SetPageData(node);
			ContentReference pageReference = _contentRepository.Save(createdPage, SaveAction.Publish);
			Created++;
			return pageReference;
		}

		protected override ContentReference UpdatePage(BaseLocalizationPage page, XmlNode node)
		{
			try
			{
				if (page is TranslationItem)
					return UpdatePage<TranslationItem>(page, node);
				if (page is TranslationContainer)
					return UpdatePage<TranslationContainer>(page, node);
			}
			catch (Exception)
			{
				Failed++;
				Errors.Add(string.Format("Update page failed for {0}", page.LookupKey));
			}
			return page.ParentLink;
		}

		protected virtual ContentReference UpdatePage<T>(BaseLocalizationPage page, XmlNode node) where T : PageData, IXmlImportable
		{
			T clone;
			T langPage = _contentRepository.Get<T>(page.ContentLink, new LanguageSelector(_lang));
			if (langPage == null)
				clone = CreateLanguageBranch<T>(page.ContentLink);
			else
				clone = page.CreateWritableClone() as T;
			
			clone.SetPageData(node);
			ContentReference updatedPage = _contentRepository.Save(clone, SaveAction.Publish);
			Updated++;
			return updatedPage;
		}

		protected virtual T CreateLanguageBranch<T>(ContentReference contentLink, bool publish = false) where T : PageData
		{
			T masterPage = _contentRepository.Get<T>(contentLink);

			T clone = _contentRepository
									.CreateLanguageBranch<T>(contentLink, new LanguageSelector(_lang))
									.CreateWritableClone() as T;
			clone.Name = masterPage.Name;
			if (publish)
			{
				ContentReference savedClone = _contentRepository.Save(clone, SaveAction.Publish);
				return _contentRepository.Get<T>(savedClone);
			}
			else
			{
				return clone;
			}
		}

		protected override string GetPathFromNode(XmlNode node)
		{
			if (node is XmlElement && node.Name != LANGUAGE_NODE_NAME)
			{
				if (node.Name == CATEGORY_NODE_NAME)
					return string.Format("{0}/{1}", GetPathFromNode(node.ParentNode), Regex.Replace(node.Attributes["name"].InnerText.ToLowerInvariant(), REGEX_REPLACE, string.Empty));
				else
					return string.Format("{0}/{1}", GetPathFromNode(node.ParentNode), Regex.Replace(node.Name.ToLowerInvariant(), REGEX_REPLACE, string.Empty));
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
