using System;
using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using GameTweaks.Tweaks;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Utilities;
using PowerTools;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Assets;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace GameTweaks.Components;

[RegisterInIl2Cpp]
public class CustomSpawnInMinigame(IntPtr cppPtr) : Minigame(cppPtr)
{

#pragma warning disable CS8618
	public TacticalDeploymentTweak.CustomSpawnLocation[] Locations;
	public PassiveButton[] LocationButtons;
	public TextMeshPro Text;
	public AudioClip DefaultRolloverSound;
	public UiElement DefaultButtonSelected;
	public Il2CppSystem.Collections.Generic.List<UiElement> ControllerSelectable;
	private bool gotButton;
#pragma warning restore CS8618

	public static CustomSpawnInMinigame Create()
	{
		var prefab = Resources.FindObjectsOfTypeAll(Il2CppType.Of<SpawnInMinigame>()).Cast<Il2CppReferenceArray<SpawnInMinigame>>();
		var minigame = Instantiate(prefab[0], Camera.main!.transform, false);
		minigame.transform.localPosition = new Vector3(0f, 0f, -600f);

		var customMinigame = minigame.gameObject.AddComponent<CustomSpawnInMinigame>();
		customMinigame.LocationButtons = minigame.LocationButtons;
		customMinigame.Text = minigame.Text;
		customMinigame.DefaultRolloverSound = minigame.DefaultRolloverSound;
		customMinigame.DefaultButtonSelected =  minigame.DefaultButtonSelected;
		customMinigame.ControllerSelectable = minigame.ControllerSelectable;
		return customMinigame;
	}

	public void BeginCustom()
	{
		var spawnPoints = Locations.ToArray();
		spawnPoints = spawnPoints.Shuffle().ToArray();

		for (int i = 0; i < LocationButtons.Length; i++)
		{
			var button = LocationButtons[i];
			var location = spawnPoints[i];
			button.OnClick.AddListener((UnityAction)(() =>
			{
				SpawnAt(location);
			}));

			button.GetComponent<ButtonAnimRolloverHandler>().DestroyImmediate();
			button.GetComponent<SpriteAnim>().DestroyImmediate();
			button.GetComponent<Animator>().DestroyImmediate();
			button.GetComponent<TextRolloverHandler>().DestroyImmediate();

			var sprite = TouAssets.AdminSprite.LoadAsset();
			var rend = button.GetComponent<SpriteRenderer>();
			rend.sprite = sprite;
			var text = button.GetComponentInChildren<TextMeshPro>();
			var stringName = TranslationController.Instance.GetSystemName(location.System);
			text.text = TranslationController.Instance.GetString(stringName);
			var component = button.gameObject.AddComponent<ButtonRolloverHandler>();
			component.Target = rend;
			component.TargetText = text;
			component.OutColor = Color.white;
			component.OverColor = Palette.AcceptedGreen;
			//component.HoverSound = (pt.RolloverSfx ? pt.RolloverSfx : this.DefaultRolloverSound);
		}

		if (GameManager.Instance != null && GameManager.Instance.IsNormal())
		{
			foreach (var networkedPlayerInfo in GameData.Instance.AllPlayers)
			{
				if (networkedPlayerInfo == null || networkedPlayerInfo.Object == null ||
				    networkedPlayerInfo.Disconnected || networkedPlayerInfo.Object.isDummy) continue;
				networkedPlayerInfo.Object.NetTransform.transform.position = new Vector2(-25f, 40f);
				networkedPlayerInfo.Object.NetTransform.Halt();
			}
		}

		StartCoroutine(RunTimer().WrapToIl2Cpp());
		ControllerManager.Instance.OpenOverlayMenu(name, null, DefaultButtonSelected, ControllerSelectable);
		PlayerControl.HideCursorTemporarily();
		ConsoleJoystick.SetMode_Menu();
	}

	public void CustomClose()
	{
		ControllerManager.Instance.CloseOverlayMenu(name);
		if (!gotButton)
		{
			LocationButtons.Random()!.ReceiveClickUp();
		}

		if (amClosing != CloseState.Closing)
		{
			if (PlayerControl.LocalPlayer)
			{
				PlayerControl.HideCursorTemporarily();
			}
			amClosing = CloseState.Closing;
			StartCoroutine(CoDestroySelf());
			return;
		}
		Destroy(gameObject);
	}

	[HideFromIl2Cpp]
	private void SpawnAt(TacticalDeploymentTweak.CustomSpawnLocation spawnPoint)
	{
		if (amClosing != CloseState.None)
		{
			return;
		}
		gotButton = true;
		PlayerControl.LocalPlayer.SetKinematic(true);
		PlayerControl.LocalPlayer.NetTransform.SetPaused(true);
		var pos = spawnPoint.Position + new Vector3(Random.RandomRange(-0.25f, 0.25f), Random.RandomRange(-0.25f, 0.25f));
		PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(pos);
		HudManager.Instance.PlayerCam.SnapToTarget();
		StopAllCoroutines();
		StartCoroutine(CoSpawnAt(PlayerControl.LocalPlayer).WrapToIl2Cpp());
	}

	[HideFromIl2Cpp]
	private IEnumerator CoSpawnAt(PlayerControl playerControl)
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		playerControl.SetKinematic(false);
		playerControl.NetTransform.SetPaused(false);
		CustomClose();
	}

	[HideFromIl2Cpp]
	private IEnumerator RunTimer()
	{
		for (var time = 10f; time >= 0f; time -= Time.deltaTime)
		{
			Text.text = TranslationController.Instance.GetString(StringNames.TimeRemaining, Mathf.CeilToInt(time));
			yield return null;
		}
		LocationButtons.Random()!.ReceiveClickUp();
	}

	[HideFromIl2Cpp]
	public IEnumerator WaitForFinish()
	{
		yield return null;
		while (amClosing == CloseState.None)
		{
			yield return null;
		}
	}
}
