using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsControlLibrary1
{
    public partial class UserControl3 : UserControl
    {
        public UserControl3()
        {
            InitializeComponent();
        }
        public DataSet _DataSet;
        public DataSet DataSet
        {
            get
            {
                //data1.DataSource = _DataSet;
                return _DataSet;
            }
            set
            {
                _DataSet = value;
                MessageBox.Show("fired");
            }
        }
    }
}
