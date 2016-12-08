using System;
using System.Text;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomEditor
{
    public class Generator
    {
        static Random random = new Random();

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

        enum Sex
        {
            M,
            F,
            O
        }

        public static string GetNewUID()
        {
            return Element.NewUid();
        }

        public static string GetSex()
        {
            return String.Format("{0}", ((Sex)random.Next(0,2)).ToString());
        }

        public static string GetPersonName()
        {
            return String.Format("{0}^{1}", lasts[random.Next(0, lasts.Length)], firsts[random.Next(0, firsts.Length)]);
        }

        public static string GetNumericString(int length)
        {
            return GetCharacterString("0123456789".ToCharArray(), length);
        }

        public static string GetAlphaString(int length)
        {
            return GetCharacterString("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), length);
        }

        public static string GetAlphaNumericString(int length)
        {
            return GetCharacterString("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray(), length);
        }

        public static string GetCharacterString(char[] characters, int length)
        {
            StringBuilder text = new StringBuilder();
            for (int n = 0; n < length; n++)
            {
                text.Append(characters[random.Next(0,length)]);
            }
            return text.ToString();
        }

        public static string GetDate()
        {
            int year = DateTime.Now.Year;
           return String.Format("{0:0000}{1:00}{2:00}", year-random.Next(0,100), random.Next(1,13), random.Next(1,29));
        }

        public static string GetTime()
        {
            return String.Format("{0:00}{1:00}{2:00}.000", random.Next(0,24), random.Next(0,60), random.Next(0,60));
        }

        public static string GetAscendingNumber()
        {
            // There are 10,000 ticks in a millisecond. 
            // wait a little more than a millisecond in order to insure that the milliseconds is increasing
            System.Threading.Thread.Sleep(new TimeSpan(10100));  
            TimeSpan since = DateTime.Now.Subtract(new DateTime(2012, 1, 1));
            return ((long)since.TotalMilliseconds).ToString();
        }
    }
}
