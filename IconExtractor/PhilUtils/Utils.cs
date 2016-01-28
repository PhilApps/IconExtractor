using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhilUtils
{
    /// <summary>
    /// Divers utilitaires V1.1
    /// </summary>
    static class Utils
    {
        internal static void ReverseParams<T>(ref T p1, ref T p2)
        {
            T tmp = p1;
            p1 = p2;
            p2 = tmp;
        }

        internal static string GetExecPublicKeyTokenString()
        {
            return new StrongNamePublicKeyBlob(Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken()).ToString();
        }

        internal static int CombineHashCodes(int h1, int h2) => (h1 << 5) + h1 ^ h2;

        internal static int CombineHashCodes(int h1, int h2, int h3) => CombineHashCodes(CombineHashCodes(h1, h2), h3);

        internal static T[] ArrayVals<T>(params T[] elts) => elts;
    }
}
