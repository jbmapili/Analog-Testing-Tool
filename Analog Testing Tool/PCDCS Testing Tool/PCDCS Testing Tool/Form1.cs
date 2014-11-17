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

namespace PCDCS_AnalogValueTester
{
    public partial class AnalogValueTester : Form
    {
        List<TextBox> tag_no = new List<TextBox>();
        List<TextBox> reg_no = new List<TextBox>();
        List<TextBox> valueReg = new List<TextBox>();
        string file;
        int a = -1;
        public AnalogValueTester()
        {
            InitializeComponent();
            try
            {
                file = "Log\\" + DateTime.Now.ToString("MMddyyyy HHmmss") + ".txt";
                if (Directory.Exists(Application.StartupPath + "\\Log"))
                {
                    File.Create(file);
                }
                else
                {
                    Directory.CreateDirectory(Application.StartupPath + "\\Log");
                    File.Create(file);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        DxpSimpleAPI.DxpSimpleClass opc = new DxpSimpleAPI.DxpSimpleClass();
        List<string> registers = new List<string>() { };
        List<string> tags = new List<string>() { };
        string[] sItemIDArray = new string[5];

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
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
                List<string[]> list = new List<string[]>();
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
                        maintable.Visible = false;
                        registers.Clear();
                        tag_no.Clear();
                        reg_no.Clear();
                        valueReg.Clear();
                        tags.Clear();
                        maintable.RowCount = list.Count;
                        maintable.Height = 29 * list.Count;
                        for (int i = 0; i < list.Count; i++)
                        {
                            TextBox txtNo = new TextBox();
                            txtNo.ReadOnly = true;
                            txtNo.Text = (i).ToString();
                            maintable.Controls.Add(txtNo, 0, i);

                            TextBox txtTagNo = new TextBox();
                            txtTagNo.ReadOnly = true;
                            maintable.Controls.Add(txtTagNo, 1, i);
                            txtTagNo.Text = list[i][0];
                            tag_no.Add(txtTagNo);

                            TextBox txtRegNo = new TextBox();
                            txtRegNo.ReadOnly = true;
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

                            registers.Add(list[i][1]);
                            tags.Add(list[i][0]);
                        }
                        maintable.Visible = true;
                        button1.Enabled = true;
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
        private void Analog_Value(int value, int tag)
        {
            try
            {
                string[] target = new string[] {reg_no[tag].Text};
                object[] val = new object[] { value };
                int[] nErrorArray;

                data1.ColumnCount = 5;
                data1.Columns[0].Name = "Date Time";
                data1.Columns[1].Name = "Tag No.";
                data1.Columns[2].Name = "Register No";
                data1.Columns[3].Name = "Status";
                data1.Columns[4].Name = "Success/Error";

                StreamWriter sw = new StreamWriter(file, true);
                if (opc.Write(target, val, out nErrorArray))
                {
                    sw.WriteLine(DateTime.Now.ToString() + "," + tag_no[tag].Text + "," + reg_no[tag].Text + "," + value.ToString() + ",Write Success");
                    string[] row = new string[] { DateTime.Now.ToString(), tag_no[tag].Text, reg_no[tag].Text, value.ToString(), "Write Success" };
                    data1.Rows.Add(row);
                }
                else
                {
                    valueReg[tag].Text = "Write Error";
                    sw.WriteLine(DateTime.Now.ToString() + "," + tag_no[tag].Text + "," + reg_no[tag].Text + "," + value.ToString() + ",Write Error");
                    string[] row = new string[] { DateTime.Now.ToString(), tag_no[tag].Text, reg_no[tag].Text, value.ToString(), "Write Error" };
                    data1.Rows.Add(row);
                }
                short[] wQualityArray;
                OpcRcw.Da.FILETIME[] fTimeArray;

                if (opc.Read(target, out val, out wQualityArray, out fTimeArray, out nErrorArray) == true)
                {
                    valueReg[tag].Text = val[0].ToString();
                    sw.WriteLine(DateTime.Now.ToString() + "," + tag_no[tag].Text + "," + reg_no[tag].Text + "," + valueReg[tag].Text + ",Read Success");
                    string[] row = new string[] { DateTime.Now.ToString(), tag_no[tag].Text, reg_no[tag].Text, valueReg[tag].Text, "Read Success" };
                    data1.Rows.Add(row);
                }
                else
                {

                    sw.WriteLine(DateTime.Now.ToString() + "," + tag_no[tag].Text + "," + reg_no[tag].Text + "," + valueReg[tag].Text + ",Read Error");
                    string[] row = new string[] { DateTime.Now.ToString(), tag_no[tag].Text, reg_no[tag].Text, valueReg[tag].Text, "Read Error" };
                    data1.Rows.Add(row);
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        void oneVal_Click(object sender, EventArgs e)
        {
            Analog_Value(10000, Convert.ToInt32((sender as Button).Tag));
        }

        void fiveVal_Click(object sender, EventArgs e)
        {
            Analog_Value(5000, Convert.ToInt32((sender as Button).Tag));
        }

        void zeroVal_Click(object sender, EventArgs e)
        {
            Analog_Value(0, Convert.ToInt32((sender as Button).Tag));
        }

        void setVal_Click(object sender, EventArgs e)
        {
            try 
            { 
                Analog_Value(Convert.ToInt32(valueReg[Convert.ToInt32((sender as Button).Tag)].Text), Convert.ToInt32((sender as Button).Tag));
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
            if (a > -1)
            {
                tag_no[a].BackColor = SystemColors.Control;
                reg_no[a].BackColor = SystemColors.Control;
            }
            if (registers.Contains(txtReg.Text))
            {
                a = registers.IndexOf(txtReg.Text);
                panel1.VerticalScroll.Value = a * 29;
                reg_no[a].BackColor = Color.Red;
            }
            else if (tags.Contains(txtReg.Text))
            {
                a = tags.IndexOf(txtReg.Text);
                panel1.VerticalScroll.Value = a * 29;
                tag_no[a].BackColor = Color.Red;
            }
        }
    }
}
