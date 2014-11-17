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

        private DataGridView data1;
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

        private void InitializeComponent()
        {
            this.data1 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.data1)).BeginInit();
            this.SuspendLayout();
            // 
            // data1
            // 
            this.data1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.data1.Location = new System.Drawing.Point(0, 0);
            this.data1.Name = "data1";
            this.data1.RowTemplate.Height = 21;
            this.data1.Size = new System.Drawing.Size(533, 207);
            this.data1.TabIndex = 2;
            // 
            // UserControl3
            // 
            this.Controls.Add(this.data1);
            this.Name = "UserControl3";
            this.Size = new System.Drawing.Size(536, 208);
            ((System.ComponentModel.ISupportInitialize)(this.data1)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
