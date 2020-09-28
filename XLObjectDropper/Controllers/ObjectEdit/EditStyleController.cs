﻿using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLObjectDropper.SpawnableScripts;
using XLObjectDropper.UI.Controls;
using XLObjectDropper.UI.Menus;
using XLObjectDropper.Utilities;

namespace XLObjectDropper.Controllers.ObjectEdit
{
	public static class EditStyleController
	{
		public static void AddStyleOptions(GameObject SelectedObject, ObjectEditUI ObjectEdit)
		{
			var spawnable = SelectedObject.GetSpawnable();

			if (spawnable?.AlternateStyles == null || !spawnable.AlternateStyles.Any()) return;

			var newExpandable = ObjectEdit.AddStyleSettings();

			var expandable = newExpandable.GetComponent<Expandable>();
			var styleExpandable = newExpandable.GetComponent<StyleSettingsExpandable>();

			string style = spawnable.Prefab.GetComponent<StyleController>().Style;
			string subStyle = spawnable.Prefab.GetComponent<StyleController>().SubStyle;

			string name = style;
			if (!string.IsNullOrEmpty(subStyle))
			{
				name += $" / {subStyle}";
			}

			AddListItem(styleExpandable.ListItemPrefab, expandable.PropertiesListContent.transform, spawnable, name);

			foreach (var altStyle in spawnable.AlternateStyles)
			{
				style = altStyle.Prefab.GetComponent<StyleController>().Style;
				subStyle = altStyle.Prefab.GetComponent<StyleController>().SubStyle;

				name = style;
				if (!string.IsNullOrEmpty(subStyle))
				{
					name += $" / {subStyle}";
				}

				AddListItem(styleExpandable.ListItemPrefab, expandable.PropertiesListContent.transform, altStyle, name);
			}
		}

		private static void AddListItem(GameObject prefab, Transform listContent, Spawnable spawnable, string customName = null)
		{
			var listItem = GameObject.Instantiate(prefab, listContent);

			if (!string.IsNullOrEmpty(customName))
			{
				listItem.GetComponentInChildren<TMP_Text>().SetText(customName);
			}
			else
			{
				listItem.GetComponentInChildren<TMP_Text>().SetText(spawnable.Prefab.name.Replace('_', ' '));
			}
			
			//if (objectClicked != null)
			//{
			//	listItem.GetComponent<Button>().onClick.AddListener(objectClicked);
			//}

			//if (objectSelected != null)
			//{
			//	listItem.GetComponent<ObjectSelectionListItem>().onSelect.AddListener(objectSelected);
			//}

			listItem.GetComponentInChildren<RawImage>().texture = spawnable.PreviewTexture;

			listItem.SetActive(true);
		}
	}
}
