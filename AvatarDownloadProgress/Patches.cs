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

namespace AvatarDownloadProgress
{
    [HarmonyPatch(typeof(AvatarUpdate), "Apply")]
    class PatchCVRSnappingPointUpdate
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

    [HarmonyPatch(typeof(DownloadManagerHelperFunctions), "WriteProgress")]
    class Patch_DownloadManagerHelperFunctions_WriteProgress
    {
        public static bool Prefix(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                DownloadJob job = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob match) =>
                    match.Status == DownloadJob.ExecutionStatus.Downloading &&
                    (match.Type == DownloadJob.ObjectType.World || match.Type == DownloadJob.ObjectType.Avatar));
                job.Progress = e.ProgressPercentage;
                
                //MelonLogger.Msg(job.Type + " | " + job.Progress);
            }
            catch { }

            return false;
        }
    }
}
