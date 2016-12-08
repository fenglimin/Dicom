using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomMwl
{
    class Program
    {
        static Random random;
        static int counter = 0;
        static int modulus = 4;
        static string path = @".\out";

        const string studyid = "1.2.840.113564.9.1.20051212xxxxxxx.21000704101";

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

        static void Main(string[] args)
        {
            random = new Random();
            int number = 1000;

            DirectoryInfo directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                Directory.Delete(path, true);
            }

            // if you run with a 0 as the command line argument, we run Gary's first set (Cynthia)
            // if you run with a 1 as the command line argument, we run Gary's second set (Steve)
            // if you run with a 2 as the command line argument, we run Annette's set
            // if you run with a 3 as the command line argument, we run Scott's set
            // if you run with a 4 as the command line argument, we run Gary's vita set.
            // if you run with a 5 as the command line argument, we run Cynthia's set (Lalena)
            // if you run with a 6 as the command line argument, we run Bradley's set
            // if you run with a 7 as the command line argument, we run the Laura set
            // if you run with a 8 as the command line argument, we run Dale's set (Kevin)
            // if you run with a 9 as the command line argument, we run Marina's set
            // if you run with a 10 as the command line argument, we run Frank's set
            // if you run with a 11 as the command line argument, we run Frank's other, other set
            // if you run with a 12 as the command line argument, we run Frank's yet another set

            // if you run with any other number, we create that many random records

            number = Int32.Parse(args[0]);
            // this is Gary's first set (Cynthia)
            if(number == 0)
            {
                CreateGenRadSeries();
                CreateTraumaSeries();
                CreateOrthoSeries();
            }
            // this is for Gary's second set (Steve)
            else if(number == 1)
            {
                CreateScenario1();
                CreateScenario2();
                CreateScenario3();
            }
            // this is Annette's set
            else if (number == 2)
            {
                modulus = 18;
                DateTime time = new DateTime(1968, 06, 26, 08, 30, 00);
                DateTime eleventhirty = new DateTime(1968, 06, 26, 11, 30, 00);
                DateTime one = new DateTime(1968, 06, 26, 13, 00, 00);
                DateTime five = new DateTime(1968, 06, 26, 17, 00, 00);
                while (true)
                {
                    counter++;
                    if (counter % modulus == 0)
                    {
                        counter++;
                    }
                    DataSet dicom = CreateRecord(null, null, null, null, time.ToString("HHmm00.000"), "");
                    WriteDicom(dicom);

                    time = time.AddMinutes(5.00);
                    if (time > eleventhirty && time < one)
                        time = one;
                    if (time > five)
                        break;
                }
            }
            // this is Scott's set
            else if (number == 3)
            {
                modulus = 18;
                DateTime time = new DateTime(1968, 06, 26, 08, 30, 00);
                DateTime eleventhirty = new DateTime(1968, 06, 26, 11, 30, 00);
                DateTime one = new DateTime(1968, 06, 26, 13, 00, 00);
                DateTime five = new DateTime(1968, 06, 26, 17, 00, 00);
                for(int n = 0; n < 15; n++)
                {
                    counter++;
                    DataSet dicom = CreateRecord(null, null, "PC102", null, time.ToString("HHmm00.000"), "");
                    dicom[t.AccessionNumber].Value = String.Format("A{0:0000}", counter);
                    WriteDicom(dicom);

                    time = time.AddMinutes(5.00);
                }
            }
            // this is Gary's vita set
            else if (number == 4)   // read input from files like vita.csv and vita2.csv
            {
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(args[1]);
                while ((line = file.ReadLine()) != null)
                {
                    string [] columns = line.Split(",".ToCharArray());
                    
                    DataSet dicom = CreateRecord(Int32.Parse(columns[2]).ToString("D6"), null, null, columns[0]+"^"+columns[1], null, "");
                    WriteDicom(dicom);
                }
                file.Close();
            }
            // this is Cynthia's set
            else if (number == 5) // 50 patients each with two records, 100 records in all
            {
                modulus = 18;
                string id, name;
                DateTime time = new DateTime(1968, 06, 26, 08, 30, 00);
                DateTime eleventhirty = new DateTime(1968, 06, 26, 11, 30, 00);
                DateTime one = new DateTime(1968, 06, 26, 13, 00, 00);
                DateTime five = new DateTime(1968, 06, 26, 17, 00, 00);
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
            else if (number == 6) // 
            {
                DataSet dicom;

                dicom = CreateRecord(null, null, null, "ghæijøklå^ABÆCDØEFÅ^?", null, "æøåÆØÅ");
                counter++; 
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "abcdefg^ABCDEGF^A", null, "WXYZwxyz");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "abcdefg^ABCDEGF^A", null, "WXYZwxyz");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "abcdefg^ABCDEGF^A", null, "WXYZwxyz");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "abcdefg^ABCDEGF^A", null, "WXYZwxyz");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "abcåäöxyž^ABCÅÄÖXYŽ^?", null, "ÅÄÖŽåäöž");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "ëï?çàèù^éâêîôûçàèù^A", null, "àè?");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "ABCäöüßXYZ^ABCÄÖÜßXYZ^?", null, "ÄäÖöÜü?");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "áéóú^áéóú^?", null, "áéóú");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "ghæijøklå^ABÆCDØEFÅ^?", null, "æøåÆØÅ");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "áâãàéêíóôõú^ÇÁÂÃÀÇÉÊÍÓÔÕÚ^?", null, "ÇÁÂÃÀÇÉÊÍÓÔÕ?áâãàéêíóôõ?");
                counter++;
                WriteDicom(dicom);

                dicom = CreateRecord(null, null, null, "áâãàéêíóôõú^ÇÁÂÃÀÇÉÊÍÓÔÕÚ^?", null, "ÇÁÂÃÀÇÉÊÍÓÔÕ?áâãàéêíóôõ?");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "Ñüñ^Ñáéíóú^?", null, "Ñáéíóú");
                counter++;
                WriteDicom(dicom);
                
                dicom = CreateRecord(null, null, null, "Ñüñ^Ñáéíóú^?", null, "Ñáéíóú");
                counter++;
                WriteDicom(dicom);

                dicom = CreateRecord(null, null, null, "abåäöyz^ABÅÄÖYZ^?", null, "ÅÄÖåäö");
                counter++;
                WriteDicom(dicom);
            }
            // this is Laura's
            else if (number == 7)
            {
                // one record every 2 minutes, with a set of procedure codes.

                string[] codes = { "PC810", "PC840", "P830P", "P832H", "P832T" };

                string id, name;
                DateTime time = new DateTime(1968, 06, 26, 00, 00, 00);
                // 30 records per hour for a 24 hour period (1 record every 2 minutes)
                for (int n = 0; n < 24 * 30; n++)
                {
                    id = String.Format("{0:00000000}", counter++);
                    name = String.Format("{0}^{1}", lasts[random.Next(0, lasts.Length)], firsts[random.Next(0, firsts.Length)]);

                    DataSet dicom = CreateRecord(id, studyid.Replace("xxxxxxx", String.Format("{0:0000000}", random.Next(2, 9999999))), codes[n%codes.Length], name, time.ToString("HHmmss.000"), "");
                    //dicom.Set(t.ReferringPhysiciansName, "VELI^Velianou^James^Louis^^^");
                    WriteDicom(dicom);

                    time = time.AddSeconds(120);
                }
            }
            else if (number == 8)
            {
                number = 500;
                for (int n = 0; n < number; n++)
                {
                    counter++;
                    if (counter % modulus == 0)
                    {
                        counter++;
                    }

                    DataSet dicom = CreateRecord(null, null, null, null, null, "");
                    WriteDicom(dicom);
                }
            }
            else if (number == 9)
            {
                number = 33;

                string[] ids = new string[] { "", "A1", "A2", "A3", "A4", "A01", "A5", "A010", "A011", "A04",
                "A05",  "A06", "A07", "A08", "A09", "A10", "A11", "A12", "A13", "A14", "A15", "A16", "A17", 
                "A18", "A19", "A20", "A21", "A22", "A023", "A024", "A025", "A026", "A027", "A028" };

                string[] codes = new string[] { "", "P997", "P998", "P1000", "P1001", "P1002", "P1003", "P1004", "P1005", "P1006",
                "P1007",  "P1008", "P1009", "P1010", "P1011", "P1012", "P1013", "P1014", "P1015", "P1016", "P1017", "P1018", "P1019", 
                "P1020", "P1021", "P1022", "P1023", "P1024", "PC201", "PC702", "PC242", "PC302", "PC361", "PC501" };

                string[] names = new string[] { "", "VeryLowBirthWeight_1", "VeryLowBirthWeight_1.49kg", "VeryLowBirthWeight_1.50kg", 
                    "VeryLowBirthWeight_1.51kg", "LowBirthWeight_2kg", "LowBirthWeight_2.49kg", "LowBirthWeight_2.50kg", "LowBirthWeight_2.51kg", "NewBorn_1 day", "NewBorn_1 day < One month", 
                    "NewBorn_One month", "NewBorn_1 day >  One month", "Infant_1 year", "Infant_1 day < 2 years", "Infant_2 years", 
                    "Infant_1 day > 2 years", "Child_5 years", "Child_1 day < 11 years", "Child_11 years", "Child_1 day > 11 years", 
                    "Preadolescent_12 years", "Preadolescent_1 day < 13 year", "Preadolescent_13 years", "Preadolescent_1 day > 13 years", 
                    "Adolescent_1 day <  21 years", "Adolescent_21 years", "Adolescent_1 day > 21 years", "Very Low BirthWeight", 
                    "Low Birth Weight", "Infant", "Child", "Preadolescent", "Adolescent"  };

                string[] descriptions = new string[] { "", "VeryLowBirthWeight_0.5kg", "VeryLowBirthWeight_1.49kg", "VeryLowBirthWeight_1.50kg", 
                    "VeryLowBirthWeight_1.51kg", "LowBirthWeight_2kg", "LowBirthWeight_2.49kg", "LowBirthWeight_2.50kg", "LowBirthWeight_2.51kg", 
                    "NewBorn_1 day", "NewBorn_1 day < One month", "NewBorn_One month", "NewBorn_1 day >  One month", "Infant_1 year", "Infant_1 day < 2 years", 
                    "Infant_2 years", "Infant_1 day > 2 years", "Child_5 years", "Child_1 day < 11 years", "Child_11 years", "Child_1 day > 11 years", 
                    "Preadolescent_12 years", "Preadolescent_1 day < 13 years", "Preadolescent_13 years", "Preadolescent_1 day > 13 years", 
                    "Adolescent_1 day <  21 years", "Adolescent_21 years", "Adolescent_1 day > 21 years", "Hand - 1 View", "Skull - 2 Views", 
                    "Shoulder - 2 Views", "Foot - 2 Views", "Pelvis - 2 Views", "C-Spine - 1 View" };

                string[] weights = new string[] { "", "0.5", "1.49", "1.50", "1.51", "2", "2.49", "2.50", "2.51", 
                    "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "1.49", "2.49", "", "", "", "" };

                for (int n = 0; n < number; n++)
                {
                    counter++;
                    DataSet dicom = CreateRecord(ids[counter], null, codes[counter], names[counter], null, descriptions[counter]);

                    if (counter <= 8 && counter >= 28 && counter <= 29)
                    {
                        dicom[t.PatientWeight].Value = weights[counter];
                    }

                    WriteDicom(dicom);
                }
            }
            // this is Frank's
            else if (number == 10)
            {
                // I need 132 patients
                // With Procedure Codes of PC100, PC103, PC105, PC107 (randomize if you want, I can't remember what the tag is)
                // ScheduledProcedureStepStartTime of every 5 minutes from 7:00AM to 6:00 PM
                // Every other info can be random

                string[] codes = { "PC100", "PC103", "PC105", "PC107" };

                string id, name;
                DateTime time = new DateTime(1968, 06, 26, 07, 00, 00);
                // 12 records per hour for a 11 hour period (1 record every 12 minutes)
                for (int n = 0; n < 12 * 11; n++)
                {
                    id = String.Format("{0:00000000}", counter++);
                    name = String.Format("{0}^{1}", lasts[random.Next(0, lasts.Length)], firsts[random.Next(0, firsts.Length)]);

                    DataSet dicom = CreateRecord(id, studyid.Replace("xxxxxxx", String.Format("{0:0000000}", random.Next(2, 9999999))), codes[n % codes.Length], name, time.ToString("HHmmss.000"), "");
                    WriteDicom(dicom);

                    time = time.AddSeconds(5 * 60);
                }
            }
            // this is Frank's otehr other
            else if (number == 11)
            {
                // I need 1000 patients, exam times a minute apart
                // With Procedure Codes of PC107, PC111, PC201, PC403
                // All other info is random

                string[] codes = { "PC107", "PC111", "PC201", "PC403" };

                string id, name;
                // start at 6:00 AM
                DateTime time = new DateTime(1968, 06, 26, 06, 00, 00);
                // 1000 records, one every minute
                for (int n = 0; n < 1000; n++)
                {
                    id = String.Format("{0:00000000}", counter++);
                    name = String.Format("{0}^{1}", lasts[random.Next(0, lasts.Length)], firsts[random.Next(0, firsts.Length)]);

                    DataSet dicom = CreateRecord(id, studyid.Replace("xxxxxxx", String.Format("{0:0000000}", random.Next(2, 9999999))), codes[n % codes.Length], name, time.ToString("HHmmss.000"), "");
                    WriteDicom(dicom);

                    time = time.AddSeconds(60);
                }
            }
            else if (number == 12)
            {
/*
When you get a chance could you create me 2000 patients in the following format:

Last name = Pry(number of patient, eg Pry0001)
First name = First(number of patient, eg First0001)
Middle name = Mid(number of patient, eg Mid0001)
Patient ID = 888(number of patient, eg 8880001)

Procedure code = PC100

Patients are created 10 minutes apart
                 * */

                string id, name;
                // start at 12:01 AM
                DateTime time = new DateTime(1968, 06, 26, 00, 01, 00);

                // one every minute
                for (int n = 1; n <= 60*24; n++)
                {
                    name = String.Format("Pry{0:0000}^First{0:0000}^Mid{0:0000}", n);
                    id = String.Format("888{0:0000}", n);

                    DataSet dicom = CreateRecord(id, studyid.Replace("xxxxxxx", String.Format("{0:0000000}", random.Next(2, 9999999))), "PC100", name, time.ToString("HHmmss.000"), "");
                    WriteDicom(dicom);

                    time = time.AddSeconds(60);
                }
            }
            else
            {
                for (int n = 0; n < number; n++)
                {
                    counter++;
                    DataSet dicom = CreateRecord(null, null, null, null, null, "");
                    WriteDicom(dicom);
                }
            }
        }

        enum Sex
        {
            M,
            F,
            O
        }

        static DataSet CreateRecord(string id, string uid, string code, string name, string time, string description)
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
            dicom.Add(t.ReferringPhysicianName, "Sadler");

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
            dicom.Add(t.PatientBirthDate, String.Format("{0:0000}{1:00}{2:00}", 2008-random.Next(0,100), random.Next(1,13), random.Next(1,29)));
            dicom.Add(t.PatientBirthTime, String.Format("{0:00}{1:00}{2:00}.000", random.Next(0,24), random.Next(0,60), random.Next(0,60)));
            // no other
            dicom.Add(t.PatientSex, String.Format("{0}", ((Sex)random.Next(0,2)).ToString()));
            dicom.Add(t.OtherPatientIDs, "");
            dicom.Add(t.OtherPatientNames, "");
            dicom.Add(t.PatientComments, "");
            id = id.Replace("P", "");
            id = id.Replace("A", "");
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

        static void WriteDicom(DataSet dicom)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                directory.Create();
            }
            string temp = Path.Combine(path, (string)dicom[t.PatientID].Value);
            FileInfo file = new FileInfo(temp + ".dcm");
            if (file.Exists)
                temp += "1";
            dicom.Write(temp + ".dcm");
        }

        static void WriteRecord(string code, string id)
        {
            counter++;
            DataSet dicom = CreateRecord(id, null, code, null, null, "");
            WriteDicom(dicom);
            Console.Write(".");
        }

        static void WriteRecord(string code, string id, string last, string first)
        {
            counter++;
            DataSet dicom = CreateRecord(id, null, code, last+"^"+first, null, "");
            WriteDicom(dicom);
            Console.Write(".");
        }

        static void CreateGenRadSeries()
        {
            counter = 1;
            path = @"C:\Gary\GenRad\ModalityWorklist";
            if(Directory.Exists(path))
                Directory.Delete(path, true);

            WriteRecord("PC111", "P10033");
            WriteRecord("PC400", "P10044");
            WriteRecord("PC403", "P10055");
            WriteRecord("PC111", "P10066");
            WriteRecord("PC107", "P10077");
            WriteRecord("PC301", "P10088");
            WriteRecord("PC503", "P10099");
            WriteRecord("PC111", "P10100");
            WriteRecord("PC403", "P11111");
            WriteRecord("PC111", "P11112");
            WriteRecord("PC400", "P11113");
            WriteRecord("PC112", "P11114");
            WriteRecord("PC111", "P11115");
            WriteRecord("PC112", "P11116");
            WriteRecord("PC111", "P11117");
            WriteRecord("PC111", "P11118");
            WriteRecord("PC400", "P11119");
            WriteRecord("PC111", "P11120");
            WriteRecord("PC112", "P11121");
            WriteRecord("PC403", "P11122");
            WriteRecord("PC112", "P11123");
            WriteRecord("PC400", "P11124");
            WriteRecord("PC111", "P11125");
            WriteRecord("PC301", "P11126");
            WriteRecord("PC111", "P11127");
            WriteRecord("PC503", "P11128");
            WriteRecord("PC111", "P11129");
            WriteRecord("PC111", "P11130");
            WriteRecord("PC403", "P11131");
            WriteRecord("PC403", "P11132");
            WriteRecord("PC112", "P11133");
            WriteRecord("PC107", "P11134");
            WriteRecord("PC111", "P11135");
            WriteRecord("PC400", "P11136");
            WriteRecord("PC111", "P11137");
            WriteRecord("PC301", "P11138");
            WriteRecord("PC112", "P11139");
            WriteRecord("PC111", "P11140");
            WriteRecord("PC201", "P11141");
            WriteRecord("PC111", "P11142");
            WriteRecord("PC400", "P11143");
            WriteRecord("PC112", "P11144");
            WriteRecord("PC111", "P11145");
            WriteRecord("PC201", "P11146");
            WriteRecord("PC111", "P11147");
            WriteRecord("PC400", "P11148");
            WriteRecord("PC112", "P11149");
            WriteRecord("PC111", "P11150");
            WriteRecord("PC107", "P11151");
            WriteRecord("PC111", "P11152");
            WriteRecord("PC403", "P11153");
            WriteRecord("PC400", "P11154");
            WriteRecord("PC111", "P11155");
            WriteRecord("PC301", "P11156");
            WriteRecord("PC112", "P11157");
            WriteRecord("PC112", "P11158");
            WriteRecord("PC403", "P11159");
            WriteRecord("PC112", "P11160");
            WriteRecord("PC112", "P11161");
            WriteRecord("PC111", "P11162");
            WriteRecord("PC502", "P11163");
            WriteRecord("PC111", "P11164");
            WriteRecord("PC400", "P11165");
            WriteRecord("PC403", "P11166");
            WriteRecord("PC111", "P11167");
            WriteRecord("PC111", "P11168");
            WriteRecord("PC107", "P11169");
            WriteRecord("PC112", "P11170");
            WriteRecord("PC400", "P11171");
            WriteRecord("PC503", "P11172");
            WriteRecord("PC111", "P11173");
            WriteRecord("PC403", "P11174");
            WriteRecord("PC112", "P11175");
            WriteRecord("PC111", "P11176");
            WriteRecord("PC107", "P11177");
            WriteRecord("PC400", "P11178");
            WriteRecord("PC111", "P11179");
            WriteRecord("PC400", "P11180");
            WriteRecord("PC112", "P11181");
            WriteRecord("PC107", "P11182");
            WriteRecord("PC112", "P11183");
            WriteRecord("PC400", "P11184");
            WriteRecord("PC111", "P11185");
            WriteRecord("PC400", "P11186");
            WriteRecord("PC403", "P11187");
            WriteRecord("PC400", "P11188");
            WriteRecord("PC111", "P11189");
            WriteRecord("PC112", "P11190");
            WriteRecord("PC403", "P11191");
            WriteRecord("PC111", "P11192");
            WriteRecord("PC301", "P11193");
            WriteRecord("PC107", "P11194");
            WriteRecord("PC111", "P11195");
            WriteRecord("PC400", "P11196");
            WriteRecord("PC112", "P11197");
            WriteRecord("PC111", "P11198");
            WriteRecord("PC400", "P11199");
            WriteRecord("PC403", "P11200");
            WriteRecord("PC111", "P11201");
            WriteRecord("PC201", "P11202");
            WriteRecord("PC403", "P11203");
            WriteRecord("PC107", "P11204");
            WriteRecord("PC111", "P11205");
            WriteRecord("PC502", "P11206");
            WriteRecord("PC111", "P11207");
            WriteRecord("PC112", "P11208");
            WriteRecord("PC400", "P11209");
            WriteRecord("PC111", "P11210");
            WriteRecord("PC112", "P11211");
            WriteRecord("PC201", "P11212");
            WriteRecord("PC111", "P11213");
            WriteRecord("PC502", "P11214");
            WriteRecord("PC400", "P11215");
            WriteRecord("PC111", "P11216");
            WriteRecord("PC111", "P11217");
            WriteRecord("PC112", "P11218");
            WriteRecord("PC403", "P11219");
            WriteRecord("PC107", "P11220");
            WriteRecord("PC503", "P11221");
            WriteRecord("PC111", "P11222");
            WriteRecord("PC400", "P11223");
            WriteRecord("PC112", "P11224");
            WriteRecord("PC111", "P11225");
            WriteRecord("PC403", "P11226");
            WriteRecord("PC400", "P11227");
            WriteRecord("PC111", "P11228");
            WriteRecord("PC112", "P11229");
            WriteRecord("PC111", "P11230");
            WriteRecord("PC111", "P11231");
            WriteRecord("PC112", "P11232");
            WriteRecord("PC112", "P11233");
            WriteRecord("PC400", "P11234");
            WriteRecord("PC111", "P11235");
            WriteRecord("PC400", "P11236");
            WriteRecord("PC111", "P11237");

            Console.WriteLine();
        }

        static void CreateTraumaSeries()
        {
            counter = 1;
            path = @"C:\Gary\Trauma\ModalityWorklist";
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            WriteRecord("PC112", "P10033");
            WriteRecord("PC400", "P10044");
            WriteRecord("PC403", "P10055");
            WriteRecord("PC502", "P10066");
            WriteRecord("PC403", "P10077");
            WriteRecord("PC301", "P10088");
            WriteRecord("PC112", "P10099");
            WriteRecord("PC400", "P10100");
            WriteRecord("PC112", "P11111");
            WriteRecord("PC403", "P11112");
            WriteRecord("PC107", "P11113");
            WriteRecord("PC112", "P11114");
            WriteRecord("PC112", "P11115");
            WriteRecord("PC400", "P11116");
            WriteRecord("PC112", "P11117");
            WriteRecord("PC403", "P11118");
            WriteRecord("PC502", "P11119");
            WriteRecord("PC112", "P11120");
            WriteRecord("PC403", "P11121");
            WriteRecord("PC112", "P11122");
            WriteRecord("PC112", "P11123");
            WriteRecord("PC403", "P11124");
            WriteRecord("PC400", "P11125");
            WriteRecord("PC112", "P11126");
            WriteRecord("PC400", "P11127");
            WriteRecord("PC403", "P11128");
            WriteRecord("PC503", "P11129");
            WriteRecord("PC400", "P11130");
            WriteRecord("PC112", "P11131");
            WriteRecord("PC301", "P11132");
            WriteRecord("PC112", "P11133");
            WriteRecord("PC403", "P11134");
            WriteRecord("PC400", "P11135");
            WriteRecord("PC403", "P11136");
            WriteRecord("PC112", "P11137");
            WriteRecord("PC503", "P11138");
            WriteRecord("PC107", "P11139");
            WriteRecord("PC403", "P11140");
            WriteRecord("PC112", "P11141");
            WriteRecord("PC502", "P11142");
            WriteRecord("PC112", "P11143");
            WriteRecord("PC201", "P11144");
            WriteRecord("PC112", "P11145");
            WriteRecord("PC403", "P11146");
            WriteRecord("PC400", "P11147");
            WriteRecord("PC112", "P11148");
            WriteRecord("PC403", "P11149");
            WriteRecord("PC112", "P11150");
            WriteRecord("PC112", "P11151");
            WriteRecord("PC400", "P11152");
            WriteRecord("PC403", "P11153");
            WriteRecord("PC112", "P11154");
            WriteRecord("PC503", "P11155");
            WriteRecord("PC112", "P11156");
            WriteRecord("PC107", "P11157");    
            WriteRecord("PC403", "P11158");
            WriteRecord("PC112", "P11159");
            WriteRecord("PC301", "P11160");    
            WriteRecord("PC403", "P11161");
            WriteRecord("PC112", "P11162");
            WriteRecord("PC112", "P11163");
            WriteRecord("PC112", "P11164");
            WriteRecord("PC403", "P11165");
            WriteRecord("PC400", "P11166");
            WriteRecord("PC403", "P11167");
            WriteRecord("PC301", "P11168");
            WriteRecord("PC112", "P11169");
            WriteRecord("PC403", "P11170");
            WriteRecord("PC112", "P11171");
            WriteRecord("PC201", "P11172");
            WriteRecord("PC403", "P11173");
            WriteRecord("PC112", "P11174");
            WriteRecord("PC400", "P11175");
            WriteRecord("PC403", "P11176");
            WriteRecord("PC112", "P11177");
            WriteRecord("PC107", "P11178");
            WriteRecord("PC112", "P11179");
            WriteRecord("PC403", "P11180");
            WriteRecord("PC112", "P11181");
            WriteRecord("PC502", "P11182");
            WriteRecord("PC112", "P11183");
            WriteRecord("PC112", "P11184");
            WriteRecord("PC503", "P11185");
            WriteRecord("PC112", "P11186");
            WriteRecord("PC403", "P11187");
            WriteRecord("PC201", "P11188");
            WriteRecord("PC112", "P11189");
            WriteRecord("PC400", "P11190");
            WriteRecord("PC112", "P11191");
            WriteRecord("PC112", "P11192");
            WriteRecord("PC403", "P11193");
            WriteRecord("PC502", "P11194");
            WriteRecord("PC112", "P11195");
            WriteRecord("PC400", "P11196");
            WriteRecord("PC112", "P11197");
            WriteRecord("PC112", "P11198");
            WriteRecord("PC400", "P11199");
            WriteRecord("PC403", "P11200");
            WriteRecord("PC112", "P11201");
            WriteRecord("PC201", "P11202");
            WriteRecord("PC112", "P11203");
            WriteRecord("PC403", "P11204");
            WriteRecord("PC502", "P11205");
            WriteRecord("PC403", "P11206");
            WriteRecord("PC112", "P11207");
            WriteRecord("PC400", "P11208");

            Console.WriteLine();
        }

        static void CreateOrthoSeries()
        {
            counter = 1;
            path = @"C:\Gary\Ortho\ModalityWorklist";
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            WriteRecord("PC403", "P10033");
            WriteRecord("PC111", "P10044");
            WriteRecord("PC502", "P10055");
            WriteRecord("PC400", "P10066");
            WriteRecord("PC403", "P10077");
            WriteRecord("PC502", "P10088");
            WriteRecord("PC201", "P10099");
            WriteRecord("PC107", "P10100");
            WriteRecord("PC301", "P11111");
            WriteRecord("PC107", "P11112");
            WriteRecord("PC301", "P11113");
            WriteRecord("PC403", "P11114");
            WriteRecord("PC502", "P11115");
            WriteRecord("PC107", "P11116");
            WriteRecord("PC301", "P11117");
            WriteRecord("PC201", "P11118");
            WriteRecord("PC403", "P11119");
            WriteRecord("PC403", "P11120");
            WriteRecord("PC107", "P11121");
            WriteRecord("PC111", "P11122");
            WriteRecord("PC502", "P11123");
            WriteRecord("PC107", "P11124");
            WriteRecord("PC201", "P11125");
            WriteRecord("PC502", "P11126");
            WriteRecord("PC301", "P11127");
            WriteRecord("PC107", "P11128");
            WriteRecord("PC201", "P11129");
            WriteRecord("PC403", "P11130");
            WriteRecord("PC502", "P11131");
            WriteRecord("PC301", "P11132");
            WriteRecord("PC107", "P11133");
            WriteRecord("PC111", "P11134");
            WriteRecord("PC107", "P11135");
            WriteRecord("PC403", "P11136");
            WriteRecord("PC400", "P11137");
            WriteRecord("PC502", "P11138");
            WriteRecord("PC301", "P11139");
            WriteRecord("PC111", "P11140");
            WriteRecord("PC201", "P11141");
            WriteRecord("PC301", "P11142");
            WriteRecord("PC502", "P11143");
            WriteRecord("PC403", "P11144");
            WriteRecord("PC502", "P11145");
            WriteRecord("PC301", "P11146");
            WriteRecord("PC107", "P11147");
            WriteRecord("PC301", "P11148");
            WriteRecord("PC502", "P11149");
            WriteRecord("PC111", "P11150");
            WriteRecord("PC201", "P11151");
            WriteRecord("PC403", "P11152");
            WriteRecord("PC107", "P11153");
            WriteRecord("PC201", "P11154");
            WriteRecord("PC403", "P11155");
            WriteRecord("PC502", "P11156");
            WriteRecord("PC201", "P11157");
            WriteRecord("PC301", "P11158");
            WriteRecord("PC107", "P11159");
            WriteRecord("PC201", "P11160");
            WriteRecord("PC107", "P11161");
            WriteRecord("PC403", "P11162");
            WriteRecord("PC107", "P11163");
            WriteRecord("PC111", "P11164");
            WriteRecord("PC201", "P11165");
            WriteRecord("PC403", "P11166");
            WriteRecord("PC107", "P11167");
            WriteRecord("PC502", "P11168");
            WriteRecord("PC107", "P11169");
            WriteRecord("PC403", "P11170");
            WriteRecord("PC107", "P11171");

            Console.WriteLine();
        }

        static void CreateScenario1()
        {
            counter = 1;
            path = @"C:\Gary\Scenario1\ModalityWorklist";
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            WriteRecord("PC600", "P11250", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11251", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11252", "Table Top", "Portrait");
            WriteRecord("PC700", "P11253", "Table Top", "Landscape");
            WriteRecord("PC800", "P11254", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11255", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11256", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11257", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11258", "Table Top", "Portrait");
            WriteRecord("PC700", "P11259", "Table Top", "Landscape");
            WriteRecord("PC800", "P11260", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11261", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11262", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11263", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11264", "Table Top", "Portrait");
            WriteRecord("PC700", "P11265", "Table Top", "Landscape");
            WriteRecord("PC800", "P11266", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11267", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11268", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11269", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11270", "Table Top", "Portrait");
            WriteRecord("PC700", "P11271", "Table Top", "Landscape");
            WriteRecord("PC800", "P11272", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11273", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11274", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11275", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11276", "Table Top", "Portrait");
            WriteRecord("PC700", "P11277", "Table Top", "Landscape");
            WriteRecord("PC800", "P11278", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11279", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11280", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11281", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11282", "Table Top", "Portrait");
            WriteRecord("PC700", "P11283", "Table Top", "Landscape");
            WriteRecord("PC800", "P11284", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11285", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11286", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11287", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11288", "Table Top", "Portrait");
            WriteRecord("PC700", "P11289", "Table Top", "Landscape");
            WriteRecord("PC800", "P11290", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11291", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11292", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11293", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11294", "Table Top", "Portrait");
            WriteRecord("PC700", "P11295", "Table Top", "Landscape");
            WriteRecord("PC800", "P11296", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11297", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11298", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11299", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11300", "Table Top", "Portrait");
            WriteRecord("PC700", "P11301", "Table Top", "Landscape");
            WriteRecord("PC800", "P11302", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11303", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11304", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11305", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11306", "Table Top", "Portrait");
            WriteRecord("PC700", "P11307", "Table Top", "Landscape");
            WriteRecord("PC800", "P11308", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11309", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11310", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11311", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11312", "Table Top", "Portrait");
            WriteRecord("PC700", "P11313", "Table Top", "Landscape");
            WriteRecord("PC800", "P11314", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11315", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11316", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11317", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11318", "Table Top", "Portrait");
            WriteRecord("PC700", "P11319", "Table Top", "Landscape");
            WriteRecord("PC800", "P11320", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11321", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11322", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11323", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11324", "Table Top", "Portrait");
            WriteRecord("PC700", "P11325", "Table Top", "Landscape");
            WriteRecord("PC800", "P11326", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11327", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11328", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11329", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11330", "Table Top", "Portrait");
            WriteRecord("PC700", "P11331", "Table Top", "Landscape");
            WriteRecord("PC800", "P11332", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11333", "Table Bucky", "Landscape");
            WriteRecord("PC600", "P11334", "Wall Stand", "Portrait");
            WriteRecord("PC600", "P11335", "Wall Stand", "Landscape");
            WriteRecord("PC700", "P11336", "Table Top", "Portrait");
            WriteRecord("PC700", "P11337", "Table Top", "Landscape");
            WriteRecord("PC800", "P11338", "Table Bucky", "Portrait");
            WriteRecord("PC800", "P11339", "Table Bucky", "Landscape");

            Console.WriteLine();
        }

        static void CreateScenario2()
        {
            counter = 1;
            path = @"C:\Gary\Scenario2\ModalityWorklist";
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            WriteRecord("PC600", "P11550", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11551", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11552", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11553", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11554", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11555", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11556", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11557", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11558", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11559", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11560", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11561", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11562", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11563", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11564", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11565", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11566", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11567", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11568", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11569", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11570", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11571", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11572", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11573", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11574", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11575", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11576", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11577", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11578", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11579", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11580", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11581", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11582", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11583", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11584", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11585", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11586", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11587", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11588", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11589", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11590", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11591", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11592", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11593", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11594", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11595", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11596", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11597", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11598", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11599", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11600", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11601", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11602", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11603", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11604", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11605", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11606", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11607", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11608", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11609", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11610", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11611", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11612", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11613", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11614", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11615", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11616", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11617", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11618", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11619", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11620", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11621", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11622", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11623", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11624", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11625", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11626", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11627", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11628", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11629", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11630", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11631", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11632", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11633", "Wireless", "T-Top Landscape");
            WriteRecord("PC600", "P11634", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P11635", "Tethered", "Wall-Landscape");
            WriteRecord("PC750", "P11636", "Tethered", "Wall-Portrait");
            WriteRecord("PC750", "P11637", "Tethered", "Wall-Landscape");
            WriteRecord("PC850", "P11638", "Wireless", "T-Top Portrait");
            WriteRecord("PC850", "P11639", "Wireless", "T-Top Landscape");

            Console.WriteLine();
        }

        static void CreateScenario3()
        {
            counter = 1;
            path = @"C:\Gary\Scenario3\ModalityWorklist";
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            WriteRecord("PC600", "P12111", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12112", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12113", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12114", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12115", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12116", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12117", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12118", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12119", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12120", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12121", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12122", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12123", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12124", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12125", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12126", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12127", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12128", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12129", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12130", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12131", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12132", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12133", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12134", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12135", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12136", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12137", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12138", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12139", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12140", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12141", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12142", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12143", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12144", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12145", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12146", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12147", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12148", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12149", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12150", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12151", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12152", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12153", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12154", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12155", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12156", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12157", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12158", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12159", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12160", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12161", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12162", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12163", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12164", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12165", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12166", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12167", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12168", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12169", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12170", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12171", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12172", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12173", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12174", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12175", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12176", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12177", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12178", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12179", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12180", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12181", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12182", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12183", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12184", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12185", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12186", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12187", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12188", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12189", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12190", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12191", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12192", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12193", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12194", "Wireless", "T-Bucky Landscape");
            WriteRecord("PC600", "P12195", "Tethered", "Wall-Portrait");
            WriteRecord("PC600", "P12196", "Tethered", "Wall-Landscape");
            WriteRecord("PC700", "P12197", "Wireless", "T-Top Portrait");
            WriteRecord("PC700", "P12198", "Wireless", "T-Top Landscape");
            WriteRecord("PC800", "P12199", "Wireless", "T-Bucky Portrait");
            WriteRecord("PC800", "P12200", "Wireless", "T-Bucky Landscape");

            Console.WriteLine();
        }

    }
}
