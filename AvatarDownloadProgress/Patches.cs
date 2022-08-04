using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using ABI_RC.Core.Networking.Jobs;
using DarkRift;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
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
                    loadingBar.transform.SetParent(playerEntity.PlayerNameplate.s_Nameplate.transform);

                    DownloadProgress progress = loadingBar.AddComponent<DownloadProgress>();
                    progress.downloadId = id;
					
				}
			}
        }
    }
}
