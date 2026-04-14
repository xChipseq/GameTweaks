using System.Linq;
using System.Text;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GameTweaks.Patches;

[HarmonyPatch]
public static class TweaksTabPatches
{
    private static bool infoPanel { get; set; }
    private static PassiveButton? tweaksInfoButton { get; set; }
    private static TextMeshPro? tweaksTaskText { get; set; }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    private static void StartPostfix(HudManager __instance)
    {
        // Tweaks button
        var infoButtonObj = Object.Instantiate(__instance.SettingsButton, __instance.transform.Find("TaskDisplay/ProgressTracker"));
        infoButtonObj.name = "TweaksInfoButton";
        infoButtonObj.GetComponent<AspectPosition>().DestroyImmediate();
        infoButtonObj.transform.localPosition = new Vector3(4.3f, -0.05f, 0f);
        infoButtonObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        infoButtonObj.transform.Find("Background").gameObject.Destroy();
        infoButtonObj.gameObject.SetActive(false);

        tweaksInfoButton = infoButtonObj.GetComponent<PassiveButton>();
        tweaksInfoButton.OnClick = new Button.ButtonClickedEvent();
        tweaksInfoButton.OnClick.AddListener((UnityAction)ToggleTab);

        // Tweaks task text
        var taskText = __instance.TaskPanel.taskText;
        tweaksTaskText =
            Object.Instantiate(taskText, taskText.transform.parent);
        tweaksTaskText.name = "TweaksTaskText";
        tweaksTaskText.transform.localPosition = taskText.transform.localPosition;
        tweaksTaskText.gameObject.SetActive(false);

        infoPanel = false;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.Update))]
    private static bool UpdatePrefix(TaskPanelBehaviour __instance)
    {
        if (__instance.name == "RolePanel") // we leave mira's role panel alone
        {
            __instance.transform.Find("TweaksTaskText")?.gameObject.Destroy();
            __instance.taskText.gameObject.SetActive(true);
            return true;
        }

        var manager = GameTweaksManager.Instance;
        if (manager == null || !manager.ActiveTweaks.Any() || MeetingHud.Instance != null)
        {
            infoPanel = false;
            tweaksInfoButton?.gameObject.SetActive(false);
        }
        else
        {
            tweaksInfoButton?.gameObject.SetActive(true);
            var ordered = manager.ActiveTweaks.OrderBy(x => x.Name).ToArray();
            var builder = new StringBuilder("<b>Active Tweaks</b>\n");
            foreach (var tweak in ordered)
            {
                builder.Append($"<color=#{tweak.Color.ToHtmlStringRGBA()}>{tweak.Name}</color>");
                if (manager.TweaksAddedLater.Contains(tweak))
                    builder.Append(" <color=green><b>❖</b></color>");
                builder.Append('\n');
            }
            tweaksTaskText?.text = builder.ToString();
        }

        // we need to patch this whole method to make it compute both text sizes smh
        tweaksTaskText!.gameObject.SetActive(infoPanel);
        __instance.taskText.gameObject.SetActive(!infoPanel);
        var text = infoPanel && tweaksTaskText != null ? tweaksTaskText : __instance.taskText;
        __instance.background.transform.localScale = ((text.textBounds.size.x > 0f) ? new Vector3(text.textBounds.size.x + 0.2f, text.textBounds.size.y + 0.2f, 1f) : Vector3.zero);
        var vector = __instance.background.sprite.bounds.extents;
        vector.y = -vector.y;
        vector = vector.Mul(__instance.background.transform.localScale);
        __instance.background.transform.localPosition = vector;
        var vector2 = __instance.tab.sprite.bounds.extents;
        vector2 = vector2.Mul(__instance.tab.transform.localScale);
        vector2.y = -vector2.y;
        vector2.x += vector.x * 2f;
        __instance.tab.transform.localPosition = vector2;
        if (GameManager.Instance == null)
        {
            return false;
        }
        if (GameManager.Instance.IsHideAndSeek())
        {
            __instance.closedPosition = __instance.closedPosition with { y = 1.6f };
            __instance.openPosition = __instance.openPosition with { y = 1.6f };
        }
        else
        {
            __instance.closedPosition = __instance.closedPosition with { y = 0.6f };
            __instance.openPosition = __instance.openPosition with { y = 0.6f };
        }
        __instance.closedPosition = __instance.closedPosition with { x = -__instance.background.sprite.bounds.size.x * __instance.background.transform.localScale.x };
        __instance.timer = __instance.open
            ? Mathf.Min(1f, __instance.timer + Time.deltaTime / __instance.animationTimeSeconds)
            : Mathf.Max(0f, __instance.timer - Time.deltaTime / __instance.animationTimeSeconds);
        var vector3 = new Vector3(Mathf.SmoothStep(__instance.closedPosition.x, __instance.openPosition.x, __instance.timer), Mathf.SmoothStep(__instance.closedPosition.y, __instance.openPosition.y, __instance.timer), __instance.openPosition.z);
        __instance.transform.localPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop, vector3);
        return false;
    }

    private static void ToggleTab()
    {
        infoPanel = !infoPanel;
        tweaksInfoButton?.SelectButton(infoPanel);
    }
}