using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhilUtils
{
	/// <summary>
	/// <para>Permet d'obtenir des objets <see cref="IDisposable"/> pour les objets <see cref="SpinLock"/>.</para>
	/// <para>V1.0</para>
	/// </summary>
	static class SpinLockHelper
	{
		private sealed class SpinLockGet : IDisposable
		{
			readonly SpinLock _spinLock;
			internal SpinLockGet(SpinLock spinLock)
			{
				bool gotLock = false;
				_spinLock = spinLock;
				_spinLock.Enter(ref gotLock);
				Debug.Assert(gotLock, "Le lock aurait dû être obtenu normalement...");
			}

			private void Dispose(bool disposing)
			{
				_spinLock.Exit();
			}

			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}
			~SpinLockGet() { this.Dispose(false); }
		}
		/// <summary>
		/// Obtient un objet disposable pour le lock obtenu.
		/// </summary>
		/// <param name="spinlock">Un objet <see cref="SpinLock"/>.</param>
		/// <returns>Un objet à disposer à la fin (utiliser un using).</returns>
		internal static IDisposable GetLock(this SpinLock spinlock)
		{
			return new SpinLockGet(spinlock);
		}
	}
}
