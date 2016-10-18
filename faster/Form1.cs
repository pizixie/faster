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
            var uri_avail = "https://reserve.cdn-apple.com/HK/zh_HK/reserve/iPhone/availability.json";

            var uri_store = "https://reserve.cdn-apple.com/HK/zh_HK/reserve/iPhone/stores.json";

            var client = new WebClient();

            var str1 = client.DownloadString(uri_store);
            var store = JsonConvert.DeserializeObject<dynamic>(str1);

            string store_id = string.Empty;
            string buy_id = string.Empty;

            while (true)
            {
                try
                {
                    var str2 = client.DownloadString(uri_avail);
                    var avail = JsonConvert.DeserializeObject<dynamic>(str2);

                    //buy_id = textBoxBuy.Text;

                    var find = FindItem(avail, buy_id, ref store_id);

                    if (find)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(1000);
            }

            e.Result = new { StoreId = store_id, BuyId = buy_id };
        }

        bool FindItem(dynamic json, string buy_id, ref string result)
        {
            foreach (var item in json)
            {
                string store_id = item.Name;

                if (!store_id.StartsWith("R"))
                    continue;

                string exists = (item.Value)[buy_id];

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
            var result = e.Result as dynamic;

            string store_id = result.StoreId;
            var buy_id = result.BuyId;

            /*
$('#selectStore').val('R428').trigger('change');

function select(){
    $('#selectSubfamily').val('10143').trigger('change');
    $('input[value="256GB"]').attr('checked','checked').trigger('change');
    $('input[value="MN8V2ZP/A"]').attr('checked','checked').trigger('change');
    $('button[name="submit"]').trigger('click');
}

setTimeout(select,200);

             */
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //backgroundWorker1.RunWorkerAsync();

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
            }

            if (e.Url.LocalPath == "/IDMSWebAuth/signin")
            {
                Console.WriteLine("父窗口");
            }

            if (e.Url.LocalPath == "/appleauth/auth/signin")
            {
                Console.WriteLine("第二步");
            }

            if (e.Url.LocalPath == "/HK/zh_HK/reserve/iPhone")
            {
                Console.WriteLine("第三步");
            }
        }

        private void button1_Click(object sender, EventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
        {
            var script = textBoxScript2.Text;
            var args = new object[] { script };

            webBrowser1.Document.Window.Frames[0].Document.InvokeScript("eval", args);

            webBrowser1.Document.Window.Frames[0].Document.InvokeScript("step2", new object[] {
                textBoxUser.Text,
                textBoxPwd.Text
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var script = textBoxScript3.Text;
            var args = new object[] { script };

            webBrowser1.Document.InvokeScript("eval", args);

            webBrowser1.Document.InvokeScript("step3", new object[] {
                textBoxPhoneNumber.Text
            });
        }
    }
}
