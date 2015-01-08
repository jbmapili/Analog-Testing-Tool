using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpcRcw.Da;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace PCDCS_AnalogValueTester
{
    public partial class AnalogValueTester : Form
    {
        List<TextBox> tag_no = new List<TextBox>();
        List<TextBox> reg_no = new List<TextBox>();
        List<TextBox> valueReg = new List<TextBox>();
        List<string> listreg = new List<string>();
        List<string[]> list = new List<string[]>();
        int a = -1;
        bool saved = true;
        public AnalogValueTester()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
        }

        DxpSimpleAPI.DxpSimpleClass opc = new DxpSimpleAPI.DxpSimpleClass();
        List<string> registers = new List<string>() { };
        List<string> tags = new List<string>() { };
        string[] sItemIDArray = new string[5];

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            progressBar1.Visible = false;
            maintable.Visible = false;
        }

        private void btnListRefresh_Click(object sender, EventArgs e)
        {
            cmbServerList.Items.Clear();
            string[] ServerNameArray;
            opc.EnumServerList(txtNode.Text, out ServerNameArray);

            for (int a = 0; a < ServerNameArray.Count<string>(); a++)
            {
                cmbServerList.Items.Add(ServerNameArray[a]);
            }
            cmbServerList.SelectedIndex = 0;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (opc.Connect(txtNode.Text, cmbServerList.Text))
            {
                btnListRefresh.Enabled = false;
                btnDisconnect.Enabled = true;
                btnConnect.Enabled = false;
            }
        }
        public class DoubleBufferedTableLayoutPanel : TableLayoutPanel
        {
            public DoubleBufferedTableLayoutPanel()
            {
                DoubleBuffered = true;
            }
        }
        private void FileReadBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (TextFieldParser parser = new TextFieldParser(openFileDialog1.FileName, Encoding.GetEncoding(932)))
                {
                    parser.Delimiters = new string[] { "," };
                    bool st = false;
                    Regex reg = new Regex(@"^PV\(.+");

                    while (true)
                    {
                        string[] parts = parser.ReadFields();//read field function reads the line and returns array after that reading it moves to the next line
                        if (parts == null)
                        {
                            break;
                        }
                        if (parts[0] == "[EVENT]")
                        {
                            st = false;
                        }                           
                        Match m = reg.Match(parts[0]);
                        if (m.Success || st) {
                            list.Add(parts);//list here is array then the array parts will be added to list
                        }

                        if (parts[0] == "[NOTGAUGE]")
                        {
                            st = true;
                        }
                    }


                    maintable.Controls.Clear();
                    if (list.Count > 0)
                    {
                        registers.Clear();
                        tag_no.Clear();
                        reg_no.Clear();
                        valueReg.Clear();
                        tags.Clear();
                        progressBar1.Maximum = list.Count;
                        maintable.RowCount = list.Count;
                        maintable.Height = 29 * list.Count;
                        backgroundWorker1.RunWorkerAsync();
                        for (int i = 0; i < list.Count; i++)
                        {
                            registers.Add(list[i][1]);
                            tags.Add(list[i][0]);
                        }
                    }
                    else {
                        Label message = new Label();
                        maintable.Visible = false;
                        message.Text = "There are no lists inside the file.";
                        message.Location = new Point(0, 30);
                        message.Width = 200;                        
                        panel1.Controls.Add(message);
                    }
                }
            }
        }
        private void Analog_Value(int value, int tag, string sender)
        {
            try
            {
                string[] target = new string[] {reg_no[tag].Text};
                object[] val = new object[] { value };
                int[] nErrorArray;

                data1.ColumnCount = 6;
                data1.Columns[0].Name = "Date Time";
                data1.Columns[1].Name = "Tag No.";
                data1.Columns[2].Name = "Register No";
                data1.Columns[3].Name = "Status";
                data1.Columns[4].Name = "Success/Error";
                data1.Columns[5].Name = "Sender";

                bool rw=opc.Write(target, val, out nErrorArray);
                if (nErrorArray[0] == 0 && rw)
                {
                    string[] row = new string[] { DateTime.Now.ToString(), tag_no[tag].Text, reg_no[tag].Text, value.ToString(), "Write Success", sender };
                    data1.Rows.Add(row);
                }
                else
                {
                    valueReg[tag].Text = "Write Error";
                    string[] row = new string[] { DateTime.Now.ToString(), tag_no[tag].Text, reg_no[tag].Text, value.ToString(), "Write Error", sender };
                    data1.Rows.Add(row);
                }
                short[] wQualityArray;
                OpcRcw.Da.FILETIME[] fTimeArray;

                bool rr = opc.Read(target, out val, out wQualityArray, out fTimeArray, out nErrorArray);
                if (nErrorArray[0] == 0 && rr)
                {
                    valueReg[tag].Text = val[0].ToString();
                    string[] row = new string[] { DateTime.Now.ToString(), tag_no[tag].Text, reg_no[tag].Text, valueReg[tag].Text, "Read Success", sender };
                    data1.Rows.Add(row);
                }
                else
                {

                    string[] row = new string[] { DateTime.Now.ToString(), tag_no[tag].Text, reg_no[tag].Text, valueReg[tag].Text, "Read Error", sender };
                    data1.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        void oneVal_Click(object sender, EventArgs e)
        {
            Analog_Value(10000, Convert.ToInt32((sender as Button).Tag), (sender as Button).Text);
        }

        void fiveVal_Click(object sender, EventArgs e)
        {
            Analog_Value(5000, Convert.ToInt32((sender as Button).Tag), (sender as Button).Text);
        }

        void zeroVal_Click(object sender, EventArgs e)
        {
            Analog_Value(0, Convert.ToInt32((sender as Button).Tag), (sender as Button).Text);
        }

        void setVal_Click(object sender, EventArgs e)
        {
            try 
            {
                Analog_Value(Convert.ToInt32(valueReg[Convert.ToInt32((sender as Button).Tag)].Text), Convert.ToInt32((sender as Button).Tag), (sender as Button).Text);
            }
            catch (Exception) { }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (opc.Disconnect())
            {
                btnConnect.Enabled = true;
                btnListRefresh.Enabled = true;
                btnDisconnect.Enabled = false;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (registers.Contains(txtReg.Text))
            {
                if (a > -1)
                {
                    tag_no[a].BackColor = SystemColors.Control;
                    reg_no[a].BackColor = SystemColors.Control;
                }
                a = registers.IndexOf(txtReg.Text);
                panel1.VerticalScroll.Value = a * 29;
                reg_no[a].BackColor = Color.Red;
            }
            else if (tags.Contains(txtReg.Text))
            {
                if (a > -1)
                {
                    tag_no[a].BackColor = SystemColors.Control;
                    reg_no[a].BackColor = SystemColors.Control;
                }
                a = tags.IndexOf(txtReg.Text);
                panel1.VerticalScroll.Value = a * 29;
                tag_no[a].BackColor = Color.Red;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Thread.Sleep(100);
                backgroundWorker1.ReportProgress(i);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int i=e.ProgressPercentage;

            TextBox txtNo = new TextBox();
            txtNo.ReadOnly = true;
            txtNo.Text = (i).ToString();
            maintable.Controls.Add(txtNo, 0, i);

            TextBox txtTagNo = new TextBox();
            txtTagNo.ReadOnly = true;
            txtTagNo.Tag = i.ToString();
            maintable.Controls.Add(txtTagNo, 1, i);
            txtTagNo.Text = list[i][0];
            tag_no.Add(txtTagNo);

            TextBox txtRegNo = new TextBox();
            txtRegNo.ReadOnly = true;
            txtRegNo.Tag = i.ToString();
            maintable.Controls.Add(txtRegNo, 2, i);
            txtRegNo.Text = list[i][1];
            reg_no.Add(txtRegNo);

            TextBox val = new TextBox();
            maintable.Controls.Add(val, 3, i);
            valueReg.Add(val);

            Button setVal = new Button();
            setVal.Text = "Set";
            setVal.Click += setVal_Click;
            maintable.Controls.Add(setVal, 4, i);
            setVal.Tag = i.ToString();

            Button zeroVal = new Button();
            zeroVal.Text = "0";
            zeroVal.Click += zeroVal_Click;
            maintable.Controls.Add(zeroVal, 5, i);
            zeroVal.Tag = i.ToString();

            Button fiveVal = new Button();
            fiveVal.Text = "5000";
            fiveVal.Click += fiveVal_Click;
            maintable.Controls.Add(fiveVal, 6, i);
            fiveVal.Tag = i.ToString();

            Button oneVal = new Button();
            oneVal.Text = "10000";
            oneVal.Click += oneVal_Click;
            maintable.Controls.Add(oneVal, 7, i);
            oneVal.Tag = i.ToString();

            Button down = new Button();
            down.Text = "▼";
            down.Click += down_Click;
            maintable.Controls.Add(down, 8, i);
            down.Tag = i.ToString();

            Button up = new Button();
            up.Text = "▲";
            up.Click += up_Click;
            maintable.Controls.Add(up, 9, i);
            up.Tag = i.ToString();

            progressBar1.Value = e.ProgressPercentage;

            if (e.ProgressPercentage + 1 == list.Count)
            {
                maintable.Visible = true;
                button1.Enabled = true;
                progressBar1.Visible = false;
            }
            if (e.ProgressPercentage == 0)
            {
                maintable.Visible = false;
                button1.Enabled = false;
                progressBar1.Visible = true;
            }
        }

        void up_Click(object sender, EventArgs e)
        {
            int c = 0, a = Convert.ToInt32((sender as Button).Tag);
            if (Int32.TryParse(valueReg[a].Text, out c))
            {
                valueReg[a].Text = (Convert.ToInt32(valueReg[a].Text) + 1).ToString();
                Analog_Value(Convert.ToInt32(valueReg[a].Text), a, (sender as Button).Text);
            }
        }

        void down_Click(object sender, EventArgs e)
        {
            int c = 0, a = Convert.ToInt32((sender as Button).Tag);
            if (Int32.TryParse(valueReg[a].Text, out c))
            {
                valueReg[a].Text = (Convert.ToInt32(valueReg[a].Text) - 1).ToString();
                Analog_Value(Convert.ToInt32(valueReg[a].Text), a, (sender as Button).Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "Csv File|*.csv|Text File|*.txt";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sf.FileName))
                {
                    for (int r = 0; r < data1.RowCount - 1; r++)
                    {
                        sw.WriteLine("{0},{1},{2},{3},{4}", data1.Rows[r].Cells[0].Value,
                                                            data1.Rows[r].Cells[1].Value,
                                                            data1.Rows[r].Cells[2].Value,
                                                            data1.Rows[r].Cells[3].Value,
                                                            data1.Rows[r].Cells[4].Value);
                    }
                    sw.Close();
                    saved = true;
                }
            }
        }

        private void txtReg_TextChanged(object sender, EventArgs e)
        {

            foreach (TextBox tag in tag_no)
            {
                tag.BackColor = SystemColors.Control;
            }
            foreach (TextBox regs in reg_no)
            {
                regs.BackColor = SystemColors.Control;
            }
            if (txtReg.Text != "")
            {
                listreg.Clear();
                string regE=txtReg.Text.Replace("(",@"\(");
                regE = regE.Replace(")", @"\)");
                Regex reg = new Regex(regE, RegexOptions.IgnoreCase);
                foreach (TextBox tag in tag_no)
                {
                    Match m = reg.Match(tag.Text);
                    if (m.Success)
                    {
                        tag.BackColor = Color.Red;
                        listreg.Add(tag.Tag.ToString());
                    }
                }
                foreach (TextBox regs in reg_no)
                {
                    Match m = reg.Match(regs.Text);
                    if (m.Success)
                    {
                        regs.BackColor = Color.Red;
                        listreg.Add(regs.Tag.ToString());
                    }
                }
                panel1.VerticalScroll.Value = Convert.ToInt32(listreg[0]) * 29;
            }
        }

        private void data1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            saved = false;
            data1.FirstDisplayedScrollingRowIndex = data1.RowCount - 1;
        }

        private void AnalogValueTester_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved)
            {
                if (MessageBox.Show("Do you want to save the log to file", "ERROR", 
                                    MessageBoxButtons.OKCancel, 
                                    MessageBoxIcon.Warning).ToString() == "OK") 
                {
                    SaveFileDialog sf = new SaveFileDialog();
                    sf.Filter = "Csv File|*.csv|Text File|*.txt";
                    if (sf.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter sw = new StreamWriter(sf.FileName))
                        {
                            for (int r = 0; r < data1.RowCount - 1; r++)
                            {
                                sw.WriteLine("{0},{1},{2},{3},{4}", data1.Rows[r].Cells[0].Value,
                                                                    data1.Rows[r].Cells[1].Value,
                                                                    data1.Rows[r].Cells[2].Value,
                                                                    data1.Rows[r].Cells[3].Value,
                                                                    data1.Rows[r].Cells[4].Value);
                            }
                            sw.Close();
                            saved = true;
                        }
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }
                
        }
    }
}
