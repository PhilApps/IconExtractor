using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconExtractor.PhilUtils
{
	/// <summary>
	/// DirectoryInfoComparer
	/// <para>V1.1</para>
	/// </summary>
	sealed class DirectoryInfoComparer : IEqualityComparer<DirectoryInfo>, IComparer<DirectoryInfo>,
		IEqualityComparer, IComparer
	{
		internal DirectoryInfoComparer() { }

		public bool Equals(DirectoryInfo x, DirectoryInfo y)
		{
			if (null == x) return null == y;
			if (null == y) return false;

			return string.Equals(x.FullName.TrimEnd(Path.DirectorySeparatorChar),
				y.FullName.TrimEnd(Path.DirectorySeparatorChar), StringComparison.InvariantCultureIgnoreCase);
		}

		public int GetHashCode(DirectoryInfo obj)
		{
			int hcodebase = typeof(DirectoryInfo).GetHashCode();
			return (hcodebase << 5) + hcodebase ^
				StringComparer.InvariantCultureIgnoreCase
				.GetHashCode(obj.FullName.TrimEnd(Path.DirectorySeparatorChar));
		}

		public int Compare(DirectoryInfo x, DirectoryInfo y)
		{
			if (null == x) return (null != y) ? -1 : 0;
			if (null == y) return 1;

			return StringComparer.InvariantCultureIgnoreCase
				.Compare(x.FullName.TrimEnd(Path.DirectorySeparatorChar),
				y.FullName.TrimEnd(Path.DirectorySeparatorChar));
		}

		bool IEqualityComparer.Equals(object x, object y)
		{
			return this.Equals((DirectoryInfo)x, (DirectoryInfo)y);
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return this.GetHashCode((DirectoryInfo)obj);
		}

		int IComparer.Compare(object x, object y)
		{
			return this.Compare((DirectoryInfo)x, (DirectoryInfo)y);
		}
	}
}
