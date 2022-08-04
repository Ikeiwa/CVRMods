using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using HarmonyLib;


[assembly: MelonInfo(typeof(AvatarDownloadProgress.Main), "AvatarDownloadProgress", "1.0.0", "Ikeiwa")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace AvatarDownloadProgress
{
    public class Main : MelonMod
    {
        HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.ikeiwa.AvatarDownloadProgress");

        public override void OnApplicationStart()
        {
            harmony.PatchAll();
        }
    }
}
