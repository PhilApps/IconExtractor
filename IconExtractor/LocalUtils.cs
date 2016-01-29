using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IconExtractor.CommonExceptions;

namespace IconExtractor
{
    static class LocalUtils
    {
        internal static IDisposable SetInitializing(this ISupportInitialize source) => new InitializingHolder(source);

        private sealed class InitializingHolder : IDisposable
        {
            readonly ISupportInitialize _source;
            internal InitializingHolder(ISupportInitialize source)
            {
                ThrowArgumentNullIfObjectNull(source, nameof(source));
                _source = source;
                _source.BeginInit();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private bool _disposed;
            private void Dispose(bool disposing)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(InitializingHolder));
                _source.EndInit();
                _disposed = true;
            }

            ~InitializingHolder() { Dispose(false); }
        }
    }
}
