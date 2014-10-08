using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.imageright.server;
using com.imageright.server.workflow;
using ImageRight.Client;
using ImageRight.Common.Streaming.CSV;
using imageright.interfaces;
using Form = System.Windows.Forms.Form;
using System.ComponentModel;
using System.Windows.Forms;

namespace ImgrAutochecker
{
    public partial class Form1 : Form
    {
        private static IConnection _conn;
        private string _workflow;
        private long _file;
        BindingList<AttributeList> _attributes;

        public Form1()
        {
            InitializeComponent();

            backgroundWorker1.WorkerReportsProgress = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ImageRightSystem imgr = new ConnectToImgr().Connect();
            _conn = imgr.ServerConnection;
            GetAllWorkflows();
            GetAllFiles();
            GetAllAttributes();
        }

        private void GetAllWorkflows()
        {
            WorkflowDef[] workflows = _conn.WorkflowMetadata.Workflows();
            if (!workflows.Any()) return;

            foreach (WorkflowDef workflow in  workflows)
            {
                comboBox1.Items.Add(workflow.Name);
            }
            comboBox1.SelectedIndex = 0;
        }

        private void GetAllFiles()
        {
            var fileSearch = new SearchConditionLists();
            var fileNumCond = new FileCondition(-1, "", FileAttribute.fsaFileNumber, CustomAttributeTarget.catSelf,
                CompareOperation.coNotNull);

            fileSearch.FileConditions = new ArrayList();
            fileSearch.FileConditions.Add(fileNumCond);
            FileDataTransportObject filesObj = _conn.DocumentServer.FindFile(fileSearch);

            if (!filesObj.objects.Any()) return;

            for (int i = 0; i < filesObj.objects.Count() && i < 10; i++)
            {
                ImgrFiles item = new ImgrFiles();
                item.Text = filesObj.objects[i].fileNumber1;
                item.Value = filesObj.objects[i].id;
                comboBox2.Items.Add(item);
            }
            comboBox2.SelectedIndex = 0;
        }

        private void GetAllAttributes()
        {
            _attributes = new BindingList<AttributeList>();

            // Allow new parts to be added, but not removed once committed.        
            _attributes.AllowNew = true;
            _attributes.AllowRemove = false;

            // Raise ListChanged events when new parts are added.
            _attributes.RaiseListChangedEvents = true;

            // Do not allow parts to be edited.
            _attributes.AllowEdit = true;   
 
            // Add a couple of parts to the list.
            AttributeDef[] attrDefs = _conn.Metadata.AttributeDefs();
            foreach (AttributeDef attrDef in attrDefs)
            {
                _attributes.Add(new AttributeList(attrDef.name, attrDef.validationRules));
            }

            dataGridView1.DataSource = _attributes;
            dataGridView1.Columns[0].HeaderText = "Attribute";
            dataGridView1.Columns[1].HeaderText = "All";
            dataGridView1.Columns[1].FillWeight = 20;
            dataGridView1.Columns[2].HeaderText = "Value";
        }

        private void Button_Click(object sender, EventArgs e)
        {
            _workflow = comboBox1.SelectedItem.ToString();
            _file = ((ImgrFiles)comboBox2.SelectedItem).Value;

            if (String.IsNullOrEmpty(_workflow))
            {
                MessageBox.Show("Please select workflow!","Warning");
            }

            if (_file == 0)
            {
                MessageBox.Show("Please select file in combobox!", "Warning");
            }


            progressBar1.Visible = true;
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            dataGridView1.Enabled = false;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;

            backgroundWorker1.RunWorkerAsync();
        }

        void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            dataGridView1.Enabled = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            List<WorkflowStepData> wfSteps = ImgrProcessor.GetFromImgr(_conn, _workflow, _attributes);
            int i = 0;
            foreach (WorkflowStepData step in wfSteps)
            {
                
                ImgrProcessor.CreateTask(_conn, _file, step);
                ImgrProcessor.ReleaseTask(_conn, step);
                _conn.Workflow.KillTask(step.TaskId);

                int position = (int)((((double)i) / wfSteps.Count) * 100);
                (sender as BackgroundWorker).ReportProgress(position);
                i++;
            }

            Excel.SaveToExcel(wfSteps);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
             // The progress percentage is a property of e
            progressBar1.Value = e.ProgressPercentage;
        }

 }
}