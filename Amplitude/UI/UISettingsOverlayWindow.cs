using System;
using Amplitude.Framework.Overlay;
using UnityEngine;

namespace Amplitude.UI
{
	public class UISettingsOverlayWindow : FloatingWindow, IPopupWindowWithAdjustableWindowHeight
	{
		protected override void OnDrawWindowClientArea(int instanceId)
		{
			using (new GUILayout.VerticalScope("Widget.ClientArea"))
			{
				SettingEntryField(ref UISettings.EnableRender);
				SettingEntryField(ref UISettings.EnablePerformanceAlertFeedback);
				SettingEntryField(ref UISettings.BudgetPerFrameWarning, OnBudgetPerFrameChanged);
				SettingEntryField(ref UISettings.BudgetPerFrameCritical, OnBudgetPerFrameChanged);
				SettingEntryField(ref UISettings.EnableVerboseHierarchy);
				SettingEntryField(ref UISettings.EnableVerboseRendering);
				SettingEntryField(ref UISettings.EnableVerboseInteractivity);
				SettingEntryField(ref UISettings.EnableVerboseAnimations);
				SettingEntryField(ref UISettings.EnableVerboseStyles);
				SettingEntryField(ref UISettings.EnableVerboseWindows);
				SettingEntryField(ref UISettings.EnableVerboseTooltips);
				SettingEntryField(ref UISettings.EnableVerboseStamps);
			}
		}

		private void SettingEntryField(ref UISettings.Entry<bool> entry)
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(entry.Name, GUILayout.Width(160f));
				GUILayout.FlexibleSpace();
				if (GUILayout.Toggle(!entry.Value, "Disabled", "PopupWindow.Button", GUILayout.Width(80f)) != !entry.Value)
				{
					entry.Value = false;
				}
				if (GUILayout.Toggle(entry.Value, "Enabled", "PopupWindow.Button", GUILayout.Width(80f)) != entry.Value)
				{
					entry.Value = true;
				}
			}
		}

		private void SettingEntryField(ref UISettings.Entry<float> entry, Action onChanged)
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(entry.Name, GUILayout.Width(160f));
				GUILayout.FlexibleSpace();
				if (float.TryParse(GUILayout.TextField(entry.Value.ToString(), GUILayout.Width(160f)), out var result))
				{
					entry.Value = result;
					onChanged?.Invoke();
				}
			}
		}

		private void OnBudgetPerFrameChanged()
		{
		}
	}
}
