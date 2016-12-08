using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class Settings : IEnumerable<string>
    {
        private static object sentry = new object();
        private string application;
        private string path;
        private System.Collections.Specialized.NameValueCollection settings = null;

        public Settings(string application)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, application);
            this.application = application;
            Load();
        }

        public string Application
        {
            get
            {
                return application;
            }
            set
            {
                application = value;
            }
        }

        public string this[string name]
        {
            get
            {
                return settings[name];
            }
            set
            {
                settings[name] = value;
                Save();
            }
        }

        public void Remove(string name)
        {
            settings.Remove(name);
        }

        private void Load()
        {
            path = Path.Combine(SettingsFolder, String.Format("{0}.settings.xml", application));
            this.settings = new System.Collections.Specialized.NameValueCollection();
            System.IO.FileStream file = null;
            try
            {
                lock (sentry)
                {
                    file = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read);
                    if (file.Length > 0)
                    {
                        System.Runtime.Serialization.Formatters.Soap.SoapFormatter formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
                        settings = (System.Collections.Specialized.NameValueCollection)formatter.Deserialize(file);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                    file.Dispose();
                    file = null;
                }
            }
        }

        private void Save()
        {
            System.IO.FileStream file = null;
            try
            {
                lock (sentry)
                {
                    file = new System.IO.FileStream(path, System.IO.FileMode.Create);
                    System.Runtime.Serialization.Formatters.Soap.SoapFormatter formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
                    formatter.Serialize(file, settings);
                }
            }
            catch
            {
            }
            finally
            {
                if (file != null)
                {
                    file.Flush();
                    file.Dispose();
                    file = null;
                }
            }
        }

        private string SettingsFolder
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            foreach (string key in settings.AllKeys)
            {
                yield return key;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        #endregion
    }
}
