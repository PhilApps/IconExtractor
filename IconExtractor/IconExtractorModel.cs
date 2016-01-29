using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PhilUtils;
using static IconExtractor.CommonExceptions;
using static PhilUtils.Utils;

namespace IconExtractor
{
    sealed class IconExtractorModel : INotifyPropertyChanged
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int ExtractIconEx(string fileName, int iconStartingIndex, IntPtr[] largeIcons, IntPtr[] smallIcons, int iconCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int DestroyIcon(IntPtr icon);

        static readonly Lazy<FileInfoComparer> _lazyFileInfoComparer =
            new Lazy<FileInfoComparer>(() => new FileInfoComparer());
        static readonly Lazy<DirectoryInfoComparer> _lazyDirectoryInfoComparer =
            new Lazy<DirectoryInfoComparer>(() => new DirectoryInfoComparer());

        private sealed class DispatcherHolder : DispatcherObject
        {
            readonly IconExtractorModel _parent;
            internal DispatcherHolder(IconExtractorModel parent) { _parent = parent; }
        }

        readonly DispatcherHolder _dispatcherHolder;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler) handler(this, new PropertyChangedEventArgs(propName));
        }

        private string _folder;
        public string Folder
        {
            get { return _folder; }
            set
            {
                DirectoryInfo oldDir = string.IsNullOrWhiteSpace(_folder) ? null : new DirectoryInfo(_folder);
                DirectoryInfo newDir = string.IsNullOrWhiteSpace(value) ? null : new DirectoryInfo(value);

                if (!_lazyDirectoryInfoComparer.Value.Equals(newDir, oldDir))
                {
                    _folder = value;
                    RaisePropertyChanged(nameof(Folder));
                }
            }
        }

        private bool _searchSubfolders;
        public bool SearchSubfolders
        {
            get { return _searchSubfolders; }
            set
            {
                if (_searchSubfolders != value)
                {
                    _searchSubfolders = value;
                    RaisePropertyChanged(nameof(SearchSubfolders));
                }
            }
        }

        internal IconExtractorModel()
        {
            _dispatcherHolder = new DispatcherHolder(this);
        }

        private IntPtr[] ExtractIconsFromFile(FileInfo file, IconSize size)
        {
            ThrowArgumentNullIfObjectNull(file, nameof(file));
            ThrowFileNotFoundIfNotFound(file);

            IntPtr[] nullPtr = null;
            int count = ExtractIconEx(file.FullName, -1, nullPtr, nullPtr, 1);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            IntPtr[] largeIcons = size == IconSize.Large ? new IntPtr[count] : null;
            IntPtr[] smallIcons = size == IconSize.Small ? new IntPtr[count] : null;
            IntPtr[] arrayNotNull = (largeIcons != null) ? largeIcons : smallIcons;

            if (count <= 0) return arrayNotNull;

            ExtractIconEx(file.FullName, 0, largeIcons, smallIcons, count);
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            return arrayNotNull;
        }

        public Task<IReadOnlyDictionary<FileInfo, IReadOnlyCollection<BitmapSource>>> GetIconsAsync(CancellationToken canceltoken, IProgress<double> progress)
        {
            DirectoryInfo parentDir = new DirectoryInfo(Folder);
            SearchOption searchOption = SearchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return Task.Run(() => GetIcons(canceltoken, progress, parentDir, searchOption), canceltoken);
        }

        private IReadOnlyDictionary<FileInfo, IReadOnlyCollection<BitmapSource>> GetIcons(CancellationToken canceltoken, IProgress<double> progress, DirectoryInfo parentDir, SearchOption searchOption)
        {
            IDictionary<FileInfo, IReadOnlyCollection<BitmapSource>> valRet =
                new SortedList<FileInfo, IReadOnlyCollection<BitmapSource>>(_lazyFileInfoComparer.Value);
            progress?.Report(0.0);
            FileInfo[] files = parentDir.EnumerateFilesSafe(searchOption: searchOption).ToArray();

            double current = 0.0;
            foreach (FileInfo fi in files)
            {
                IntPtr[] handles = null;
                try
                {
                    handles = ExtractIconsFromFile(fi, IconSize.Large);
                }
                catch (Exception exc)
                {
                    Trace.TraceError($"In {fi.FullName} : {exc.Message}");
                }

                if (null != handles)
                {
                    try
                    {
                        IReadOnlyCollection<BitmapSource> bitmaps = Array.AsReadOnly(handles
                            .Where(h => h != IntPtr.Zero)
                            .Select(h => _dispatcherHolder.Dispatcher.InvokeAsync(() => Imaging.CreateBitmapSourceFromHIcon(h, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())).Result)
                            .ToArray());
                        valRet.Add(fi, bitmaps);
                    }
                    finally
                    {
                        foreach (IntPtr h in handles) DestroyIcon(h);
                    }
                }

                current = current + 1.0;
                progress?.Report(current / files.Length);
                canceltoken.ThrowIfCancellationRequested();
            }

            progress?.Report(1.0);

            return new ReadOnlyDictionary<FileInfo, IReadOnlyCollection<BitmapSource>>(valRet);
        }
    }

    /// <summary>
	/// Determines the size of extracted icons.
	/// </summary>
	public enum IconSize
    {
        /// <summary>
        /// 32 x 32
        /// </summary>
        Large,
        /// <summary>
        /// 16 x 16
        /// </summary>
        Small
    }
}
