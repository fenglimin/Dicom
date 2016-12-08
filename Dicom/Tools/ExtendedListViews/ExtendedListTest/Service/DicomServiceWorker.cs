using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DicomViewer;
using EK.Capture.Dicom.DicomToolKit;

namespace ExtendedListTest.Service
{
    public class DicomServiceWorker
    {
		//public ImageStoredEventHandler OnImageStored { get; set; }
		//public PrintJobEventHandler OnImagePrinted { get; set; }
		//public StorageCommitEventHandler OnStorageCommitRequested { get; set; }

        public QueryEventHandler OnQuery { get; set; }

        public bool ReceiveThenShow { get; set; }
        public int Port { get; private  set; }
        public string AeTitle { get; private set; }
        private Server server;

        public string StorageRootPath { get; set; }

        private const string mainstorageName = "Main";

        public IList<ReceivedDicomElements> ListCachedElements = new List<ReceivedDicomElements>(); 

        private IDicomServiceWorkerUser dicomServiceWorkerUser;

        public DicomServiceWorker(IDicomServiceWorkerUser dicomServiceWorkerUser)
        {
            this.dicomServiceWorkerUser = dicomServiceWorkerUser;

            Port = dicomServiceWorkerUser.GetPort();
            AeTitle = dicomServiceWorkerUser.GetAtTitle();

            CreateDefaultPath();

        }

        private void CreateDefaultPath()
        {
            StorageRootPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Storage";
            if (!Directory.Exists(StorageRootPath))
                Directory.CreateDirectory(StorageRootPath);

            var mainStoragePath = Path.Combine(StorageRootPath, mainstorageName);
            if (!Directory.Exists(mainStoragePath))
                Directory.CreateDirectory(mainStoragePath);
        }

		public void OnDicomElementsReceived(ReceivedDicomElements receivedDicomElements)
        {
			ListCachedElements.Add(receivedDicomElements);

		    if (dicomServiceWorkerUser.SaveWhenReceived())
		    {
                SaveToDicomDir(receivedDicomElements, dicomServiceWorkerUser.GetActiveStorage());
		    }

			dicomServiceWorkerUser.OnDicomElementsReceived(receivedDicomElements);
        }

        public IEnumerable<string> GetAllDicomDir()
        {
            return Directory.EnumerateDirectories(StorageRootPath).Select(x => (new DirectoryInfo(x).Name));
        }

        public void SaveToDicomDir(ReceivedDicomElements receivedDicomElements, string dicomDirName)
        {
            var message =  " -- DicomDir : " + dicomDirName + ", ";
            var hasError = true;
            try
            {
                if (receivedDicomElements.ImageSource == ImageSource.LocalDicomFile)
                {
                    message += "File : " + receivedDicomElements.FileName;
                }
                else
                {
                    message += string.Format("AeTitle : {0}, IpAddress : {1}", receivedDicomElements.CallingAeTitle, receivedDicomElements.IpAddress);
                }


                var dicomDirPath = Path.Combine(StorageRootPath, dicomDirName);
                var dicomDir = new DicomDir(dicomDirPath);
                dicomDir.Add(receivedDicomElements.Elements);
                dicomDir.Save();
                receivedDicomElements.OnDicomDirSaved(dicomDirName);
				dicomServiceWorkerUser.OnDicomDirSaved(receivedDicomElements, dicomDirName);

                message = "Save to dicom dir successfull!" + message;
                hasError = false;
            }
            catch (Exception ex)
            {
                message = "Save to dicom dir failed!" + message + "  " + ex.Message;
            }
            finally
            {
                dicomServiceWorkerUser.ShowMessage(message, hasError, false);
            }
        }

        public int CachedCount()
        {
            return ListCachedElements.Count(x => x.ImageStatus == ImageMemoryStatus.CachedInMemory);
        }

        public bool IsPortOpened()
        {
            var result = false;
            using (var association = new Association())
            {
                var echo = new VerificationServiceSCU();
                echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

                association.AddService(echo);
                if (association.Open(AeTitle, IPAddress.Parse("127.0.0.1"), Port))
                {
                    result = true;
                }
                association.Close();
            }
            return result;
        }

        public bool StartService()
        {
            if (IsPortOpened())
            {
                MessageBox.Show("SCP is already running in another instance.");
                return false;
            }

            server = new Server(AeTitle, Port);

            var echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var dx1 = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForPresentation);
            dx1.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var dx2 = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForProcessing);
            dx2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var cr = new StorageServiceSCP(SOPClass.ComputedRadiographyImageStorage);
            cr.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var mg1 = new StorageServiceSCP(SOPClass.DigitalMammographyImageStorageForPresentation);
            mg1.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var mg2 = new StorageServiceSCP(SOPClass.DigitalMammographyImageStorageForProcessing);
            mg2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var gsps = new StorageServiceSCP(SOPClass.GrayscaleSoftcopyPresentationStateStorageSOPClass);
            gsps.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var sc = new StorageServiceSCP(SOPClass.SecondaryCaptureImageStorage);
            sc.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var ct = new StorageServiceSCP(SOPClass.CTImageStorage);
            ct.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var ctEnhanced = new StorageServiceSCP(SOPClass.EnhancedCTImageStorage);
            ctEnhanced.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var dose = new StorageServiceSCP(SOPClass.XRayRadiationDoseSRStorage);
            dose.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var find = new CFindServiceSCP(SOPClass.StudyRootQueryRetrieveInformationModelFIND);
            find.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var move = new CMoveServiceSCP(SOPClass.StudyRootQueryRetrieveInformationModelMOVE);
            move.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var mpps = new MppsServiceSCP();
            mpps.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var commit = new StorageCommitServiceSCP();
            commit.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var grayscale = new PrintServiceSCP(SOPClass.BasicGrayscalePrintManagementMetaSOPClass);
            grayscale.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var plut = new PresentationLUTServiceSCP();
            plut.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            var annotation = new AnnotationServiceSCP();
            annotation.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(echo);
            server.AddService(dx1);
            server.AddService(dx2);
            server.AddService(cr);
            server.AddService(mg1);
            server.AddService(mg2);
            server.AddService(gsps);
            server.AddService(sc);
            server.AddService(ct);
            server.AddService(ctEnhanced);
            server.AddService(dose);
            server.AddService(find);
            server.AddService(move);
            server.AddService(mpps);
            server.AddService(commit);
            server.AddService(grayscale);
            server.AddService(plut);
            server.AddService(annotation);

            foreach (ServiceClass service in server.Services)
            {
                if (service != null)
                {
                    if (service is StorageServiceSCP)
                    {
                        ((StorageServiceSCP)service).ImageStored += OnImageStored;
                    }
                    else if (service is PrintServiceSCP)
                    {
                        ((PrintServiceSCP)service).JobPrinted += OnImagePrinted;
                    }
                    else if (service is MppsServiceSCP)
                    {
                        ((MppsServiceSCP)service).MppsCreate += OnMpps;
                        ((MppsServiceSCP)service).MppsSet += OnMpps;
                    }
                    else if (service is StorageCommitServiceSCP)
                    {
                        ((StorageCommitServiceSCP)service).StorageCommitRequest += OnStorageCommitRequested;
                    }
                    else if (service is CFindServiceSCP)
                    {
                        ((CFindServiceSCP)service).Query += OnQuery;
                    }
                }
            }

            server.Start();

            return true;
        }

        public void StopService()
        {
            if (server != null)
            {
                foreach (ServiceClass service in server.Services)
                {
                    if (service != null)
                    {
                        if (service is StorageServiceSCP)
                        {
							((StorageServiceSCP)service).ImageStored -= OnImageStored;
                        }
                        else if (service is PrintServiceSCP)
                        {
							((PrintServiceSCP)service).JobPrinted -= OnImagePrinted;
                        }
                        else if (service is MppsServiceSCP)
                        {
                            ((MppsServiceSCP)service).MppsCreate -= OnMpps;
                            ((MppsServiceSCP)service).MppsSet -= OnMpps;
                        }
                        else if (service is StorageCommitServiceSCP)
                        {
							((StorageCommitServiceSCP)service).StorageCommitRequest -= OnStorageCommitRequested;
                        }
                        else if (service is CFindServiceSCP)
                        {
                            ((CFindServiceSCP)service).Query -= OnQuery;
                        }
                    }
                }

                server.Stop();
                server = null;
            }
        }

		public bool SendStorageCommit(ReceivedDicomElements receivedDicomElements, int port, bool success)
		{
			var result = true;
			try
			{
				var commit = new StorageCommitServiceSCU();
				commit.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
				commit.Role = Role.Scp;

				var association = new Association();
				association.AddService(commit);

				var host = new ApplicationEntity(receivedDicomElements.CallingAeTitle, IPAddress.Parse(receivedDicomElements.IpAddress), port);

				if (association.Open(host))
				{
					if (commit.Active)
					{
						try
						{
							DataSet reportElements;

							if (!success)
							{
								reportElements = new DataSet
								{
									{t.TransactionUID, receivedDicomElements.Elements.GetSafeStringValue(t.TransactionUID)},
									{t.RetrieveAETitle, host.Title}
								};

								// add the FailedSOPSequence
								var sequence = new Sequence(t.FailedSOPSequence);
								reportElements.Add(sequence);

								var failedList = GetStorageCommitmentReferenceDicomElements(receivedDicomElements)
                                    .Where(x => x.SavedToDicomDir == false && x.SavedToDisk == false).ToList();
								foreach (var failedElements in failedList)
								{
									var item = sequence.NewItem();
									item.Add(t.ReferencedSOPClassUID, failedElements.Elements.GetSafeStringValue(t.SOPClassUID));
									item.Add(t.ReferencedSOPInstanceUID, failedElements.Elements.GetSafeStringValue(t.SOPInstanceUID));
									item.Add(t.FailureReason, 274);
								}

								reportElements.Add(t.Status, 2);
							}
							else
							{
								reportElements = receivedDicomElements.Elements;
								reportElements.Set(t.Status, 1);
							}

							

							result = commit.Report(reportElements);
							
						}
						catch (Exception ex)
						{
							result = false;
							
						}
					}
				}
				else
				{
					result = false;
				}
				association.Close();
			}
			catch
			{
			}

			return result;
		}

	    private void OnStorageCommitRequested(object sender, StorageCommitEventArgs e)
	    {
			var receivedDicomElements = new ReceivedDicomElements
			{
				Elements = e.DataSet,
				ReceivedDateTime = DateTime.Now,
				ImageSource = ImageSource.StorageCommitment,
				CallingAeTitle = e.CallingAeTitle,
				CallingAeIpAddress = e.CallingAeIpAddress,
				ImageStatus = ImageMemoryStatus.CachedInMemory
			};

			OnDicomElementsReceived(receivedDicomElements);
	    }

		private void OnImageStored(object sender, ImageStoredEventArgs e)
		{
		    var message = string.Empty;
		    var hasError = true;
		    try
		    {
		        var receivedDicomElements = new ReceivedDicomElements
		        {
		            CallingAeTitle = e.CallingAeTitle,
		            CallingAeIpAddress = e.CallingAeIpAddress,
		            ReceivedDateTime = DateTime.Now,
		            Elements = e.DataSet,
		            ImageSource = ImageSource.Store,
		            ImageStatus = dicomServiceWorkerUser.OpenWhenReceived()? ImageMemoryStatus.OpenedInWindow : ImageMemoryStatus.CachedInMemory
		        };

		        message = string.Format("AeTitle : {0}, IpAddress : {1}", receivedDicomElements.CallingAeTitle, receivedDicomElements.IpAddress);

		        OnDicomElementsReceived(receivedDicomElements);

		        message = "Store successfull! -- " + message;
		        hasError = false;
		    }
		    catch (Exception ex)
		    {
		        message = "Store failed! -- " + message + "  " + ex.Message;
		    }
		    finally
		    {
                dicomServiceWorkerUser.ShowMessage(message, hasError, false);
		    }

	    }

		private void OnImagePrinted(object sender, PrintJobEventArgs e)
	    {
            var message = string.Empty;
            var hasError = true;

		    try
		    {
                var page = e.Session.FilmBoxes[0];
                var elements = OtherImageFormats.RenderPage(page);

                var receivedDicomElements = new ReceivedDicomElements
                {
                    CallingAeTitle = e.CallingAeTitle,
                    CallingAeIpAddress = e.CallingAeIpAddress,
                    ReceivedDateTime = DateTime.Now,
                    Elements = elements,
                    ImageSource = ImageSource.Print,
                    ImageStatus = dicomServiceWorkerUser.OpenWhenReceived() ? ImageMemoryStatus.OpenedInWindow : ImageMemoryStatus.CachedInMemory
                };

                message = string.Format("AeTitle : {0}, IpAddress : {1}", receivedDicomElements.CallingAeTitle, receivedDicomElements.IpAddress);

                OnDicomElementsReceived(receivedDicomElements);

                message = "Print successfull! -- " + message;
                hasError = false;
            }
            catch (Exception ex)
            {
                message = "Print failed! -- " + message + "  " + ex.Message;
            }
            finally
            {
                dicomServiceWorkerUser.ShowMessage(message, hasError, false);
            }

	    }

		private void OnMpps(object sender, MppsEventArgs e)
		{
			var message = string.Empty;
			var hasError = true;
			try
			{
				var receivedDicomElements = new ReceivedDicomElements
				{
					CallingAeTitle = e.CallingAeTitle,
					CallingAeIpAddress = e.CallingAeIpAddress,
					ReceivedDateTime = DateTime.Now,
					Elements = e.DataSet,
					ImageSource = ImageSource.Mpps,
					ImageStatus = dicomServiceWorkerUser.OpenWhenReceived() ? ImageMemoryStatus.OpenedInWindow : ImageMemoryStatus.CachedInMemory
				};

				message = string.Format("AeTitle : {0}, IpAddress : {1}", receivedDicomElements.CallingAeTitle, receivedDicomElements.IpAddress);

				OnDicomElementsReceived(receivedDicomElements);
				

				message = "Mpps successfull! -- " + message;
				hasError = false;
			}
			catch (Exception ex)
			{
				message = "Mpps failed! -- " + message + "  " + ex.Message;
			}
			finally
			{
				dicomServiceWorkerUser.ShowMessage(message, hasError, false);
			}

		}

	    public CachedDicomElements CreateCachedFromReceived(ReceivedDicomElements receivedDicomElements)
	    {
			var cachedDicomElements = new CachedDicomElements()
			{
				CallingAeIpAddress = receivedDicomElements.CallingAeIpAddress,
				CallingAeTitle = receivedDicomElements.CallingAeTitle,
				FileName = receivedDicomElements.FileName,
				ReceivedDateTime = receivedDicomElements.ReceivedDateTime,
				ImageSource = receivedDicomElements.ImageSource,
				ImageStatus = receivedDicomElements.ImageStatus,
			};

		    if (receivedDicomElements.ImageSource == ImageSource.StorageCommitment)
		    {
			    foreach (var storageCommitmentReferenceDicomElement in GetStorageCommitmentReferenceDicomElements(receivedDicomElements))
			    {
					AddExamInfo(storageCommitmentReferenceDicomElement, ref cachedDicomElements);
			    }
		    }
		    else
		    {
			    AddExamInfo(receivedDicomElements, ref cachedDicomElements);
		    }

		    return cachedDicomElements;
	    }

	    private void AddExamInfo(ReceivedDicomElements receivedDicomElements, ref CachedDicomElements cachedDicomElements)
	    {
			cachedDicomElements.PatientId = AppendIfNotExist(cachedDicomElements.PatientId, receivedDicomElements.Elements.GetSafeStringValue(t.PatientID));
			cachedDicomElements.PatientName = AppendIfNotExist(cachedDicomElements.PatientName, receivedDicomElements.Elements.GetSafeStringValue(t.PatientName));
			cachedDicomElements.Modality = AppendIfNotExist(cachedDicomElements.Modality, receivedDicomElements.Elements.GetSafeStringValue(t.Modality));
			cachedDicomElements.AccessionNumber = AppendIfNotExist(cachedDicomElements.AccessionNumber, receivedDicomElements.Elements.GetSafeStringValue(t.AccessionNumber));
	    }

	    private string AppendIfNotExist(string source, string toBeAppended)
	    {
		    if (string.IsNullOrEmpty(source))
			    return toBeAppended;

		    const string seperator = " | ";
		    var temp = source + seperator;
		    return temp.Contains(toBeAppended + seperator)? source : source + seperator + toBeAppended;
	    }

	    public ReceivedDicomElements FindReceivedDicomElementsBySopInstanceUid(string sopInstanceUid)
	    {
		    return ListCachedElements.FirstOrDefault(receivedDicomElements => receivedDicomElements.Elements.GetSafeStringValue(t.SOPInstanceUID) == sopInstanceUid);
	    }

	    public IEnumerable<ReceivedDicomElements> GetStorageCommitmentReferenceDicomElements(
		    ReceivedDicomElements receivedDicomElements)
	    {
		    var refDicomElementsList = new List<ReceivedDicomElements>();

			var refSops = receivedDicomElements.Elements[t.ReferencedSOPSequence] as Sequence;
			if (refSops != null)
			{
				var count = refSops.Items.Count;
				for (var n = 0; n < count; n++)
				{
					var item = refSops.Items[n];

					var refSop = item[t.ReferencedSOPInstanceUID].Value;
					if (refSop != null)
					{
						var refElements = FindReceivedDicomElementsBySopInstanceUid(refSop.ToString());
						if (refElements != null)
							refDicomElementsList.Add(refElements);
					}
				}
			}

		    return refDicomElementsList;
	    }
    }
}
