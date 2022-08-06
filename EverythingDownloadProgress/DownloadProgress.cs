using ABI_RC.Core.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABI_RC.Core.Player;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EverythingDownloadProgress
{
    public class DownloadProgress : MonoBehaviour
    {
        public string downloadId = null;
        public CVRPlayerEntity player;
        private DownloadJob downloadJob;
        private TextMeshProUGUI textProgress;
        private Image loadingBarImage;
        private GameObject spinner;
        private Transform spinnerAnim;

        void Start()
        {
            if (!Main.downloadBarObj)
            {
                MelonLogger.Msg("Download Bar GameObject not loaded");
                Destroy(gameObject);
                return;
            }
            GameObject bar = Instantiate(Main.downloadBarObj, transform, false);
            bar.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            spinner = Instantiate(Main.spinnerObj, player.AvatarHolder.transform, false);
            spinnerAnim = spinner.transform.Find("SpinnerAnim");

            textProgress = bar.transform.Find("Progress").GetComponent<TextMeshProUGUI>();
            textProgress.text = "In Queue";

            loadingBarImage = bar.transform.Find("ProgressMask").GetComponent<Image>();
            loadingBarImage.fillAmount = 0;

            if(downloadId != null)
            {
                downloadJob = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob job) => job.ObjectId == downloadId);
            }
        }

        void Update()
        {
            if (player == null)
            {
                return;
            }

            if(downloadJob != null)
            {
                switch (downloadJob.Status)
                {
                    case DownloadJob.ExecutionStatus.Downloading:
                        textProgress.text = "Downloading " + downloadJob.Progress + "%";
                        loadingBarImage.fillAmount = downloadJob.Progress / 100;
                        break;
                    case DownloadJob.ExecutionStatus.Error:
                    case DownloadJob.ExecutionStatus.JobDone: 
                        Destroy(spinner);
                        Destroy(gameObject); 
                        break;
                    case DownloadJob.ExecutionStatus.DownloadComplete:
                        textProgress.text = "Importing";
                        loadingBarImage.fillAmount = 1;
                        if (player.AvatarHolder && player.AvatarHolder.transform.childCount > 0 && player.AvatarHolder.activeSelf && player.PuppetMaster.avatarObject)
                        {
                            Destroy(spinner);
                            Destroy(gameObject);
                        }
                        break;
                }
            }
            else
            {
                Destroy(spinner);
                Destroy(gameObject);
            }

            spinner.transform.position = (player.AvatarHolder.transform.position + transform.parent.position) / 2;
            spinner.transform.LookAt(Camera.main.transform);
            spinnerAnim.Rotate(new Vector3(0,0,Time.deltaTime*180),Space.Self);
        }
    }
}
