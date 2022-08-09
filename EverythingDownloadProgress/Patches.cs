using System.Collections.Generic;
using System.IO;
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
using System.Reflection;

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

    [HarmonyPatch(typeof(CVRDownloadManager), nameof(CVRDownloadManager.AddDownloadJob))]
    public class Patch_CVRDownloadManager_AddDownloadJob
    {
        public static void Postfix()
        {
			Main.UpdateJobs();
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

    [HarmonyPatch(typeof(DownloadManagerHelperFunctions), nameof(DownloadManagerHelperFunctions.WriteProgress))]
    public static class Patch_DownloadManagerHelperFunctions_WriteProgress
    {
        public static bool Prefix(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                WebClient client = (WebClient)sender;
                string fileId = null;

                if (Main.webClientRequest != null)
                {
                    WebRequest request = Main.webClientRequest.GetValue(client) as WebRequest;
                    if (request != null)
                    {
                        fileId = new DirectoryInfo(request.RequestUri.AbsolutePath).Parent.Name;
                    }
                }

                if (string.IsNullOrEmpty(fileId))
                {
                    DownloadJob job = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) =>
                        match.Status == DownloadJob.ExecutionStatus.Downloading && match.Type == DownloadJob.ObjectType.World);
                    job.Progress = e.ProgressPercentage;
                }
                else
                {
                    DownloadJob job = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) =>
                        match.Status == DownloadJob.ExecutionStatus.Downloading && match.ObjectFileId == fileId);
                    job.Progress = e.ProgressPercentage;
                }
            }
            catch { }

            return false;
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
