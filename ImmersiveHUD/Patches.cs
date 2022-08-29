using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.UI;
using HarmonyLib;

namespace ImmersiveHUD
{
    [HarmonyPatch(typeof(CohtmlHud), "UpdateNotifierStatus")]
    class Patch_CohtmlHud_UpdateNotifierStatus
    {
        private static int lastFriendCount = 0;
        private static int lastInviteCount = 0;

        public static void Postfix()
        {
            if (Main.showOnFriendRequest.Value && ViewManager.Instance.FriendRequests.Count > lastFriendCount)
                Main.instance.StartHideTimer();
            else if (Main.showOnInvite.Value && ViewManager.Instance.Invites.Count > lastInviteCount)
                Main.instance.StartHideTimer();

            lastFriendCount = ViewManager.Instance.FriendRequests.Count;
            lastInviteCount = ViewManager.Instance.Invites.Count;
        }
    }

    [HarmonyPatch(typeof(CohtmlHud), nameof(CohtmlHud.UpdateMicStatus))]
    class Patch_CohtmlHud_UpdateMicStatus
    {
        public static void Postfix(bool active)
        {
            if(Main.showOnMicOn.Value && active)
                Main.instance.StartHideTimer();
            else if(Main.showOnMicOff.Value && !active)
                Main.instance.StartHideTimer();
        }
    }

    [HarmonyPatch(typeof(CohtmlHud), nameof(CohtmlHud.SelectPropToSpawn))]
    class Patch_CohtmlHud_SelectPropToSpawn
    {
        public static void Postfix()
        {
            Main.instance.ForceShowHUD();
        }
    }

    [HarmonyPatch(typeof(CohtmlHud), nameof(CohtmlHud.ClearPropToSpawn))]
    class Patch_CohtmlHud_ClearPropToSpawn
    {
        public static void Postfix()
        {
            Main.instance.StartHideTimer(true);
        }
    }

    [HarmonyPatch(typeof(CohtmlHud), nameof(CohtmlHud.ViewDropText), new Type[] { typeof(string), typeof(string) })]
    class Patch_CohtmlHud_ViewDropText_1
    {
        public static void Postfix()
        {
            if(Main.showOnMessage.Value)
                Main.instance.StartHideTimer();
        }
    }

    [HarmonyPatch(typeof(CohtmlHud), nameof(CohtmlHud.ViewDropText), new Type[] { typeof(string), typeof(string), typeof(string) })]
    class Patch_CohtmlHud_ViewDropText_2
    {
        public static void Postfix()
        {
            if(Main.showOnMessage.Value)
                Main.instance.StartHideTimer();
        }
    }

    [HarmonyPatch(typeof(CohtmlHud), nameof(CohtmlHud.ViewDropTextImmediate), new Type[] { typeof(string), typeof(string), typeof(string) })]
    class Patch_CohtmlHud_ViewDropTextImmediate
    {
        public static void Postfix()
        {
            if(Main.showOnMessage.Value)
                Main.instance.StartHideTimer();
        }
    }
}
