using System;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using EK.Capture.Dicom.DicomToolKit;
using DataSet = EK.Capture.Dicom.DicomToolKit.DataSet;

namespace DicomRis
{
	public partial class MainForm : Form
	{
		private Server server;
		private string aetitle = "DICOMRIS";
		int port = 2104;
		private Settings settings = new Settings("DicomRis");

		private string generatedDcmFolder;

		public MainForm()
		{
			InitializeComponent();
			InitializeGeneratedDcmFolder();
			GenerateMwlDcmFiles();
			StartServer();
		}

		private void InitializeGeneratedDcmFolder()
		{
			generatedDcmFolder = Directory.GetCurrentDirectory() + @"/out";

			DirectoryInfo directory = new DirectoryInfo(generatedDcmFolder);
			if (directory.Exists)
			{
				Directory.Delete(generatedDcmFolder, true);
			}
		}

		private void StartServer()
		{
			server = new Server(aetitle, port);

			var ris = new CFindServiceSCP(SOPClass.ModalityWorklistInformationModelFIND);
			ris.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

			ris.Query += new QueryEventHandler(OnQuery);

			var echo = new VerificationServiceSCP();
			echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

			server.AddService(ris);
			server.AddService(echo);

			server.Start();
		}

		private void StopServer()
		{
			server.Stop();
			server = null;
		}

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }

		public void OnQuery(object sender, QueryEventArgs args)
		{
			if (GenerateMwlRadioButton.Checked)
			{
				args.Records = new RecordCollection(generatedDcmFolder, true);						
			}
			else
			{
				args.Records = new RecordCollection(folderPathTextBox.Text, true);		
			}
			
			args.Records.Load();
		}

		private void folderDialogButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new FolderBrowserDialog())
			{
				dialog.Description = @"Select folder containing ris dcm files.";
				dialog.RootFolder = Environment.SpecialFolder.MyComputer;
				dialog.SelectedPath = Directory.GetCurrentDirectory();

				dialog.ShowDialog();

				folderPathTextBox.Text = dialog.SelectedPath;
			}

		}

		# region Generate mwl files

		const string studyid = "1.2.840.113564.9.1.20051212xxxxxxx.21000704101";

		#region mwl names

		static private string[] firsts = {
            "Adam",
            "Albert",
            "Arthur",
            "Alicia",
            "Ashton",
            "Barbara",
            "Brian",
            "Bob",
            "Benjamin",
            "Bruce",
            "Betty",
            "Brenda",
            "Cole",
            "Carl",
            "Christopher",
            "Carol",
            "Candice",
            "Cynthia",
            "David",
            "Drew",
            "Debora",
            "Diana",
            "Dusty",
            "Ellenor",
            "Edward",
            "Elise",
            "Frederick",
            "Frank",
            "Fern",
            "Gail",
            "Greg",
            "George",
            "Georgia",
            "Henry",
            "Herb",
            "Helen",
            "Ione",
            "Jack",
            "John",
            "Joan",
            "Katherine",
            "Kathleen",
            "Lisa",
            "Liam",
            "Laura",
            "Michael",
            "Mark",
            "Matthew",
            "Mary",
            "Nerd",
            "Nancy",
            "Ned",
            "Oscar",
            "Olivia",
            "Paul",
            "Peter",
            "Patty",
            "Priscilla",
            "Quin",
            "Richard",
            "Robert",
            "Rich",
            "Robin",
            "Rebecca",
            "Stew",
            "Sam",
            "Steve",
            "Stuart",
            "Sally",
            "Sarah",
            "Todd",
            "Tracy",
            "Vicktor",
            "Walter",
        };

		static private string[] lasts = {
            "Sadler",
            "Tormey",
            "Toor",
            "St.Deny",
            "Mitchel",
            "Rao",
            "Lalena",
            "Odorczyk",
            "Morningstar",
            "Pellerino",
            "Olliver",
            "Adams",
            "Black",
            "White",
            "Fisher",
            "Obama",
            "Smith",
            "Hanks",
            "Nixon",
            "Ford",
            "Bush",
            "Fitzgerald",
            "Anderson",
            "Bauerschmidt",
            "Kaufman",
            "Finkle",
            "Saunders",
            "Perterson",
            "Brown",
            "Bauerschmidt",
            "Chelsea",
            "Cunningham",
            "Davies",
            "Ellington",
            "Hanks",
            "Ioliuci",
            "Jensen",
            "Kraft",
            "Lender",
            "Morley",
            "Nunas",
            "Peterson",
            "Roberts",
            "Sadler",
            "Tennyson",
            "Ziggurat",
            "Evans"
        };

		#endregion mwl names

		static Random random = new Random();
		static int counter = 0;
		const int modulus = 18;

		private void GenerateMwlDcmFiles()
		{        


			string id, name;
			var time = new DateTime(1968, 06, 26, 08, 30, 00);
			var eleventhirty = new DateTime(1968, 06, 26, 11, 30, 00);
			var one = new DateTime(1968, 06, 26, 13, 00, 00);
			var five = new DateTime(1968, 06, 26, 17, 00, 00);
			for (int n = 0; n < 50; n++)
			{
				id = String.Format("1234{0:000}", counter++);
				name = String.Format("{0}^{1}", lasts[random.Next(0, lasts.Length)], firsts[random.Next(0, firsts.Length)]);

				DataSet dicom = CreateRecord(id, studyid.Replace("xxxxxxx", String.Format("{0:0000000}", random.Next(2, 9999999))), "PC100", name, time.ToString("HHmm00.000"), "");
				WriteDicom(dicom);

				time = time.AddMinutes(10.00);

				dicom = CreateRecord(id, studyid.Replace("xxxxxxx", String.Format("{0:0000000}", random.Next(2, 9999999))), "PC100", name, time.ToString("HHmm00.000"), "");
				WriteDicom(dicom);

				time = time.AddMinutes(10.00);

			}

		}

		enum Sex
		{
			M,
			F,
			O
		}

		private DataSet CreateRecord(string id, string uid, string code, string name, string time, string description)
		{
			DataSet dicom = new DataSet();
			dicom.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;

			Sequence sequence;
			Elements item;

			if (id == null || id.Length == 0)
			{
				id = String.Format("1234{0:000}", counter);
			}
			if (code == null || code.Length == 0)
			{
				code = String.Format("PC1{0:00}", counter % modulus);
			}

			dicom.Add(t.SpecificCharacterSet, "ISO_IR 100");
			dicom.Add(t.AccessionNumber, String.Format("5678{0}", counter));
			dicom.Add(t.ReferringPhysicianName, "Gray");

			sequence = new Sequence(t.ReferencedStudySequence);
			dicom.Add(sequence);
			item = sequence.NewItem();
			item.Add(t.ReferencedSOPClassUID, "1.2.840.10008.3.1.2.3.1");
			item.Add(t.ReferencedSOPInstanceUID, "");

			if (name == null)
			{
				dicom.Add(t.PatientName, String.Format("{0}^{1}", lasts[random.Next(0, lasts.Length)], firsts[random.Next(0, firsts.Length)]));
			}
			else
			{
				dicom.Add(t.PatientName, name);
			}
			dicom.Add(t.PatientID, id);
			dicom.Add(t.PatientBirthDate, String.Format("{0:0000}{1:00}{2:00}", 2008 - random.Next(0, 100), random.Next(1, 13), random.Next(1, 29)));
			dicom.Add(t.PatientBirthTime, String.Format("{0:00}{1:00}{2:00}.000", random.Next(0, 24), random.Next(0, 60), random.Next(0, 60)));
			// no other
			dicom.Add(t.PatientSex, String.Format("{0}", ((Sex)random.Next(0, 2)).ToString()));
			dicom.Add(t.OtherPatientIDs, "");
			dicom.Add(t.OtherPatientNames, "");
			dicom.Add(t.PatientComments, "");
			id = id.Replace("P", "");
			if (uid == null)
			{
				dicom.Add(t.StudyInstanceUID, studyid.Replace("xxxxxxx", id));
			}
			else
			{
				dicom.Add(t.StudyInstanceUID, uid);
			}
			dicom.Add(t.RequestingService, "");
			dicom.Add(t.RequestedProcedureDescription, description);

			dicom.Add(t.VisitStatusID, "SCHEDULED");

			sequence = new Sequence(t.ScheduledProcedureStepSequence);
			dicom.Add(sequence);
			item = sequence.NewItem();
			item.Add(t.Modality, "CR");
			item.Add(t.RequestedContrastAgent, "");
			item.Add(t.ScheduledStationAETitle, "SUNDANCE-Test-1");
			//item.Add(t.ScheduledProcedureStepStartDate, DateTime.Now.AddDays(random.Next(0, 2)).ToString("yyyyMMdd"));
			item.Add(t.ScheduledProcedureStepStartDate, DateTime.Now.ToString("yyyyMMdd"));
			item.Add(t.ScheduledProcedureStepStartTime, (time != null) ? time : String.Format("{0:00}{1:00}00.000", random.Next(0, 23), random.Next(0, 12) * 5));
			item.Add(t.ScheduledProcedureStepDescription, description);
			item.Add(t.ScheduledProcedureStepID, "1");
			// (0040,0100)(0040,0008)(0008,0100)
			sequence = new Sequence(t.ScheduledProtocolCodeSequence);
			item.Add(sequence);
			item = sequence.NewItem();
			item.Add(t.CodeValue, code);
			item.Add(t.CodingSchemeDesignator, null);
			item.Add(t.CodingSchemeVersion, null);
			item.Add(t.CodeMeaning, null);

			// (0040,1001)
			dicom.Add(t.RequestedProcedureID, code);

			// (0032,1064)(0008,0100)
			sequence = new Sequence(t.RequestedProcedureCodeSequence);
			dicom.Add(sequence);
			item = sequence.NewItem();
			item.Add(t.CodeValue, code);
			item.Add(t.CodingSchemeDesignator, null);
			item.Add(t.CodingSchemeVersion, null);
			item.Add(t.CodeMeaning, null);

			return dicom;
		}

		private void WriteDicom(DataSet dicom)
		{
			DirectoryInfo directory = new DirectoryInfo(generatedDcmFolder);
			if (!directory.Exists)
			{
				directory.Create();
			}
			string temp = Path.Combine(generatedDcmFolder, (string)dicom[t.PatientID].Value);
			FileInfo file = new FileInfo(temp + ".dcm");
			if (file.Exists)
				temp += "1";
			dicom.Write(temp + ".dcm");
		}

		#endregion Generate mwl files

		private void AboutButton_Click(object sender, EventArgs e)
		{
			string version = AssemblyName.GetAssemblyName(Assembly.GetAssembly(this.GetType()).Location).Version.ToString();
			string services = string.Empty;

			foreach (ServiceClass service in server.Services)
			{
				if (services.Length > 0)
					services += "\n";
				services += Reflection.GetName(typeof(SOPClass), service.SOPClassUId);
			}

			MessageBox.Show(String.Format("DicomRis, version {0}\n\n{1} on port {2}\n\n{3}", version, aetitle, port, services), "About");

		}

	}
}
