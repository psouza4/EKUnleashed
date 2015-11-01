using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace EKUnleashed
{
    public partial class frmEKARCCAPTCHA : Form
    {
        public frmEKARCCAPTCHA()
        {
            InitializeComponent();
        }

        public CookieContainer cc = null;

        private void frmEKARCCAPTCHA_Shown(object sender, EventArgs e)
        {
            this.txtAnswer.Focus();
        }

        public void SetImage(string img_url)
        {
            CookieAwareWebClient wc = new CookieAwareWebClient();
            wc.m_container = cc;
            Image i = Utils.GetImageFromBytes(Comm.DecompressBytes(wc.DownloadData(img_url)));

            Utils.FitImageNicely(ref this.picCAPTCHA, i);
        }

        public string Answer
        {
            get
            {
                return this.txtAnswer.Text.Trim();
            }
        }

        public class CookieAwareWebClient : System.Net.WebClient
        {
            public System.Net.CookieContainer m_container = new System.Net.CookieContainer();

            protected override System.Net.WebRequest GetWebRequest(Uri address)
            {
                System.Net.WebRequest request = base.GetWebRequest(address);
                System.Net.HttpWebRequest webRequest = request as System.Net.HttpWebRequest;
                if (webRequest != null)
                {
                    webRequest.CookieContainer = m_container;
                }
                return request;
            }
        }
    }
}
