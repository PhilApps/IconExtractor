using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilUtils
{
	/// <summary>
	/// FileInfoComparer
	/// <para>V1.1</para>
	/// </summary>
	sealed class FileInfoComparer : IEqualityComparer<FileInfo>, IComparer<FileInfo>,
		IEqualityComparer, IComparer
	{
		internal FileInfoComparer() { }

		public bool Equals(FileInfo x, FileInfo y)
		{
			if (null == x) return null == y;
			if (null == y) return false;

			return string.Equals(x.FullName, y.FullName, StringComparison.InvariantCultureIgnoreCase);
		}

		public int GetHashCode(FileInfo obj)
		{
			int hcodebase = typeof(FileInfo).GetHashCode();
			return (hcodebase << 5) + hcodebase ^
				StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.FullName);
		}

		public int Compare(FileInfo x, FileInfo y)
		{
			if (null == x) return (null != y) ? -1 : 0;
			if (null == y) return 1;

			return StringComparer.InvariantCultureIgnoreCase.Compare(x.FullName, y.FullName);
		}

		bool IEqualityComparer.Equals(object x, object y)
		{
			return this.Equals((FileInfo)x, (FileInfo)y);
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return this.GetHashCode((FileInfo)obj);
		}

		int IComparer.Compare(object x, object y)
		{
			return this.Compare((FileInfo)x, (FileInfo)y);
		}
	}
}
