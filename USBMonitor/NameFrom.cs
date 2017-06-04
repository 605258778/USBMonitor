using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USBMonitor
{
    public partial class NameFrom : Form
    {
       
        public NameFrom()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.nameImput = nameImput.Text;
            this.Close();
            //这里点确定之后就关了
        }
    }
}
