using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Web;
using mshtml;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace faster
{
    [ComVisible(true)]
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var client = new WebClient();

            var uri_avail = "https://reserve.cdn-apple.com/HK/zh_HK/reserve/iPhone/availability.json";

            //var uri_store = "https://reserve.cdn-apple.com/HK/zh_HK/reserve/iPhone/stores.json";
            //var str1 = client.DownloadString(uri_store);
            //var store = JsonConvert.DeserializeObject<dynamic>(str1);

            string store_id = textBoxStore.Text;
            string product_id = textBoxProduct.Text;

            while (true)
            {
                if (worker1.CancellationPending)
                    throw new Exception("取消");

                try
                {
                    var str2 = client.DownloadString(uri_avail);
                    var avail = JsonConvert.DeserializeObject<dynamic>(str2);

                    //var find = FindItem(avail, product_id, ref store_id);
                    var find = FindItemByStore(avail, store_id, product_id);

                    if (find)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(3000);
            }

            e.Result = new { StoreId = store_id, ProductId = product_id };
        }

        bool FindItemByStore(dynamic json, string store, string product)
        {
            string v = json[store][product];
            return v == "ALL";
        }

        bool FindItem(dynamic json, string product_id, ref string result)
        {
            foreach (var item in json)
            {
                string store_id = item.Name;

                if (!store_id.StartsWith("R"))
                    continue;

                string exists = (item.Value)[product_id];

                if (exists == "ALL")
                {
                    result = store_id;
                    return true;
                }
            }

            return false;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                worker1.RunWorkerAsync();
                return;
            }

            var result = e.Result as dynamic;

            string store_id = result.StoreId;
            string product_id = result.ProductId;

            buttonSearch.Text = "开始搜索";

            webBrowser1.Navigate("https://reserve.cdn-apple.com/HK/zh_HK/reserve/iPhone/availability?channel=1");

            this.Activate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.ObjectForScripting = this;
        }

        public void sendKeys(string keys)
        {
            //发送按键消息，防止登录时提示“恢复Apple ID”
            SendKeys.SendWait(keys);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Console.WriteLine(e.Url);

            //连接几个操作步骤

            if (e.Url.LocalPath == "/HK/zh_HK/reserve/iPhone/availability")
            {
                Console.WriteLine("第一步");
                //this.BeginInvoke(new EventHandler(buttonStep1_Click));
                //Thread.Sleep(1000);
                //buttonStep1_Click(null, null);

                Task.Factory.StartNew(new Action(() =>
                {
                    Thread.Sleep(1000);

                    this.Invoke(new EventHandler(buttonStep1_Click));
                }));
            }

            if (e.Url.LocalPath == "/IDMSWebAuth/signin")
            {
                Console.WriteLine("父窗口");
            }

            if (e.Url.LocalPath == "/appleauth/auth/signin")
            {
                Console.WriteLine("第二步");

                Task.Factory.StartNew(new Action(() =>
                {
                    Thread.Sleep(1000);

                    this.Invoke(new EventHandler(buttonStep2_Click));
                }));
            }

            if (e.Url.LocalPath == "/HK/zh_HK/reserve/iPhone")
            {
                Console.WriteLine("第三步");

                Task.Factory.StartNew(new Action(() =>
                {
                    Thread.Sleep(1000);

                    this.Invoke(new EventHandler(buttonStep3_Click));
                }));
            }
        }

        private void buttonStep1_Click(object sender, EventArgs e)
        {
            var script = textBoxScript1.Text;

            var args = new object[] { script };

            webBrowser1.Document.InvokeScript("eval", args);

            webBrowser1.Document.InvokeScript("step1", new object[] {
                textBoxStore.Text,
                textBoxFamily.Text,
                textBoxCapacity.Text,
                textBoxProduct.Text
            });
        }

        private void buttonStep2_Click(object sender, EventArgs e)
        {
            var script = textBoxScript2.Text;
            var args = new object[] { script };

            webBrowser1.Document.Window.Frames[0].Document.InvokeScript("eval", args);

            this.Activate();//用于使SendKeys生效。没有焦点的话，无法发送。

            webBrowser1.Document.Window.Frames[0].Document.InvokeScript("step2", new object[] {
                textBoxUser.Text,
                textBoxPwd.Text
            });
        }

        private void buttonStep3_Click(object sender, EventArgs e)
        {
            var script = textBoxScript3.Text;
            var args = new object[] { script };

            webBrowser1.Document.InvokeScript("eval", args);

            webBrowser1.Document.InvokeScript("step3", new object[] {
                textBoxPhoneNumber.Text
            });
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (worker1.IsBusy)
            {
                buttonSearch.Text = "开始搜索";
                worker1.CancelAsync();
            }
            else
            {
                buttonSearch.Text = "停止搜索";

                worker1.RunWorkerAsync();
            }
        }
    }
}
