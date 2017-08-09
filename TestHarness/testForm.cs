using System;
using System.Windows.Forms;
using WebCamUploader;
using System.Net;

namespace TestHarness
{
    public partial class testForm : Form
    {
        public testForm()
        {
            InitializeComponent();
        }

        Uploader Uploader;

        private void button1_Click(object sender, EventArgs e)
        {
            UploaderSettings settings = new UploaderSettings
            (
                @"https://shailes.herokuapp.com/boriscam/upload",
                10,
                0,
                24
            );

            
            Uploader = new WebCamUploader.Uploader
            (
                settings,    
                WebCamUploader_FileUploaded,
                WebCamUploader_Error
            );
            stopButton.Enabled = true;
            startButton.Enabled = false;
            Uploader.start();
        }

        private void WebCamUploader_FileUploaded(object sender, EventArgs e)
        {
        }

        private void WebCamUploader_Error(object sender, EventArgs e)
        {
            MessageBox.Show(((WebCamUploaderErrorEventArgs)e).ErrorMessage);
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            Uploader.stop();
            stopButton.Enabled = false;
            startButton.Enabled = true;
        }
    }
}
