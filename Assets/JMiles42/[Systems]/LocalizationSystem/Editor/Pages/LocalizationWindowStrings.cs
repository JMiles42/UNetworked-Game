using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JMiles42.Editor;
using JMiles42.Extensions;
using UnityEditor;
using UnityEngine;

namespace JMiles42.Systems.Localization
{
	public class LocalizationWindowStrings: Tab<LocalizationWindow>
	{
		private const float PAGE_WIDTH = 230;
		private const float KEYS_WIDTH = 230;

		public override string TabName
		{
			get { return "Strings"; }
		}

		public int ActivePageIndex
		{
			get
			{
				if (activePageIndex >= LocalizationEditor.ActiveLanguage.Pages.Count)
					activePageIndex = LocalizationEditor.ActiveLanguage.Pages.Count - 1;
				return activePageIndex;
			}
			set { activePageIndex = value; }
		}

		public int activePageIndex = 0;

		private Vector2 pageScroll = Vector2.zero;

		private Vector2 entryScroll = Vector2.zero;

		private Vector2 entryDetailScroll = Vector2.zero;

		private string newChangedKey = "";

		public string activeKeyKey = "";
		public int activeKeyIndex = 0;

		public override void DrawTab(Window<LocalizationWindow> owner)
		{
			using (new GUILayout.HorizontalScope(EditorStyles.toolbarButton))
				DrawHeaderGUI();

			using (new GUILayout.HorizontalScope())
			{
				using (var scroll = new GUILayout.ScrollViewScope(pageScroll,
																  false,
																  false,
																  //GUI.skin.horizontalScrollbar,
																  //GUI.skin.verticalScrollbar,
																  //EditorStyles.textArea,
																  GUILayout.Width(PAGE_WIDTH),
																  GUILayout.ExpandHeight(true)) {handleScrollWheel = true})
				{
					pageScroll = scroll.scrollPosition;
					DrawPagesGUI();
				}

				using (var scroll = new GUILayout.ScrollViewScope(entryScroll,
																  false,
																  false,
																  //GUI.skin.horizontalScrollbar,
																  //GUI.skin.verticalScrollbar,
																  //EditorStyles.textArea,
																  GUILayout.Width(KEYS_WIDTH),
																  GUILayout.ExpandHeight(true)) {handleScrollWheel = true})
				{
					entryScroll = scroll.scrollPosition;

					DrawKeysGUI();
				}

				using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
				{
					DrawDetailsGUI();
					if (changeCheckScope.changed)
					{
						LocalizationEditor.Save();
					}
				}
			}
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
				DrawFooter();
		}

		private void DrawHeaderGUI()
		{
			EditorGUILayout.LabelField("Pages", GUILayout.Width(PAGE_WIDTH));
			EditorGUILayout.LabelField("Keys", GUILayout.Width(KEYS_WIDTH));
			EditorGUILayout.LabelField("Details");
		}

		#region Page
		private void DrawPagesGUI()
		{
			if (LocalizationEditor.ActiveLanguage.Pages != null && LocalizationEditor.ActiveLanguage.Pages.Count > 0)
			{
				for (var i = 0; i < LocalizationEditor.ActiveLanguage.Pages.Count; i++)
					using (new GUILayout.HorizontalScope(EditorStyles.toolbarButton))
						DrawPagesGUI(i);
				DrawNewPageEntry();
			}
			else
			{
				LocalizationEditor.ActiveLanguage.GetPages();
			}
		}

		private void DrawPagesGUI(int index)
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Options"), false, () => {});
			genericMenu.AddItem(new GUIContent("Add New Page: " + newPage), false, AddPage);
			genericMenu.AddItem(new GUIContent("Delete Page: " + LocalizationEditor.ActiveLanguage.Pages[index].PageName), false, () => DeletePage(index));
			var e = DrawItemButton(ActivePageIndex == index, LocalizationEditor.ActiveLanguage.Pages[index].PageName, genericMenu, Localization.DEFUALT_PAGE_NAME == LocalizationEditor.ActiveLanguage.Pages[index].PageName);
			if (e == ButtonResualt.Pressed)
			{
				ActivePageIndex = index;
				activeKeyKey = "";
				newChangedKey = "";
			}
			else if (e == ButtonResualt.Close)
			{
				DeletePage(index);
			}
		}

		private string newPage = "newPage";

		private void DrawNewPageEntry()
		{
			using (new GUILayout.HorizontalScope(EditorStyles.toolbarButton))
			{
				using (new GUILayout.HorizontalScope(EditorStyles.toolbarButton))
					newPage = EditorGUILayout.DelayedTextField(newPage, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Add Page", EditorStyles.toolbarButton, GUILayout.Width(60)))
				{
					AddPage();
				}
			}
		}

		private void AddPage()
		{
			LocalizationEditor.AddPage(newPage.ReplaceStringHaveInvalidChars());
			if (LocalizationEditor.ActiveLanguage.FindPage(newPage) != null)
			{
				newPage = StringNumber(newPage);
			}
		}

		private void DrawPageFooterGUI(float width)
		{
			var itemWidth = width / 2;
			if (GUILayout.Button("New Page", EditorStyles.toolbarButton, GUILayout.Width(itemWidth)))
			{
				AddPage();
			}
			using (new EditorGUI.DisabledGroupScope(LocalizationEditor.ActiveLanguage.Pages[activePageIndex].PageName == Localization.DEFUALT_PAGE_NAME))
			{
				if (GUILayout.Button("Remove Page", EditorStyles.toolbarButton, GUILayout.Width(itemWidth)))
				{
					DeletePage();
				}
			}
		}

		private void DeletePage(bool showWarning = true)
		{
			if (showWarning)
			{
				var yes = EditorUtility.DisplayDialog("Delete Page?",
													  String.Format("Are you sure you want to delete the Page: {0}\n\nThis will delete the Page file in the Lang Directory, and all that it contains.",
																	LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName),
													  "Yes",
													  "No, why would I!");
				if (yes)
				{
					LocalizationEditor.DeletePage(LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName);
				}
			}
			else
				LocalizationEditor.DeletePage(LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName);
		}

		private void DeletePage(int index, bool showWarning = true)
		{
			if (showWarning)
			{
				var yes = EditorUtility.DisplayDialog("Delete Page?",
													  String.Format("Are you sure you want to delete the Page: {0}\n\nThis will delete the Page file in the Lang Directory, and all that it contains.",
																	LocalizationEditor.ActiveLanguage.Pages[index].PageName),
													  "Yes",
													  "No, why would I!");
				if (yes)
				{
					LocalizationEditor.DeletePage(LocalizationEditor.ActiveLanguage.Pages[index].PageName);
				}
			}
			else
			{
				LocalizationEditor.DeletePage(LocalizationEditor.ActiveLanguage.Pages[index].PageName);
			}
		}
		#endregion

		#region Keys
		private void DrawKeysGUI()
		{
			if (LocalizationEditor.ActiveLanguage.Pages != null && LocalizationEditor.ActiveLanguage.Pages.Count > 0)
			{
				if (LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data == null || LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data.Count > 0)
				{
					LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Load();
				}
				if (LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data != null && LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data.Count > 0)
				{
					if (!LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data.ContainsKey(activeKeyKey))
					{
						var list = LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data.Keys.ToList();
						if (list.Count > activeKeyIndex)
							activeKeyKey = list[activeKeyIndex];
						else
							activeKeyKey = list.Last();
					}
				}
				activeKeyIndex = 0;
				int i = 0;
				if (LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data != null)
				{
					try
					{
						foreach (var entry in LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data)
						{
							if (activeKeyKey == entry.Key)
								activeKeyIndex = i;
							++i;

							using (new GUILayout.HorizontalScope(EditorStyles.toolbarButton))
								DrawKeyGUI(entry);
						}
					}
					catch (Exception e)
					{
						//Console.WriteLine(e);
						//throw;
					}
				}
				using (new GUILayout.HorizontalScope(EditorStyles.toolbarButton))
					DrawNewKeyEntry();
			}
			else
			{
				LocalizationEditor.ActiveLanguage.GetPages();
			}
		}

		private string newKey = "newKey";

		private void DrawNewKeyEntry()
		{
			using (new GUILayout.HorizontalScope(EditorStyles.toolbarButton))
				newKey = EditorGUILayout.DelayedTextField(newKey, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));

			var e = GUILayout.Button("Add Key", EditorStyles.toolbarButton, GUILayout.Width(60));

			if (e)
			{
				AddKey();
			}
		}

		private void DrawKeyGUI(KeyValuePair<string, string> index)
		{
			using (new GUILayout.HorizontalScope())
			{
				var genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent("Options"), false, () => {});
				genericMenu.AddItem(new GUIContent("Add New Key : " + newKey), false, AddKey);
				genericMenu.AddItem(new GUIContent("Delete Key: " + index.Key), false, () => DeleteKey(index.Key, false));
				var g = DrawItemButton(activeKeyKey == index.Key, index.Key, genericMenu, Localization.DEFUALT_LANGUAGE_ENTRY == index.Key || Localization.DEFUALT_NO_ENTRY == index.Key);
				if (g == ButtonResualt.Pressed)
				{
					activeKeyKey = index.Key;
					newChangedKey = "";
				}
				else if (g == ButtonResualt.Close)
				{
					DeleteKey(index.Key);
				}
			}
		}

		private void DrawKeyFooterGUI(float width)
		{
			var itemWidth = width / 2;
			if (GUILayout.Button("New Key", EditorStyles.toolbarButton, GUILayout.Width(itemWidth)))
			{
				AddKey();
			}

			using (new EditorGUI.DisabledGroupScope(
				!LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data.ContainsKey(activeKeyKey)
				||Localization.DEFUALT_LANGUAGE_ENTRY == activeKeyKey ||Localization.DEFUALT_NO_ENTRY == activeKeyKey
			))
			{
				if (GUILayout.Button("Remove Key", EditorStyles.toolbarButton, GUILayout.Width(itemWidth)))
				{
					DeleteKey();
				}
			}
		}

		private void AddKey()
		{
			LocalizationEditor.AddKey(LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName, newKey);
			if (LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data.ContainsKey(newKey))
			{
				newKey = StringNumber(newKey);
			}
		}

		private static string StringNumber(string str)
		{
			var resultString = Regex.Match(str, @"\d+");

			if (resultString.Success)
			{
				var i = int.Parse(resultString.Value);
				str = str.Replace(i++.ToString(), "") + i;
			}
			else
			{
				str += "1";
			}
			return str;
		}

		private void DeleteKey(bool showWarning = true)
		{
			if (showWarning) {
				var yes = EditorUtility.DisplayDialog(
					"Delete Key?",
					String.Format("Are you sure you want to delete the Entry: {0}", activeKeyKey),
					"Yes",
					"No, why would I!");
				if (yes) {
					LocalizationEditor.RemoveKey(LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName, activeKeyKey);
				}
			}
			else {
				LocalizationEditor.RemoveKey(LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName, activeKeyKey);
			}
		}

		private void DeleteKey(string key, bool showWarning = true)
		{
			if (showWarning)
			{
				var yes = EditorUtility.DisplayDialog("Delete Key?", String.Format("Are you sure you want to delete the Entry: {0}", key), "Yes", "No, why would I!");
				if (yes)
				{
					LocalizationEditor.RemoveKey(LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName, key);
				}
			}
			else
				LocalizationEditor.RemoveKey(LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName, key);
		}

		private void DrawDetailsFooterGUI()
		{
			EditorGUILayout.LabelField("Details");
			if (GUILayout.Button("Reload files (Warning, does not save data!)", EditorStyles.toolbarButton))
			{
				var yes = EditorUtility.DisplayDialog("Reload all files",
													  "Are you sure you want to reload all the files, this will delete any non-saved changes!",
													  "Yes (possable loss of data)",
													  "No");
				if (yes)
				{
					ActivePageIndex = 0;
					activeKeyKey = "";
					LocalizationEditor.LoadAll();
				}
			}
		}
		#endregion

		#region Details
		private static void DrawError()
		{
			using (new GUILayout.VerticalScope(GUI.skin.textArea, GUILayout.ExpandHeight(true)))
			{
				EditorGUILayout.HelpBox("No Keys Found!", MessageType.Error);
			}
		}

		private void DrawDetailsGUI()
		{
			var lastKey = activeKeyKey;

			#region Null&ErrorChecks
			if (LocalizationEditor.Languages == null)
			{
				DrawError();
				return;
			}
			if (LocalizationEditor.ActiveLanguage == null)
			{
				DrawError();
				return;
			}
			if (LocalizationEditor.ActiveLanguage.Pages == null)
			{
				DrawError();
				return;
			}
			if (LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex] == null)
			{
				DrawError();
				return;
			}
			if (!LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Loaded)
			{
				DrawError();
				return;
			}
			if (!LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].Data.ContainsKey(activeKeyKey))
			{
				DrawError();
				return;
			}
			#endregion

			if (newChangedKey == "")
				newChangedKey = activeKeyKey;
			using (new GUILayout.VerticalScope(GUI.skin.textArea))
			{
				using (var scope = new GUILayout.ScrollViewScope(entryDetailScroll, GUILayout.ExpandHeight(true)))
				{
					entryDetailScroll = scope.scrollPosition;
					newChangedKey = EditorGUILayout.DelayedTextField(new GUIContent("Key: "), newChangedKey, GUILayout.ExpandWidth(true));

					var pageName = LocalizationEditor.ActiveLanguage.Pages[ActivePageIndex].PageName;

					foreach (var language in LocalizationEditor.Languages)
					{
						language.Loaded = true;
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							if (language.DefualtPage.Data.ContainsKey("language_name"))
								EditorGUILayout.SelectableLabel(string.Format("{0} | {1}", language.LanguageName, language.DefualtPage.Data["language_name"]), GUILayout.Height(18f));
							else
								EditorGUILayout.SelectableLabel(language.LanguageName, GUILayout.Height(18f));

							bool pageFound = false;
							Page foundPage = null;
							foreach (var page in language.Pages)
							{
								if (page.PageName == pageName)
								{
									pageFound = true;
									foundPage = page;
									foundPage.Loaded = true;
									break;
								}
							}
							if (!pageFound || foundPage == null || !foundPage.Loaded)
							{
								using (new GUILayout.HorizontalScope())
								{
									EditorGUILayout.HelpBox(string.Format("No Page found for :{0} in {1}", pageName, language.LanguageName), MessageType.Warning);
									if (GUILayout.Button("Generate Page", GUILayout.Width(128), GUILayout.Height((16 * 2.5f) - 1.1f)))
									{
										LocalizationEditor.GeneratePage(language, pageName);
									}
									if (GUILayout.Button("Generate Page\nAnd Copy Data", GUILayout.Width(128), GUILayout.Height((16 * 2.5f) - 1.1f)))
									{
										LocalizationEditor.GeneratePage(language, pageName, true);
									}
								}
							}
							else
							{
								bool loaded = foundPage.Loaded;
								foundPage.Loaded = true;
								if (!foundPage.Data.ContainsKey(activeKeyKey))
									foundPage.Data.Add(activeKeyKey, "");

								var str = EditorGUILayout.TextField(foundPage.Data[activeKeyKey], GUILayout.Height(18 * 3), GUILayout.ExpandWidth(true));
								if (foundPage.Data.ContainsKey(activeKeyKey) && (foundPage.Data[activeKeyKey] != str))
								{
									if (changedTab)
										changedTab = false;
									else
									{
										foundPage.Data[activeKeyKey] = str;
									}
								}
							}
						}
					}
					if (newChangedKey != activeKeyKey)
					{
						foreach (var language in LocalizationEditor.Languages)
						{
							language.Pages[ActivePageIndex].Data.ChangeKey(activeKeyKey, newChangedKey);
						}
						activeKeyKey = newChangedKey;
					}
				}
			}
		}
		#endregion

		private void DrawFooter()
		{
			DrawPageFooterGUI(PAGE_WIDTH);
			DrawKeyFooterGUI(KEYS_WIDTH);
			DrawDetailsFooterGUI();
		}

		private bool changedTab = false;

		private ButtonResualt DrawItemButton(bool toggle, string label, GenericMenu genericMenu, bool hideClose = false)
		{
			GUI.SetNextControlName(label + "f");
			var b = JMilesGUILayoutEvents.Toggle(toggle, label, EditorStyles.toolbarButton);
			if (b.EventIsMouse1InRect)
			{
				GUI.FocusControl(label + "f");
				genericMenu.ShowAsContext();
			}
			if (b.EventIsMouse0InRect)
			{
				GUI.FocusControl(label + "f");
			}
			if(!hideClose)
			{
				var b2 = GUILayout.Button(EditorGUIUtility.FindTexture("d_winbtn_win_close_h"), EditorStyles.toolbarButton, GUILayout.Width(24f));

				if (b2)
					return ButtonResualt.Close;
			}

			return b ? ButtonResualt.Pressed : ButtonResualt.Nothing;
		}


		enum ButtonResualt{
			Nothing,
			Pressed,
			Close
		}
	}
}