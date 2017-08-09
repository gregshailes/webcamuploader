using System;
using System.Diagnostics;
using System.ServiceProcess;
using WebCamUploader;
using System.Net;
using System.Configuration;

using System.IO;

namespace WebCamUploadService
{
    public partial class WebCamUploadService : ServiceBase
    {

        Uploader Uploader;

        public WebCamUploadService()
        {
            InitializeComponent();

            UploaderSettings settings = new UploaderSettings
            (
                Properties.Settings.Default.PostToUrl,
                Properties.Settings.Default.PicsToKeep,
                Properties.Settings.Default.StartHour,
                Properties.Settings.Default.EndHour
            );

            Uploader = new Uploader
            (
                settings,
                WebCamUploader_FileUploaded,
                WebCamUploader_Error
            );
        }

        private void WebCamUploader_FileUploaded(object sender, EventArgs e)
        {
            // do nothing.
        }

        private void WebCamUploader_Error(object sender, EventArgs e)
        {
            // Record the error in the event log. We're a service, there's not much else we can do.
            using (EventLog log = new EventLog("Application"))
            {
                log.Source = "WebCamUploadService";
                log.WriteEntry("Error uploading: " + ((WebCamUploaderErrorEventArgs)e).ErrorMessage);
            }
        }

        protected override void OnStart(string[] args)
        {
            Uploader.start();
        }

        protected override void OnStop()
        {
            Uploader.stop();
        }
    }
}
