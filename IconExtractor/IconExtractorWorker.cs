using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using IconExtractor.PhilUtils;
using static IconExtractor.CommonExceptions;
using static IconExtractor.PhilUtils.Utils;

namespace IconExtractor
{
    sealed class IconExtractorWorker
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int ExtractIconEx(string fileName, int iconStartingIndex, IntPtr[] largeIcons, IntPtr[] smallIcons, int iconCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int DestroyIcon(IntPtr icon);

        readonly IDictionary<FileInfo, IReadOnlyCollection<BitmapSource>> _extractedIcons;
        public IReadOnlyDictionary<FileInfo, IReadOnlyCollection<BitmapSource>> ExtractedIcons { get; }

        static readonly Lazy<FileInfoComparer> _lazyFileInfoComparer =
            new Lazy<FileInfoComparer>(() => new FileInfoComparer());

        private sealed class DispatcherHolder : DispatcherObject
        {
            readonly IconExtractorWorker _parent;
            internal DispatcherHolder(IconExtractorWorker parent) { _parent = parent; }
        }

        readonly DispatcherHolder _dispatcherHolder;

        internal IconExtractorWorker()
        {
            _dispatcherHolder = new DispatcherHolder(this);
            _extractedIcons = new SortedList<FileInfo, IReadOnlyCollection<BitmapSource>>(_lazyFileInfoComparer.Value);
            ExtractedIcons = new ReadOnlyDictionary<FileInfo, IReadOnlyCollection<BitmapSource>>(_extractedIcons);
        }

        public void ExtractIconsFromFile(FileInfo file, IconSize size)
        {
            ThrowArgumentNullIfObjectNull(file, nameof(file));
            ThrowFileNotFoundIfNotFound(file);

            IntPtr[] nullPtr = null;
            int count = ExtractIconEx(file.FullName, -1, nullPtr, nullPtr, 1);

            IntPtr[] largeIcons = size == IconSize.Large ? new IntPtr[count] : null;
            IntPtr[] smallIcons = size == IconSize.Small ? new IntPtr[count] : null;

            //TODO To finish...
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
