using EPiServer.ContentTransfer.Properties;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EPiServer.ContentTransfer
{
    public abstract class BaseContentExcelImporter<T, TEnum> : IContentImporter
        where T : ContentData
        where TEnum : struct
    {
        protected abstract string CATEGORY_ERROR_KEY { get; }
        protected abstract string ERROR_FORMAT_STRING { get; }
        protected abstract string CREATE_ERROR_KEY { get; }
        protected abstract string UPDATE_ERROR_KEY { get; }

        public List<string> Errors { get; protected set; }
        public int Created { get; protected set; }
        public int Updated { get; protected set; }
        public int Failed { get; protected set; }
        protected bool _overwrite { get; set; }

        protected ContentReference _container { get; set; }
        protected IContentRepository _contentRepository;
        protected int _idColumnIndex;
        protected string _lang;
        protected ILanguageSelector _langSelector;

        public BaseContentExcelImporter(List<string> errors, int containerRefId, string language = "", int idColumnIndex = 0, bool overwrite = false)
		{
			Errors = errors;
            _container = new ContentReference(containerRefId);

            if (!string.IsNullOrWhiteSpace(language))
                _lang = language; 
            else
                _lang = Settings.Default.MASTER_LANGUAGE;

            _langSelector = new LanguageSelector(_lang);

			_idColumnIndex = idColumnIndex;
			_overwrite = overwrite;
			_contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
			Created = 0;
			Updated = 0;
			Failed = 0;
		}

        public virtual void Run(Stream fileStream)
        {
            if (!CheckExcelHeaders(fileStream))
            {
                Errors.Add("Excel doc headers does not match what's expected.");
            }
            else
            {
                HandleRowsFromExcel(fileStream);
            }
        }

        protected abstract void HandleRowsFromExcel(Stream fileStream);

        protected virtual void CreateContent(string[] contentValues)
        {
            if (Settings.Default.ALWAYS_CREATE_MASTER_LANGUAGE && Settings.Default.MASTER_LANGUAGE != _lang)
            {
                T masterLanguageContent = _contentRepository.GetDefault<T>(_container, new LanguageSelector(Settings.Default.MASTER_LANGUAGE));
                SetContentName(masterLanguageContent, contentValues);
                SetGlobalProperties(masterLanguageContent, contentValues);

                masterLanguageContent = HandleCreateContentPublish(masterLanguageContent as IContent);
                if (masterLanguageContent == null)
                    return;

                T languagePage = CreateLanguageBranch((masterLanguageContent as IContent).ContentLink);
                HandleUpdate(languagePage, contentValues);
            }
            else
            {
                T content = _contentRepository.GetDefault<T>(_container, _langSelector);
                SetContentData(content, contentValues);
                HandleCreateContentPublish(content as IContent);
            }
        }

        protected virtual T HandleCreateContentPublish(IContent content)
        {
            ContentReference contentReference = _contentRepository.Save(content, SaveAction.Publish);
            var typedContent = _contentRepository.Get<T>(contentReference);
            Created++;
            return typedContent;
        }

        protected virtual void UpdateContent(IContent content, string[] contentValues)
        {
            T clone;
            T langPage = _contentRepository.Get<IContent>(content.ContentLink, _langSelector) as T;
            if (langPage == null)
                clone = CreateLanguageBranch(content.ContentLink);
            else
                clone = langPage.CreateWritableClone() as T;

            HandleUpdate(clone, contentValues);
            Updated++;
        }

        protected abstract void HandleUpdate(T clone, string[] contentValues);

        protected virtual T CreateLanguageBranch(ContentReference contentLink, bool publish = false)
        {
            IContent masterContent = _contentRepository.Get<IContent>(contentLink);

            T clone = _contentRepository
                                    .CreateLanguageBranch<T>(contentLink, _langSelector)
                                    .CreateWritableClone() as T;

            (clone as IContent).Name = masterContent.Name;
            if (publish)
            {
                ContentReference savedClone = _contentRepository.Save(clone as IContent, SaveAction.Publish);
				return _contentRepository.Get<IContent>(savedClone) as T;
            }
            else
            {
                return clone;
            }
        }

        protected abstract void SetContentName(T content, string[] contentValues);
        protected abstract void SetGlobalProperties(T content, string[] contentValues);
        protected abstract void SetContentData(T content, string[] contentValues);
        protected abstract bool CheckExcelHeaders(Stream fileStream);

        protected virtual V SetProperty<V>(V excelValue, V defaultValue)
        {
            if (_overwrite)
                return excelValue;
            else if (excelValue.ToString() == string.Empty)
                return defaultValue;
            else
                return excelValue;
        }

		protected virtual void SetContentAreaBlocks(IEnumerable<string> ids, ContentArea contentArea, List<string> errors = null)
		{
			if (!ids.Any()) return;

			if (contentArea == null) contentArea = new ContentArea().CreateWritableClone();
			if (contentArea.Items != null && contentArea.Items.Count > 0) contentArea.Items.Clear();

			foreach (var contentId in ids)
			{
				try
				{
					var contentRef = new ContentReference(int.Parse(contentId));
					if (ServiceLocator.Current.GetInstance<IContentRepository>().Get<ContentData>(contentRef) != null)
					{
						contentArea.Items.Add(new ContentAreaItem { ContentLink = contentRef });
					}
				}
				catch (ContentNotFoundException cnfex)
				{
					if (errors != null)
						errors.Add(string.Format("Could not add block.  Content with ID: {0} does not exist", contentId));
				}
				catch (Exception ex)
				{
					if (errors != null)
					{
						errors.Add(string.Format("Could not add block.  Update failed for content ID {0}", contentId));
					}
				}
			}
		}
	}
}
