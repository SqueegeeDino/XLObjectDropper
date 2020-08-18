﻿using GameManagement;
using Rewired;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityModManagerNet;
using XLObjectDropper.GameManagement;
using XLObjectDropper.UI;
using XLObjectDropper.UserInterface;

namespace XLObjectDropper
{
	public class ObjectMovementController : MonoBehaviour
	{
		public GameObject PreviewObject { get; set; }
        public List<GameObject> SpawnedObjects { get; set; }
        
		private TMP_Text ZoomInOutText { get; set; }
		private static PinMovementController PinMovementController { get; set; }
		private static GameObject OriginalPinObject { get; set; }

		private void Awake()
        {
	        SpawnedObjects = new List<GameObject>();

	        PinMovementController = GameStateMachine.Instance.PinObject.GetComponent<PinMovementController>();

			PreviewObject = Instantiate(AssetBundleHelper.LoadedAssets.ElementAt(0), PinMovementController.GroundIndicator.transform);
			PinMovementController.GroundIndicator.transform.localScale = Vector3.one;
			PreviewObject.transform.rotation = GameStateMachine.Instance.PinObject.transform.rotation;
			PreviewObject.transform.position = GameStateMachine.Instance.PinObject.transform.position;
			
			PreviewObject.transform.ChangeLayersRecursively("Ignore Raycast");

			PreviewObject.SetActive(false);
			
	        //DontDestroyOnLoad(PreviewObject);

	        UserInterfaceHelper.LoadUserInterface();
	        
	        OriginalPinObject = GameStateMachine.Instance.PinObject;

	        if (!(GameStateMachine.Instance.CurrentState.GetType() != typeof(ObjectMovementState)))
				return;

			enabled = false;
        }

		private void OnEnable()
        {
	        enabled = true;
	        GameStateMachine.Instance.PinObject.SetActive(true);

	        UserInterfaceHelper.UserInterface?.SetActive(true);

			PinMovementController.PinRenderer.enabled = false;

			PreviewObject.SetActive(true);

			ZoomInOutText = GameStateMachine.Instance.PinObject.GetComponentInChildren<TMP_Text>();
			ZoomInOutText?.gameObject?.SetActive(false);
        }

        private void OnDisable()
        {
			enabled = false;

			GameStateMachine.Instance.PinObject.SetActive(false);

			UserInterfaceHelper.UserInterface?.SetActive(false);
			ZoomInOutText?.gameObject?.SetActive(true);

			PinMovementController.PinRenderer.enabled = true;

			PreviewObject.SetActive(false);
        }

        private bool showMenu;

        private void Update()
        {
	        UpdateGroundLevel();

	        Player player = PlayerController.Instance.inputController.player;

			if (player.GetButtonSinglePressHold("LB"))
			{
				Time.timeScale = 0.0f;

				// If left stick movement, rotate the object on X/z axis
				Vector2 leftStick = player.GetAxis2D("LeftStickX", "LeftStickY");
				PreviewObject.transform.Rotate(leftStick.y, leftStick.x, 0);

				// If right stick Y movement, scale object
				var scaleFactor = 10f;
				Vector2 rightStick = player.GetAxis2D("RightStickX", "RightStickY");
				PreviewObject.transform.localScale += new Vector3(rightStick.y / scaleFactor, rightStick.y / scaleFactor, rightStick.y / scaleFactor);


				// If a, place the object, but keep the preview object
				if (player.GetButtonDown("A"))
				{
					Debug.Log("XLObjectDropper: Holding LB Pressed A");

					UISounds.Instance?.PlayOneShotSelectMajor();

					var newObject = Instantiate(PreviewObject, PreviewObject.transform.position, PreviewObject.transform.rotation);
					newObject.SetActive(true);
					SpawnedObjects.Add(newObject);

					newObject.transform.ChangeLayersRecursively("Default");
				}
				else if (player.GetButtonDown("Left Stick Button"))
				{
					PreviewObject.transform.rotation = GameStateMachine.Instance.PinObject.transform.rotation;
				}
				else if (player.GetButtonDown("Right Stick Button"))
				{
					PreviewObject.transform.localScale = Vector3.one;
				}
			}
			else
	        {
		        // If dpad up/down, move object up/down
		        var scaleFactor = 10f;
		        Vector2 dpad = player.GetAxis2D("DPadX", "DPadY");
		        PreviewObject.transform.position = new Vector3(PreviewObject.transform.position.x, PreviewObject.transform.position.y + dpad.y / scaleFactor, PreviewObject.transform.position.z);

		        if (player.GetButtonDown("A") && PreviewObject.activeSelf)
				{
					// If a, place object and delete preview
					Debug.Log("XLObjectDropper: Pressed A");
					UISounds.Instance?.PlayOneShotSelectMajor();

					var newObject = Instantiate(PreviewObject, PreviewObject.transform.position, PreviewObject.transform.rotation);
					newObject.SetActive(true);

					newObject.transform.ChangeLayersRecursively("Default");

					SpawnedObjects.Add(newObject);

					PreviewObject.SetActive(false);
				}
				else if (player.GetButtonDown("X"))
				{
					// if x, open new object selection menu
					Debug.Log("XLObjectDropper: Pressed X");
					UISounds.Instance?.PlayOneShotSelectMinor();
					
					showMenu = !showMenu;

					GameStateMachine.Instance.RequestTransitionTo(typeof(ObjectSelectionState));
				}
				else if (player.GetButtonDown("Y"))
				{
					// if y, delete highlighted object (if any)
					Debug.Log("XLObjectDropper: Pressed Y");
				}
				else if (player.GetButtonDown("Left Stick Button"))
		        {
					PreviewObject.transform.localScale = Vector3.one;
					PreviewObject.transform.rotation = GameStateMachine.Instance.PinObject.transform.rotation;
				}
				else if (player.GetButtonDown("Select"))
		        {
			        // stop obj movement here
		        }
				else if (player.GetButtonDown("Start"))
		        {
					// stop obj movement here
		        }
	        }
        }

        private float groundLevel;

        private bool UpdateGroundLevel()
        {
	        Ray ray1 = new Ray(this.transform.position, Vector3.down);
	        Ray ray2 = new Ray(this.transform.position, Vector3.down);
	        RaycastHit raycastHit = new RaycastHit();
	        ref RaycastHit local = ref raycastHit;
	        if (!Physics.SphereCast(ray1, 0.2f, out local))
		        return false;
	        this.groundLevel = raycastHit.point.y;

			//Debug.Log("XLObjectDropper: groundLevel : " + groundLevel);

	        return true;
        }
	}

	public static class Extensions
	{
		public static void ChangeLayersRecursively(this Transform trans, string name)
		{
			trans.gameObject.layer = LayerMask.NameToLayer(name);
			foreach (Transform child in trans)
			{
				child.ChangeLayersRecursively(name);
			}
		}
	}
}