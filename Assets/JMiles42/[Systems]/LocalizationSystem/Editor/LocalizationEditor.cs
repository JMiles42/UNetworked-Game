using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Language = JMiles42.Systems.Localization.Localization.Language;

namespace JMiles42.Systems.Localization
{
	public static class LocalizationEditor
	{
		public static List<Language> Languages
		{
			get { return Localization.Languages; }
			set { Localization.Languages = value; }
		}

		public static int ActiveLanguageIndex
		{
			get { return Localization.ActiveLanguageIndex; }
			set { Localization.ActiveLanguageIndex = value; }
		}

		public static Language ActiveLanguage
		{
			get
			{
				if (ActiveLanguageIndex >= 0 && ActiveLanguageIndex < Languages.Count)
					return Languages[ActiveLanguageIndex];
				return Languages[(ActiveLanguageIndex = 0)];
			}
		}

		public static string ActiveLanguageFolderPath
		{
			get { return Localization.ActiveLanguageFolderPath; }
		}

		public static string LangFilePath
		{
			get { return Localization.LangFilePath; }
		}

		static LocalizationEditor()
		{
			GetLanguages();
			LoadAll();
		}

		public static void LoadAll()
		{
			foreach (var Language in Languages)
			{
				Language.Loaded = false;
				Language.GetPages();
				Language.Load();
			}
		}

		public static void Save()
		{
			foreach (var language in Languages)
			{
				foreach (var page in language.Pages)
				{
					SavePage(page);
				}
			}
		}

		public static void GetLanguages()
		{
			if (Directory.Exists(LangFilePath))
			{
				var langs = Directory.GetDirectories(LangFilePath);
				Languages = new List<Language>(langs.Length);
				foreach (var lang in langs)
					Languages.Add(lang.Remove(0, LangFilePath.Length + 1));
			}
		}

		public static void GeneratePage(Language language, string pageName, bool copyData = false)
		{
			using (var stream = new StreamWriter(language.FilePath + "\\" + pageName + ".page"))
			{
				var activePage = ActiveLanguage.FindPage(pageName);
				if (activePage == null || !copyData)
				{
					stream.Write("");
				}
				else
				{
					foreach (var e in activePage.Data)
					{
						stream.Write(e.Key);
						if (copyData)
						{
							stream.Write("|");
							stream.Write(e.Value);
						}
						stream.Write('\n');
					}
				}
			}
			language.LoadPage(pageName);
		}

		public static void CreateNewLanguage(string newLang)
		{
			if (!Languages.Contains(newLang))
				Languages.Add(newLang);
			Directory.CreateDirectory(LangFilePath + "\\" + newLang);
			using (var stream = new StreamWriter(LangFilePath + "\\" + newLang + "\\" + Localization.DEFUALT_PAGE_NAME + ".page"))
			{
				stream.Write("no_vaild_entry|There was no entry for the key:");
				stream.Write('\n');
				stream.Write("language_name|");
				stream.Write(newLang);
				stream.Write('\n');
				stream.Write("game_title|");
				stream.Write(Application.productName);
			}
		}

		public static void GenerateAllPagesFromActive(bool copyData = false)
		{
			ActiveLanguage.Loaded = true;
			for (int i = 0; i < Languages.Count; i++)
			{
				if (i >= Languages.Count)
					break;
				Languages[i].Loaded = true;
				if (i == ActiveLanguageIndex)
					continue;
				foreach (var page in ActiveLanguage.Pages)
				{
					if (!File.Exists(page.PageName))
					{
						GeneratePage(Languages[i], page.PageName, copyData);
					}
				}
			}
			LoadAll();
		}

		public static void SavePage(Page page)
		{
			using (var stream = new StreamWriter(page.PageFilePath))
			{
				page.Loaded = true;
				if (page.Loaded)
				{
					foreach (var pageEntry in page.Data)
					{
						stream.Write(pageEntry.Key + "|" + pageEntry.Value + "\n");
					}
				}
			}
		}

		public static void AddKey(string pageName, string newKey, string newKeyData = "")
		{
			foreach (var language in Languages)
			{
				var page = language.FindPage(pageName);
				if (!page.Data.ContainsKey(newKey))
					page.Data.Add(newKey, newKeyData);

				SavePage(page);
			}
		}

		public static void RemoveKey(string pageName, string removekey)
		{
			foreach (var language in Languages)
			{
				var page = language.FindPage(pageName);
				page.Data.Remove(removekey);

				SavePage(page);
			}
		}

		public static void AddPage(string newPage)
		{
			foreach (var language in Languages)
			{
				var str = language.FilePath + '\\' + newPage + ".page";
				if (!File.Exists(str))
				{
					GeneratePage(language, newPage, false);
				}
			}
			LoadAll();
		}

		public static void DeletePage(string deletePage)
		{
			foreach (var language in Languages)
			{
				var page = language.FindPage(deletePage);
				if (page != null)
				{
					if (File.Exists(page.PageFilePath))
						File.Delete(page.PageFilePath);
					if (File.Exists(page.PageFilePath + ".meta"))
						File.Delete(page.PageFilePath + ".meta");
					language.Pages.Remove(page);
				}
			}
			Save();
			LoadAll();
		}
	}
}