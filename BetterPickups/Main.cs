using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABI.CCK.Components;
using MelonLoader;
using DarkRift;
using Dissonance.Integrations.DarkRift2;
using Dissonance;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;

[assembly: MelonInfo(typeof(BetterPickups.Main), "BetterPickups", "1.0.0", "Ikeiwa")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace BetterPickups
{
    public class Main : MelonMod
    {
        HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.ikeiwa.betterpickups");

        public override void OnApplicationStart()
        {
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(CVRSnappingPoint), "Update")]
    class PatchCVRSnappingPointUpdate
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(CVRSnappingPoint), "LateUpdate")]
    class PatchCVRSnappingPointLateUpdate
    {
        public static void Postfix()
        {
            CVRSnappingPointManager.Update();
        }
    }
}
