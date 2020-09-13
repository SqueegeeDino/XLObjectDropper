﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using XLObjectDropper.UI.Utilities;
using Object = UnityEngine.Object;

namespace XLObjectDropper.Utilities
{
	public static class AssetBundleHelper
	{
		public static List<Spawnable> LoadedSpawnables { get; private set; }
		public static string ImagesPath;
		public static string AssetPacksPath;

		static AssetBundleHelper()
		{
			LoadedSpawnables = new List<Spawnable>();
		}

		public static void LoadDefaultBundles()
		{
			ImagesPath = Path.Combine(Main.ModPath, "Images");

			if (!Directory.Exists(ImagesPath))
			{
				Directory.CreateDirectory(ImagesPath);
			}

			PlayerController.Instance.StartCoroutine(LoadBundleAsync("XLObjectDropper.Assets.object_testbundle", true));
		}

		public static void LoadUserBundles()
		{
			AssetPacksPath = Path.Combine(Main.ModPath, "AssetPacks");

			if (!Directory.Exists(AssetPacksPath))
			{
				Directory.CreateDirectory(AssetPacksPath);
			}

			foreach (var assetPack in Directory.GetFiles(AssetPacksPath, "*", SearchOption.AllDirectories))
			{
				if (Path.HasExtension(assetPack)) continue;

				var bundlePath = Path.Combine(ImagesPath, Path.GetFileName(assetPack));
				if (!Directory.Exists(bundlePath))
				{
					Directory.CreateDirectory(bundlePath);
				}

				try
				{
					PlayerController.Instance.StartCoroutine(LoadBundleAsync(assetPack));
				}
				catch (Exception e)
				{
					UnityModManager.Logger.Log("XLObjectDropper: Failed to load asset pack: " + assetPack);
				}
			}
		}

		public static void DisposeLoadedAssets()
		{
			foreach (var spawnable in LoadedSpawnables)
			{
				spawnable.Prefab.SetActive(false);
				Object.Destroy(spawnable.Prefab);
			}

			LoadedSpawnables.Clear();
		}

		static IEnumerator LoadBundleAsync(string name, bool isEmbedded = false)
		{
			AssetBundleCreateRequest abCreateRequest;

			if (isEmbedded)
			{
				abCreateRequest = AssetBundle.LoadFromMemoryAsync(ExtractResource(name));
			}
			else
			{
				abCreateRequest = AssetBundle.LoadFromFileAsync(name);
			}

			yield return abCreateRequest;

			var bundle = abCreateRequest.assetBundle;
			if (bundle == null) yield break;

			var assetLoadRequest = bundle.LoadAllAssetsAsync<GameObject>();
			yield return assetLoadRequest;

			var assets = assetLoadRequest.allAssets;
			if (assets == null || !assets.Any()) yield break;

			foreach (var asset in assets)
			{
				var type = SpawnableType.Rails;

				if (!isEmbedded)
				{
					type = SpawnableType.Packs;
				}
				else
				{
					if (asset.name.StartsWith("grind", StringComparison.InvariantCultureIgnoreCase) || asset.name.StartsWith("coping", StringComparison.InvariantCultureIgnoreCase))
					{
						type = SpawnableType.Splines;
					}
					else
					{
						type = SpawnableType.Rails;
					}
				}

				LoadedSpawnables.Add(new Spawnable(type, asset as GameObject, bundle));
			}

			SelfieCamera.enabled = false;

			bundle.Unload(false);
		}

		public static GameObject UIPrefab { get; set; }
		public static GameObject ListItemPrefab { get; set; }
		public static GameObject CustomPassPrefab { get; set; }
		public static Camera SelfieCamera { get; set; }
		public static GameObject GridOverlayPrefab { get; set; }

		public static void LoadUIBundle()
		{
			AssetBundle bundle = AssetBundle.LoadFromMemory(ExtractResource("XLObjectDropper.Assets.ui_bundle"));

			var prefabPath = "Assets/OBJ_Dropper_Bundles/UI_Bundle/";

			UIPrefab = bundle.LoadAsset<GameObject>(prefabPath + "ObjDrop_UI.prefab");
			ListItemPrefab = bundle.LoadAsset<GameObject>(prefabPath + "ListItem.prefab");
			CustomPassPrefab = bundle.LoadAsset<GameObject>(prefabPath + "Outline Custom Pass.prefab");
			GridOverlayPrefab = bundle.LoadAsset<GameObject>("Assets/GridLayout/Grid_Overlay.prefab");

			SelfieCamera = bundle.LoadAsset<GameObject>(prefabPath + "SelfieCamera.prefab").GetComponent<Camera>();

			bundle.Unload(false);
		}

		private static byte[] ExtractResource(string filename)
		{
			Assembly a = Assembly.GetExecutingAssembly();
			using (var resFilestream = a.GetManifestResourceStream(filename))
			{
				if (resFilestream == null) return null;
				byte[] ba = new byte[resFilestream.Length];
				resFilestream.Read(ba, 0, ba.Length);
				return ba;
			}
		}
	}
}
