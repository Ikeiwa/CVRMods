using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ABI_RC.Core.IO;
using HarmonyLib;
using ABI_RC.Core.Networking.Jobs;
using DarkRift;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using MelonLoader;
using UnityEngine;
using ABI_RC.Core.Base;

namespace EverythingDownloadProgress
{
    [HarmonyPatch(typeof(AvatarUpdate), nameof(AvatarUpdate.Apply))]
    class PatchAvatarUpdateApply
    {
        public static void Postfix(Message message)
        {
            using (DarkRiftReader reader = message.GetReader())
			{
				string ownerId = reader.ReadString();
				string id = reader.ReadString();
				
                CVRPlayerEntity playerEntity = CVRPlayerManager.Instance.NetworkPlayers.Find((CVRPlayerEntity players) => players.Uuid == ownerId);
				if (ownerId != MetaPort.Instance.ownerId && playerEntity.PlayerObject != null)
				{
                    DownloadProgress oldProgress = playerEntity.PlayerNameplate.s_Nameplate.GetComponentInChildren<DownloadProgress>();
                    if (oldProgress)
                        Object.Destroy(oldProgress.gameObject);

                    GameObject loadingBar = new GameObject("LoadingBar");
                    loadingBar.transform.SetParent(playerEntity.PlayerNameplate.s_Nameplate.transform.GetChild(0).transform);
                    loadingBar.transform.localScale = Vector3.one;
                    loadingBar.transform.localPosition = Vector3.zero;
                    loadingBar.transform.localRotation = Quaternion.identity;

                    DownloadProgress progress = loadingBar.AddComponent<DownloadProgress>();
                    progress.downloadId = id;
                    progress.player = playerEntity;
                }
			}
        }
    }

    [HarmonyPatch(typeof(CVRDownloadManager), nameof(CVRDownloadManager.TryGetCurentJoiningWorldDownloadPercentage))]
    public class Patch_CVRDownloadManager_TryGetCurentJoiningWorldDownloadPercentage
    {
        public static bool Prefix(ref float __result)
        {
			try
			{
				__result = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) => match.Type == DownloadJob.ObjectType.World && match.Status == DownloadJob.ExecutionStatus.Downloading).Progress;
			}
			catch
			{
				__result = 0f;
			}
            return false;
        }
    }

    [HarmonyPatch(typeof(DownloadManagerHelperFunctions), nameof(DownloadManagerHelperFunctions.GetFolderPath))]
    public class Patch_DownloadManagerHelperFunctions_GetFolderPath
    {
        public static string lastObjectId = null;
        public static string lastObjectFileId = null;

        public static void Prefix(DownloadJob.ObjectType t, string objectId,string fileId)
        {
            lastObjectId = objectId;
            lastObjectFileId = fileId;
            //MelonLogger.Msg(lastObjectId + " | " + lastObjectFileId);
        }
    }

    public static class CustomDownloadManagerHelperFunctions
    {
        public static void WriteProgress(object sender, DownloadProgressChangedEventArgs e, DownloadJob job)
        {
            try
            {
                /*DownloadJob job = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) =>
                    match.Status == DownloadJob.ExecutionStatus.Downloading && match.ObjectId == objectId);*/

                if(job != null)
                {
                    job.Progress = e.ProgressPercentage;
                    //MelonLogger.Msg(job.ObjectId + " | " + job.Type + " | " + job.Progress);
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(WebClient), nameof(WebClient.DownloadFileTaskAsync), new System.Type[] {typeof(string), typeof(string) })]
    public class Patch_WebClient_DownloadFileTaskAsync
    {
        public static void Prefix(WebClient __instance, string address, string fileName)
        {
            string lastObjectId = string.Copy(Patch_DownloadManagerHelperFunctions_GetFolderPath.lastObjectId);
            string lastObjectFileId = string.Copy(Patch_DownloadManagerHelperFunctions_GetFolderPath.lastObjectFileId);
            //MelonLogger.Msg(address + " | " + fileName + " | " + lastObjectId + " | " + lastObjectFileId);

            if(address.Contains(lastObjectId) && address.Contains(lastObjectFileId))
            {
                DownloadJob job = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) => match.ObjectId == lastObjectId);

                if(job != null)
                {
                    if(!job.IsRemoteObject && job.Type != DownloadJob.ObjectType.World && (Main.currentLocalJob == null || (Main.currentLocalJob != null && Main.currentLocalJob.Status == DownloadJob.ExecutionStatus.DownloadComplete)))
                    {
                        Main.currentLocalJob = job;
                    }
                    //MelonLogger.Msg(job.ObjectId);
                    __instance.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => { CustomDownloadManagerHelperFunctions.WriteProgress(sender,e,job); };
                }

            }
        }
    }

    [HarmonyPatch(typeof(HudOperations), nameof(HudOperations.LoadWorldIndicator))]
    public class Patch_HudOperations_LoadWorldIndicator
    {
        public static bool Prefix(bool reset, int stage, float value)
        {
            switch (stage)
            {
                case 0:
                    HudOperations.Instance.worldLoadStatus.text = "Verifying World Version";
                    break;
                case 1:
                    HudOperations.Instance.worldLoadStatus.text = "Downloading World: " + CVRDownloadManager.Instance.TryGetCurentJoiningWorldDownloadPercentage() + "%";
                    break;
                case 2:
                    if (value > 0f)
                    {
                        HudOperations.Instance.worldLoadStatus.text = "Loading World Bundle: " + Mathf.RoundToInt(value * 100f) + "%";
                    }
                    else
                    {
                        HudOperations.Instance.worldLoadStatus.text = "Loading World Bundle";
                    }
                    break;
            }
            if (reset)
            {
                HudOperations.Instance.worldLoadStatus.text = string.Empty;
            }
            return false;
        }
    }
}
