using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABI_RC.Core.IO;
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
        private static AssetBundle barAssetBundle;
        public static GameObject downloadBarObj;
        public static GameObject spinnerObj;

        public override void OnApplicationStart()
        {
            
            MelonLogger.Msg(Properties.Resources.downloadbar.Length);

            barAssetBundle = AssetBundle.LoadFromMemory(Properties.Resources.downloadbar);
            if (barAssetBundle)
            {
                downloadBarObj = barAssetBundle.LoadAsset<GameObject>("DownloadBarRoot");
                spinnerObj = barAssetBundle.LoadAsset<GameObject>("Spinner");
                if(downloadBarObj && spinnerObj)
                    MelonLogger.Msg("DownloadBar Loaded");
            }
            else
            {
                MelonLogger.Msg("Error loading assetbundle");
            }

            harmony.PatchAll();
        }
    }
}
