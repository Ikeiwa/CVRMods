using ABI_RC.Core.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace AvatarDownloadProgress
{
    public class DownloadProgress : MonoBehaviour
    {
        public string downloadId = null;
        public DownloadJob downloadJob;
        public TextMeshProUGUI text;
        public bool startedDownloading;

        void Start()
        {
            text = gameObject.AddComponent<TextMeshProUGUI>();
            text.text = "Downloading 0%";
            if(downloadId != null)
            {
                downloadJob = CVRDownloadManager.Instance.AllDownloadJobs.Find((DownloadJob job) => job.ObjectId == downloadId);
                startedDownloading = true;
            }
        }

        void Update()
        {
            if(downloadJob != null)
            {
                switch (downloadJob.Status)
                {
                    case DownloadJob.ExecutionStatus.Waiting:
                        text.text = "Waiting"; break;
                    case DownloadJob.ExecutionStatus.Downloading:
                        text.text = "Downloading " + Mathf.RoundToInt(downloadJob.Progress*100) + "%"; break;
                    case DownloadJob.ExecutionStatus.Instantiating:
                        text.text = "Loading " + Mathf.RoundToInt(downloadJob.Progress*100) + "%"; break;
                    case DownloadJob.ExecutionStatus.Error:
                    case DownloadJob.ExecutionStatus.JobDone: Destroy(gameObject); break;
                }
            }
        }
    }
}
