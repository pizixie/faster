using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace faster
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var tb = webBrowser1.Document.GetElementById("kw");
            var su = webBrowser1.Document.GetElementById("su");

            (tb.DomElement as dynamic).value = "Hello";

            var script = @"
var obj = document.getElementById('su');
alert(obj.value);

evt = document.createEvent('MouseEvents'); 
evt.initMouseEvent('click', true, true, window,
0, 0, 0, 0, 0, false, false, false, false, 0, null);
document.dispatchEvent(evt);
            ";
            //webBrowser1.Document.InvokeScript("eval", new object[] { script });
            //su.RaiseEvent("onclick");//onclick无效 click异常

            su.InvokeMember("click");//有效
        }
    }
}
