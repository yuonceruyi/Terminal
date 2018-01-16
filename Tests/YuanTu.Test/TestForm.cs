using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YuanTu.AutoUpdater;
using YuanTu.Consts;

namespace YuanTu.Test
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
            Log += s => richTextBox1.AppendText($"[{DateTimeCore.Now}]\n{s}\n====\n");
        }

        public bool Ret
        {
            set { richTextBox1.AppendText(value + "\n"); }
        }

        public event Action<string> Log;

        private void button2_Click(object sender, EventArgs e)
        {
            string hospitalId = ConfirmForm.Show("HospitalID", true);
            string fileName = string.Format(ConstFile.CREATEFILENAME, hospitalId);
            var config = CreateFileConfig.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
            AutoUpdater.AutoUpdater.ServerUrl = config.ServerUrl;

            foreach (var file in config.CreateFileList)
            {
                if (AutoUpdater.AutoUpdater.PathList.Contains(file.FileName))
                    continue;
                AutoUpdater.AutoUpdater.PathList.Add(file.FileName);
            }

            var list = new Server().Make(hospitalId);
            Log?.Invoke(list.Select(one => one.Name).Aggregate("", (s, two) => s + " " + two));
        }

        public string Test()
        {
            string IpComm = "http://10.122.2.116:8081/frontgateway/gateway.do?";
            //string IpComm = "http://120.26.55.58/frontgateway/gateway.do?";
            string postUrl = "cardType=2&cardNo=1231332&patientName=&guarderId=&service=yuantu.wap.query.patient.vs.info&sign_type=RSA&sign=DKrw%2bOWA45%2fAltM7V%2b54DjuL7qhJHP8kdLV3GyeoiCgYeN%2fdbSkco2u5MVDi4yk1v3M2z%2foMnWVyc6sNunwF4WobBvUXAFijxQdkgYq1Wl9h%2fDtUEKqv6EVACcTxeLOxRFFPPxhw5TbkNgI34FDYNUFQiXO940JBGI1fnetYTpc%3d&sourceCode=ZZJ&deviceInfo=&hospitalId=265&hospCode=3702010326&flowId=00999920160418033027537164&tradeTime=2016-04-18+15%3a30%3a26";
            var request = (HttpWebRequest)WebRequest.Create(new Uri(IpComm));

            request.Proxy = new WebProxy()
            {
                Address = new Uri("http://CrazyPhoenix-Office:8888")
            };

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var bts = Encoding.UTF8.GetBytes(postUrl);
            request.ContentLength = bts.Length;

            var myRequestStream = request.GetRequestStream();
            myRequestStream.Write(bts, 0, bts.Length);

            var response = (HttpWebResponse)request.GetResponse();

            var myResponseStream = response.GetResponseStream();
            var myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            var retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
    }
}