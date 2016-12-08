using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Represents a collection of Dicom DataSets.
    /// </summary>
    /// <remarks>The collection can be held in memory or cached in disk files.  Note: this class
    /// relies on garbage collection being run to clean up after itself.</remarks>
    public class RecordCollection : IDisposable, IEnumerable<Elements>
    {
        #region Fields

        /// <summary>
        /// If the collection is not held in memory this member holds a DirectoryInfo
        /// instance for the folder used to store individual items, null otherwise.  
        /// Used internally to tell the difference.
        /// </summary>
        private DirectoryInfo info = null;

        /// <summary>
        /// An internal collection of Dicom Data Sets or file names that represent
        /// Dicom Data Sets.
        /// </summary>
        /// <remarks>Sometimes the contents of this collection are DataSets, sometimes it is pathnames.  This is
        /// the reason to use a collection instead of a generic, we want it to be non-typed.</remarks>
        private ArrayList collection;

        /// <summary>
        /// Whether or not the instance has been disposed.
        /// </summary>
        private bool disposed = false;

        private bool existing = false;

        #endregion Fields

        #region Constructors, destructor and IDisposable overrides

        /// <summary>
        /// Initializes a new instance of the RecordCollection class that is held in memory.
        /// </summary>
        public RecordCollection()
            : this(null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RecordCollection class that is backed by disk files.
        /// </summary>
        /// <param name="folder">The root folder within which to create a folder used by this instance
        /// to store individual items.</param>
        /// <param name="existing">Indicates whether the folder is to be used as a base folder or
        /// as the folder.</param>
        /// <remarks>If the folder argument is null, the collection will be held in memory.
        /// When the instance is garbage collected, the private folder and the individual items are 
        /// deleted.  
        /// If the folder is not null and the existing flag is true, the folder argument is the folder 
        /// that will contain the cache.  If the existing flag is false, the folder is used as a base 
        /// folder, and the cache is created in a folder within the base folder.
        /// </remarks>
        public RecordCollection(string folder, bool existing)
        {
            this.existing = existing;
            if (this.existing)
            {
                this.info = new DirectoryInfo(folder);
                if (!info.Exists)
                {
                    // TODO likely does not work all the time
                    this.info = Directory.CreateDirectory(folder);
                }
            }
            else
            {
                if (folder != null)
                {
                    // each instance must have its own unique folder name
                    // also each folder is prefaced with mwl so that we can clean them up later if needed.
                    this.info = Directory.CreateDirectory(Path.Combine(folder, "mwl." + Path.GetRandomFileName()));
                }
            }
            collection = new ArrayList(500);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~RecordCollection()      
        {
            // Dispose was not called from user code
            Dispose(false);
        }

        /// <summary>
        /// Performs tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Since we are called from user code, we suppress garbage collection.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        /// <param name="disposing">Whether or not the method is called from user code.</param>
        /// <remarks>Do not make this method virtual.  A derived class should not be able to override 
        /// this method.</remarks>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources here if we ever get any.
                }
                // Dispose unmanaged resources.
                if (info != null && !existing)
                {
                    info.Delete(true);
                }
            }
            disposed = true;
        }

        #endregion Constructors, destructor and IDisposable overrides

        #region Collection

        /// <summary>
        /// The number if items in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return collection.Count;
            }
        }

        /// <summary>
        /// Provide access to individual items
        /// </summary>
        /// <param name="index">The index of item within the collection.</param>
        /// <returns>The DataSet at the specified index.</returns>
        /// <remarks>If the collection is backed by disk files this causes a disk file
        /// to be read.</remarks>
        public Elements this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new Exception();
                }

                Elements dicom = null;
                // if we are backed by disk
                if (info != null)
                {
                    // extract the filename from the collection and fetch the contents.
                    string name = collection[index] as string;
                    dicom = GetCachedDataSet(name);
                }
                else
                {
                    dicom = collection[index] as Elements;
                }
                return dicom;
            }
        }

        /// <summary>
        /// Adds a DataSet to the collection.
        /// </summary>
        /// <param name="dicom">The DataSet to add.</param>
        /// <remarks>If the collection is backed by disk files this causes a disk file
        /// to be read.</remarks>
        public void Add(Elements dicom)
        {
            // if we are backed by disk
            if (info != null)
            {
                // since there is no way to remove items, and each instance has its own folder
                // this filename is unique enough
                string name = String.Format("{0}.dcm", collection.Count);

                FileStream output = new FileStream(Path.Combine(info.FullName, name), FileMode.Create);
                dicom.Write(output);
                output.Close();

                collection.Add(name);
            }
            else
            {
                collection.Add(dicom);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Load()
        {
            if (info == null)
            {
                throw new Exception("The cache is not file backed.");
            }
            if (Count != 0)
            {
                throw new Exception("The cache is not empty.");
            }
            FileInfo[] files = info.GetFiles("*.dcm");
            foreach (FileInfo file in files)
            {
                Add(file.FullName);
            }
            return Count;
        }

        /// <summary>
        /// Adds a DataSet to the collection cached in a file.
        /// </summary>
        /// <param name="path">The path of an existing file containing the record.</param>
        /// <remarks>The instance must have been created to be file based, i.e. <see cref="RecordCollection(string folder, bool existing)"/></remarks>
        public void Add(string path)
        {
            if (info != null)
            {
                collection.Add(path);
            }
            else
            {
                throw new Exception("The cache is not file backed.");
            }
        }

        private Elements GetCachedDataSet(string name)
        {
            // create a stream on the file
            FileStream input = new FileStream(Path.Combine(info.FullName, name), FileMode.Open, FileAccess.Read, FileShare.Read);
            
            // and read the contents
            DataSet dicom = new DataSet();
            dicom.Read(input, 0x7000);

            input.Close();
            input.Dispose();

            return dicom.Elements;
        }

        #endregion Collection

        #region IEnumerable implementation

        IEnumerator<Elements> IEnumerable<Elements>.GetEnumerator()
        {
            foreach (object item in collection)
            {
                if (info != null)
                {
                    yield return GetCachedDataSet(item as String);
                }
                else
                {
                    yield return item as Elements;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Elements>)this).GetEnumerator();
        }

        #endregion IEnumerable implementation
    }
}
