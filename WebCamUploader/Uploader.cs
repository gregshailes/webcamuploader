using System;
using System.IO;
using System.Net;
using AForge.Video;
using AForge.Video.DirectShow;

using System.Drawing;
using System.Drawing.Imaging;
using System.Configuration;

using System.Net.Http;

namespace WebCamUploader
{
    public sealed class Uploader
    {

        FileSystemWatcher watcher;

        public event EventHandler Error;
        public event EventHandler FileUploaded;

        public UploaderSettings Settings;

        private VideoCaptureDevice webCam;
        private DateTime lastUploadTime = new DateTime(1900, 01, 01);
        
        public Uploader(UploaderSettings settings, EventHandler FileUploadedHandler, EventHandler errHandler)
        {
            Settings = settings;
            FileUploaded += FileUploadedHandler;
            Error += errHandler;
        }


        public void start()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if(devices.Count == 0)
            {
                throw new Exception("Camera not found");
            }

            webCam = new VideoCaptureDevice(devices[0].MonikerString);

            webCam.NewFrame += webCam_NewFrame;
            webCam.Start();
        }


        void webCam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            sendNewFrame((Bitmap)eventArgs.Frame.Clone());
        }


        void sendNewFrame(Bitmap img)
        {
            if (lastUploadTime.AddMinutes(6) < DateTime.Now && DateTime.Now.Hour >= Settings.StartHour && DateTime.Now.Hour < Settings.EndHour)
            {

                string stampString = DateTime.Now.ToString("dd MMMM yyyy HH:mm");

                Graphics g = Graphics.FromImage(img);
                g.DrawString(stampString, new Font("Arial", 10f), Brushes.Black, 3, img.Height - 17);
                g.DrawString(stampString, new Font("Arial", 10f), Brushes.White, 2, img.Height - 18);

                Upload(Settings.PostToUrl, img);
                img.Dispose();

                OnFileUploaded(new EventArgs());
                lastUploadTime = DateTime.Now;
            }
        }


        private System.IO.Stream Upload(string url, Image img)
        {
            string boundary = "---------------------------7e13702703c6";

            HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create(Settings.PostToUrl);
            httpWebRequest2.Accept = "text/html, application/xhtml+xml, image/jxr, */*";
            httpWebRequest2.ContentType = "multipart/form-data; boundary=" + boundary;
            httpWebRequest2.Method = "POST";

            Stream memStream = new System.IO.MemoryStream();

            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("--" + boundary);

            string headerTemplate = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: image/jpeg\r\n\r\n";
            string header = string.Format(headerTemplate, "datafile", "Boris.jpg");
            byte[] headerbytes = System.Text.Encoding.ASCII.GetBytes(header);

            memStream.Write(headerbytes, 0, headerbytes.Length);

            img.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);

            memStream.Write(boundarybytes, 0, boundarybytes.Length);

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\";\r\n{1}";

            string formitem = string.Format(formdataTemplate, "filename", "Boris.jpg");

            byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);

            memStream.Write(boundarybytes, 0, boundarybytes.Length);

            httpWebRequest2.ContentLength = memStream.Length;
           
            Stream requestStream = httpWebRequest2.GetRequestStream();

            memStream.Position = 0;
            byte[] tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();
            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            WebResponse webResponse2 = httpWebRequest2.GetResponse();

            return webResponse2.GetResponseStream();
        }


        public void stop()
        {
            webCam.Stop();
        }


        void OnError(WebCamUploaderErrorEventArgs error)
        {
            EventHandler handler = Error;
            if (handler != null)
            {
                handler(null, error);
            }
        }

        void OnFileUploaded(EventArgs uploaded)
        {
            EventHandler handler = FileUploaded;
            if(handler != null)
            {
                handler(null, uploaded);
            }
        }

    }

    public class WebCamUploaderErrorEventArgs : EventArgs
    {
        public WebCamUploaderErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; private set; }
    }

    public class UploaderSettings
    {
        public string PostToUrl { get; set; }
        public byte PicsToKeep { get; set; }
        public byte StartHour { get; set; }
        public byte EndHour { get; set; }

        public UploaderSettings(string postToUrl, byte picsToKeep, byte startHour, byte endHour)
        {
            PostToUrl = postToUrl;
            PicsToKeep = picsToKeep;
            StartHour = startHour;
            EndHour = endHour;
        }

    }
}
