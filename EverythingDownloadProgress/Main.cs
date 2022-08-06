using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static DownloadJob currentLocalJob;
        private GameObject itemLoadingItem;
        private TMP_Text itemLoadingStatus;

        public override void OnApplicationStart()
        {
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

        public override void OnUpdate()
        {
            if(itemLoadingItem && itemLoadingStatus)
            {
                if(CVRDownloadManager.Instance.AllDownloadJobs.Count > 0 && currentLocalJob != null && !CVRDownloadManager.Instance.IsDownloadingWorld)
                {
                    if(!itemLoadingItem.activeSelf)
                        itemLoadingItem.SetActive(true);

                    TryUpdateCurrentLocalItemPercent();

                    if(currentLocalJob.Status == DownloadJob.ExecutionStatus.DownloadComplete || 
                        currentLocalJob.Status == DownloadJob.ExecutionStatus.Error || 
                        currentLocalJob.Status == DownloadJob.ExecutionStatus.JobDone ||
                        currentLocalJob.Progress >= 99.9)
                    {
                        itemLoadingItem.SetActive(false);
                        currentLocalJob = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) => !match.IsRemoteObject && match.Status != DownloadJob.ExecutionStatus.DownloadComplete && match.Type != DownloadJob.ObjectType.World);
                    }
                }
                else
                {
                    if(itemLoadingItem.activeSelf && (currentLocalJob == null || (currentLocalJob != null && currentLocalJob.Status != DownloadJob.ExecutionStatus.Downloading) || CVRDownloadManager.Instance.IsDownloadingWorld))
                        itemLoadingItem.SetActive(false);
                }
            }
            
        }

        private void TryUpdateCurrentLocalItemPercent()
        {
            itemLoadingStatus.text = currentLocalJob.Status + " " + currentLocalJob.Type + ": " + currentLocalJob.Progress + "%";
        }
    }
}
