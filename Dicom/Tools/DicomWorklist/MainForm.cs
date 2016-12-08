using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomWorklist
{
    public partial class MainForm : Form
    {
        private Server server;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Start();
        }

        private void Start()
        {
            if (server == null)
            {
                server = new Server(new ApplicationEntity("DicomWorklist", 6104));

                VerificationServiceSCP echo = new VerificationServiceSCP();
                echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
                echo.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
                echo.Syntaxes.Add(Syntax.ExplicitVrBigEndian);

                CFindServiceSCP ris = new CFindServiceSCP(SOPClass.ModalityWorklistInformationModelSOPClass);
                ris.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
                ris.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
                ris.Syntaxes.Add(Syntax.ExplicitVrBigEndian);
                ris.Query += new QueryEventHandler(OnQuery);

                server.AddService(echo);
                server.AddService(ris);

                server.Start();
            }
        }

        private void Stop()
        {
            server.Stop();
            server = null;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
        }

        public void OnQuery(object sender, QueryEventArgs args)
        {
            args.Records = new RecordCollection(@".", true);
            args.Records.Load();
            foreach(Elements record in args.Records)
            {
                record.Set(t.ScheduledProcedureStepSequence + t.ScheduledProcedureStepStartDate, DateTime.Now.ToString("YYYYMMdd"));
            }
        }
    }
}
