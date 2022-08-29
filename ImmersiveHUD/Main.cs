using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using ABI_RC.Core.UI;

[assembly: MelonInfo(typeof(ImmersiveHUD.Main), "ImmersiveHUD", "1.0.0", "Ikeiwa")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace ImmersiveHUD
{
    public class Main : MelonMod
    {
        public static Main instance;

        public static MelonPreferences_Category settingsCategory;
        public static MelonPreferences_Entry<int> hideDelay;
        public static MelonPreferences_Entry<bool> showOnMicOn;
        public static MelonPreferences_Entry<bool> showOnMicOff;
        public static MelonPreferences_Entry<bool> showOnFriendRequest;
        public static MelonPreferences_Entry<bool> showOnInvite;
        public static MelonPreferences_Entry<bool> showOnMessage;

        private object currentHideTimer;
        private bool forceShow;

        public override void OnApplicationStart()
        {
            if(instance == null)
                instance = this;

            settingsCategory = MelonPreferences.CreateCategory("ImmersiveHUD");
            hideDelay = settingsCategory.CreateEntry("Hide delay (seconds)", 10);
            showOnMicOn = settingsCategory.CreateEntry("Show on unmute", true);
            showOnMicOff = settingsCategory.CreateEntry("Show on mute", true);
            showOnFriendRequest = settingsCategory.CreateEntry("Show on friend request", true);
            showOnInvite = settingsCategory.CreateEntry("Show on invite", true);
            showOnMessage = settingsCategory.CreateEntry("Show on system message", true);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if(buildIndex == 3)
                StartHideTimer();
        }

        public void ForceShowHUD()
        {
            forceShow = true;
            CohtmlHud.Instance.RestoreHud();
        }

        public void StartHideTimer(bool force = false)
        {
            if(force) forceShow = false;
            else if(forceShow) return;

            if(currentHideTimer != null)
                MelonCoroutines.Stop(currentHideTimer);
            currentHideTimer = MelonCoroutines.Start(HideAfterDelay(hideDelay.Value));
        }

        private IEnumerator HideAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);
            CohtmlHud.Instance.HideHud();
        }
    }
}
