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

namespace faster
{
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
                var str2 = client.DownloadString(uri_avail);
                var avail = JsonConvert.DeserializeObject<dynamic>(str2);

                buy_id = textBoxBuy.Text;

                var find = FindItem(avail, buy_id, ref store_id);

                if (find)
                {
                    break;
                }
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

$('#selectSubfamily').val('10143').trigger('change');

$('input[value="128GB"]').attr('checked','checked').trigger('change');

$('input[value="MN8P2ZP/A"]').attr('checked','checked').trigger('change');

$('button[name="submit"]').trigger('click');

             */
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //backgroundWorker1.RunWorkerAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var store = "R580";
            var family = "10143";
            var capacity = "128GB";
            var product = "MNH02CH/A";

            /// MN8P2ZP/A   iPhone 7 
            /// MN4C2ZP/A   iPhone 7 PLUS

            //SubmitAction(store_id, buy_id);
            SimulateSelect(store, family, capacity, product);
        }

        void SimulateSelect(string store, string family, string capacity, string product)
        {
            var bug = @"hel""lo";
            Console.WriteLine(bug);

            string s1 = $"$('#selectStore').val('{store}').trigger('change');\n";
            string s2 = $"$('#selectSubfamily').val('{family}').trigger('change');\n";
            string s3 = $"$('input[value='{ capacity} ']').attr('checked','checked').trigger('change');\n";
            string s4 = $"$('input[value='{product}']').attr('checked','checked').trigger('change');\n";
            var s5 = $"$('button[name='submit']').trigger('click');\n";

            var script = s1 + s2 + s3 + s4 + s5;

            webBrowser1.Document.InvokeScript("eval", new object[] { script });

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Console.WriteLine(e.Url);
        }
    }
}
