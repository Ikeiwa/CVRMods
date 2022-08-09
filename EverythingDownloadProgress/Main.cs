using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ABI_RC.Core.IO;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using ABI_RC.Core.Player;
using TMPro;

[assembly: MelonInfo(typeof(EverythingDownloadProgress.Main), "EverythingDownloadProgress", "1.0.0", "Ikeiwa")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
[assembly: MelonIncompatibleAssemblies("WorldDownloadPercentage")]
namespace EverythingDownloadProgress
{
    public class Main : MelonMod
    {
        public static MelonPreferences_Category settingsCategory;
        public static MelonPreferences_Entry<bool> showDebug;

        HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.ikeiwa.EverythingDownloadProgress");
        private static AssetBundle barAssetBundle;
        public static GameObject downloadBarObj;
        public static GameObject spinnerObj;

        public static FieldInfo webClientRequest;
        public static DownloadJob currentAvatarJob;
        public static List<DownloadJob> currentPropsJob;
        public static DownloadJob currentPropJob;
        private GameObject itemLoadingItem;
        private TMP_Text itemLoadingStatus;
        private int lastDownloadAmount;

        public override void OnApplicationStart()
        {
            webClientRequest = typeof(WebClient).GetField("m_WebRequest", BindingFlags.NonPublic | BindingFlags.Instance);

            settingsCategory = MelonPreferences.CreateCategory("EverythingDownloadProgress");
            showDebug = settingsCategory.CreateEntry("Show Debug", false);

            barAssetBundle = AssetBundle.LoadFromMemory(Properties.Resources.downloadbar);
            if (barAssetBundle)
            {
                downloadBarObj = barAssetBundle.LoadAsset<GameObject>("DownloadBarRoot");
                spinnerObj = barAssetBundle.LoadAsset<GameObject>("Spinner");
            }
            else
            {
                MelonLogger.Msg("Error loading assetbundle");
            }

            currentPropsJob = new List<DownloadJob>();

            harmony.PatchAll();
        }

        public override void OnGUI()
        {
            if (showDebug.Value)
            {
                for(int i = 0; i < CVRDownloadManager.Instance.AllDownloadJobs.Count; i++)
                {
                    DownloadJob job = CVRDownloadManager.Instance.AllDownloadJobs[i];

                    if(job.Type == DownloadJob.ObjectType.World && job.JoinOnCompletion)
                        GUI.Label(new Rect(10,10+i*15,1000,90), job.ObjectId + " - " + job.Type + " - Join On Complete - " + job.Status + " : " + job.Progress);
                    else
                        GUI.Label(new Rect(10,10+i*15,1000,90),$"{job.ObjectId} - {job.Type} - Remote:{job.IsRemoteObject} - {job.Status} : {job.Progress}");
                }
            }
            
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if(buildIndex == 3 && (!itemLoadingItem || !itemLoadingStatus))
            {
                if(!itemLoadingItem)
                    itemLoadingItem = GameObject.Instantiate(HudOperations.Instance.worldLoadingItem,HudOperations.Instance.worldLoadingItem.transform.parent,false);
                if (itemLoadingItem)
                {
                    itemLoadingItem.transform.SetParent(HudOperations.Instance.worldLoadingItem.transform.parent);
                    itemLoadingStatus = itemLoadingItem.GetComponentInChildren<TMP_Text>();
                }
            }
        }

        public static void UpdateJobs()
        {
            currentAvatarJob = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) => !match.IsRemoteObject &&
            match.Type == DownloadJob.ObjectType.Avatar);
            
            currentPropJob = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) => match.Type == DownloadJob.ObjectType.Prop && match.Status == DownloadJob.ExecutionStatus.Downloading);
            currentPropsJob = CVRDownloadManager.Instance.AllDownloadJobs.FindAll((DownloadJob match) => match.Type == DownloadJob.ObjectType.Prop);
        }

        public override void OnUpdate()
        {
            if(itemLoadingItem && itemLoadingStatus)
            {
                if(CVRDownloadManager.Instance.AllDownloadJobs.Count > 0 && !CVRDownloadManager.Instance.IsDownloadingWorld && (currentAvatarJob != null || currentPropJob != null || currentPropsJob.Count > 0))
                {
                    if(!itemLoadingItem.activeSelf)
                        itemLoadingItem.SetActive(true);

                    TryUpdateCurrentLocalItemPercent();
                }
                else if(itemLoadingItem.activeSelf)
                {
                    itemLoadingItem.SetActive(false);
                }

                if(CVRDownloadManager.Instance.AllDownloadJobs.Count != lastDownloadAmount)
                {
                    lastDownloadAmount = CVRDownloadManager.Instance.AllDownloadJobs.Count;
                    UpdateJobs();
                }
            }
        }

        private void TryUpdateCurrentLocalItemPercent()
        {
            string avatarString = "";
            if(currentAvatarJob != null)
                avatarString = currentAvatarJob.Status + " Avatar" + ": " + currentAvatarJob.Progress + "%\n";

            string propsString = "";
            if(currentPropsJob.Count > 0)
            {
                if(currentPropJob != null)
                    propsString = currentPropJob.Status + " " + currentPropsJob.Count + " Prop" + (currentPropsJob.Count>1?"s":"") + ": " + currentPropJob.Progress + "%";
                else
                    propsString = "Waiting " + currentPropsJob.Count + " Prop"+(currentPropsJob.Count>1?"s":"");
            }

            itemLoadingStatus.text = avatarString+propsString;
        }
    }
}
