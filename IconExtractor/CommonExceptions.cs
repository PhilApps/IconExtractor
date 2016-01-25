using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconExtractor
{
    static class CommonExceptions
    {
        internal static void ThrowArgumentNullIfObjectNull<T>(T obj, string paramName) where T : class
        {
            if (null == obj) throw new ArgumentNullException(paramName);
        }
        internal static void ThrowFileNotFound(string fileName)
        {
            ThrowArgumentNullIfObjectNull(fileName, nameof(fileName));
            throw new FileNotFoundException(string.Format(ResErrors.ErrFileNotFound, fileName), fileName);
        }

        internal static void ThrowFileNotFoundIfNotFound(FileInfo file)
        {
            ThrowArgumentNullIfObjectNull(file, nameof(file));
            if (!file.Exists) ThrowFileNotFound(file.FullName);
        }
    }
}
