using System;
using Microsoft.SPOT;
using System.Collections;
using System.Reflection;
using Microsoft.SPOT.IO;

namespace BlueCone.Utils
{
    /// <summary>
    /// Implementation of the System.IO.Directory class
    /// </summary>
    public class DirectoryEx
    {
        /// <summary>
        /// This method return IEnumerable for all files in the given path
        /// </summary>
        /// <param name="path">directory path</param>
        /// <returns>IEnumerable</returns>
        public static IEnumerable GetFiles(string path)
        {
            FilesCollection collection = new FilesCollection(path);

            return collection;
        }

        /// <summary>
        /// This method returns the total number of files in the given path.
        /// </summary>
        /// <param name="path">directory path</param>
        /// <returns>Total number of files.</returns>
        public static int GetTotalFiles(string path)
        {
            FilesCollection collection = new FilesCollection(path);

            return collection.TotalFiles;
        }
    }

    /// <summary>
    /// IEnumerable for files in the directory
    /// </summary>
    public class FilesCollection : IEnumerable
    {
        FileFinder filesFinder = null;
        int totalFiles;

        public int TotalFiles
        {
            get
            {
                foreach (string file in this)
                {
                    totalFiles++;
                }
                return totalFiles;
            }
        }

        public FilesCollection(string path)
        {
            filesFinder = new FileFinder(path);
            totalFiles = 0;
        }

        public IEnumerator GetEnumerator()
        {
            return filesFinder;
        }
    }

    /// <summary>
    /// IEnumerator wrapper around internal NativeFileFinder and NativeFileInfo classes
    /// </summary>
    public class FileFinder : IEnumerator
    {
        Type _nativeFileFinderType = null;
        ConstructorInfo _nativeFileFinderConstructor = null;
        MethodInfo _getNextMethod = null;
        MethodInfo _closeMethod = null;
        object _nativeFileFinderInstance = null;


        Type _nativeFileInfoType = null;
        object _nativeFileInfoInstance = null;
        FieldInfo _nativeFileInfoFileNameField = null;
        FieldInfo _nativeFileInfoAttributesField = null;

        string _directoryPath;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        public FileFinder(string path)
        {
            _directoryPath = path;

            //Use reflection to get all "Info" objects

            Assembly assembly = Assembly.GetAssembly(typeof(MediaEventArgs));

            Type[] types = assembly.GetTypes();

            foreach (Type t in types)
            {
                if (t.FullName == "Microsoft.SPOT.IO.NativeFindFile")
                {
                    _nativeFileFinderType = t;
                }
                else if (t.FullName == "Microsoft.SPOT.IO.NativeFileInfo")
                {
                    _nativeFileInfoType = t;
                }
            }

            if (_nativeFileInfoType != null)
            {
                _nativeFileInfoFileNameField = _nativeFileInfoType.GetField("FileName");
                _nativeFileInfoAttributesField = _nativeFileInfoType.GetField("Attributes");
            }

            if (_nativeFileFinderType != null)
            {
                _nativeFileFinderConstructor = _nativeFileFinderType.GetConstructor(new Type[] { typeof(string), typeof(string) });

                _getNextMethod = _nativeFileFinderType.GetMethod("GetNext");
                _closeMethod = _nativeFileFinderType.GetMethod("Close");
            }

            if (_nativeFileFinderConstructor != null)
                _nativeFileFinderInstance = _nativeFileFinderConstructor.Invoke(new object[] { path, "*" });

        }

        #region IEnumerator implementation

        /// <summary>
        /// IEnumerator Current property
        /// </summary>
        public object Current
        {
            get
            {
                if (_nativeFileInfoInstance != null)
                {
                    return _nativeFileInfoFileNameField.GetValue(_nativeFileInfoInstance);

                }
                return null;
            }
        }

        /// <summary>
        /// IEnumerator MoveNext method
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (_getNextMethod != null)
            {
                for (_nativeFileInfoInstance = _getNextMethod.Invoke(_nativeFileFinderInstance, new object[] { }); _nativeFileInfoInstance != null; )
                {
                    uint attributes = (uint)_nativeFileInfoAttributesField.GetValue(_nativeFileInfoInstance);
                    if ((attributes & 0x10) == 0)
                    {
                        return true;
                    }
                    _nativeFileInfoInstance = _getNextMethod.Invoke(_nativeFileFinderInstance, new object[] { });
                }

                _closeMethod.Invoke(_nativeFileFinderInstance, new object[] { });

                return false;
            }
            return false;
        }

        /// <summary>
        /// IEnumerator Reset method
        /// </summary>
        public void Reset()
        {
            if (_nativeFileFinderConstructor != null)
            {
                if (_nativeFileFinderInstance != null)
                    _closeMethod.Invoke(_nativeFileFinderInstance, new object[] { });

                _nativeFileFinderInstance = _nativeFileFinderConstructor.Invoke(new object[] { _directoryPath, "*" });
            }
        }

        #endregion
    }
}
