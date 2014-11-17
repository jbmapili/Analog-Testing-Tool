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
    public partial class dataGrid : UserControl
    {
        public dataGrid()
        {
            InitializeComponent();
        }
        public DataSet ds;
        public DataTable dt;
        public string[] _dt
        {
            get
            {
                return _dt;
            }
            set
            {
                dt.Rows.Add(_dt[0],_dt[1], _dt[2], _dt[3], _dt[4]);
                dt.AcceptChanges();
                ds.Tables.Add(dt);
                dataGridView1.DataSource=ds;
                MessageBox.Show(_dt[0] + "" + _dt[1] + "" + _dt[2] + "" + _dt[3] + "" + _dt[4]);
            }
        }

        private void dataGrid_Load(object sender, EventArgs e)
        {
            dt.Columns.Add("Date_Time", typeof(string));
            dt.Columns.Add("Tag_No", typeof(string));
            dt.Columns.Add("Register_No", typeof(string));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("Success_Error", typeof(string));
            MessageBox.Show("Column Header Set");
        }
    }
}
