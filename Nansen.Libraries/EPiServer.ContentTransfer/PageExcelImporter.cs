using EPiServer.Core;
using EPiServer.DataAbstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using EPiServer.ContentTransfer.Utility;

namespace BlueBuffalo.Core.ContentTransfer
{
    public abstract class PageExcelImporter<T, TEnum> : BaseContentExcelImporter<T, TEnum>
        where T : PageData
        where TEnum : struct
    {
        protected IEnumerable<T> _pages;

        public PageExcelImporter(List<string> errors, int containerRefId, string language = "", int idColumnIndex = 0, bool overwrite = false)
            : base(errors, containerRefId, language, idColumnIndex, overwrite)
        {
			_contentRepository.GetDescendents(ContentReference.StartPage).Select(cr => _contentRepository.Get<T>(cr, LanguageSelector.MasterLanguage()));
        }

        protected override void HandleRowsFromExcel(Stream fileStream)
        {
            //Get the file
            Dictionary<string, string[]> excelData;
            try
            {
                excelData = ExcelUtility.GetExcelData(fileStream, _idColumnIndex);
            }
            catch
            {
                Errors.Add("The file could not be found on the server.");
                return;
            }

            foreach (var page in excelData)
            {
                HandleRow(page);
            }
        }

        protected virtual void HandleRow(KeyValuePair<string, string[]> productRow)
        {
            string[] pageValues = productRow.Value;
            T page = GetPageFromRow(pageValues);

            if (page != null) //if page already exists, update
            {
                try
                {
                    UpdateContent(page, pageValues);
                }
                catch
                {
                    Errors.Add(string.Format(ERROR_FORMAT_STRING, UPDATE_ERROR_KEY, pageValues[_idColumnIndex]));
                    Failed++;
                }
            }
            else //create
            {
                try
                {
                    CreateContent(pageValues);
                }
                catch
                {
                    if (!string.IsNullOrWhiteSpace(pageValues[_idColumnIndex]))
                    {
                        Errors.Add(string.Format(ERROR_FORMAT_STRING, CREATE_ERROR_KEY, pageValues[_idColumnIndex]));
                        Failed++;
                    }
                }
            }
        }

        protected abstract T GetPageFromRow(string[] pageValues);

        protected virtual CategoryList GetCategories(string categoryString, string contentId)
        {
            CategoryList categoryList = new CategoryList();
            string[] categories = categoryString.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string categoryName in categories)
            {
                Category categoryToAdd = Category.GetRoot().FindChild(categoryName);
                if (categoryToAdd == null)
                {
                    string categoryErrorString = string.Format(ERROR_FORMAT_STRING, CATEGORY_ERROR_KEY, contentId);
                    Errors.Add(string.Format("{0} Category: {1}", categoryErrorString, categoryName));
                    continue;
                }

                categoryList.Add(categoryToAdd.ID);
            }
            return categoryList;
        }
    }
}
