using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace IconExtractor.PhilUtils
{
	/// <summary>
	/// <para>Permet d'énumérer des dossiers et fichiers avec gestion d'erreur.</para>
	/// <para>V1.1</para>
	/// </summary>
	static class DirectoriesEnumerationHelper
	{
		/// <summary>
		/// Enumère les dossiers sans arrêt d'erreur si une erreur se produit.
		/// </summary>
		/// <param name="dirInfo">Dossier à regarder.</param>
		/// <param name="searchPattern">Chaîne recherchée. Le modèle par défaut est « * », qui retourne tous les répertoires.</param>
		/// <param name="searchOption"><para>L'une des valeurs d'énumération qui spécifie si l'opération de recherche
		/// doit inclure uniquement le répertoire actif ou tous les sous-répertoires.</para>
		/// <para>La valeur par défaut est System.IO.SearchOption.TopDirectoryOnly.</para>
		/// </param>
		/// <param name="actionOnError">Action à effectuer si une erreur se produit.</param>
		/// <returns>Collection énumérable de répertoires qui correspond à searchPattern et searchOption.</returns>
		public static IEnumerable<DirectoryInfo> EnumerateDirectoriesSafe(this DirectoryInfo dirInfo,
			string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly,
			Action<Exception> actionOnError = null)
		{
			if (null == dirInfo) throw new ArgumentNullException("dirInfo");
			if (null == actionOnError) actionOnError = exc => { };
			switch (searchOption)
			{
				case SearchOption.TopDirectoryOnly:
					try { return dirInfo.EnumerateDirectories(searchPattern); }
					catch (ArgumentNullException) { throw; }
					catch (DirectoryNotFoundException) { throw; }
					catch (Exception exc) { actionOnError(exc); return Enumerable.Empty<DirectoryInfo>(); }
				case SearchOption.AllDirectories:
					return dirInfo.EnumerateAllDirectoriesSafe(searchPattern, actionOnError);
				default:
					throw new NotSupportedException();
			}
		}
		private static IEnumerable<DirectoryInfo> EnumerateAllDirectoriesSafe(this DirectoryInfo dirInfo,
			string searchPattern, Action<Exception> actionOnError)
		{
			if (null == dirInfo) throw new ArgumentNullException("dirInfo");
			if (null == actionOnError) actionOnError = exc => { };

			IEnumerable<DirectoryInfo> dirInfosRet;
			try { dirInfosRet = dirInfo.EnumerateDirectories(searchPattern); }
			catch (ArgumentNullException) { throw; }
			catch (DirectoryNotFoundException) { throw; }
			catch (Exception exc) { actionOnError(exc); return Enumerable.Empty<DirectoryInfo>(); }

			return dirInfosRet.Concat(dirInfosRet
				.SelectMany(di => di.EnumerateAllDirectoriesSafe(searchPattern, actionOnError)));
		}

		/// <summary>
		/// Enumère les fichiers sans arrêt d'erreur si une erreur se produit.
		/// </summary>
		/// <param name="dirInfoRoot">Dossier à regarder.</param>
		/// <param name="searchPattern">Chaîne recherchée. Le modèle par défaut est « * », qui retourne tous les fichiers.</param>
		/// <param name="searchOption">
		/// <para>L'une des valeurs d'énumération qui spécifie si l'opération de recherche
		/// doit inclure uniquement le répertoire actif ou tous les sous-répertoires.</para>
		/// <para>La valeur par défaut est System.IO.SearchOption.TopDirectoryOnly.</para>
		/// </param>
		/// <param name="actionOnError">Action à effectuer si une erreur se produit.</param>
		/// <returns>Collection énumérable de fichiers qui correspond à searchPattern et searchOption.</returns>
		public static IEnumerable<FileInfo> EnumerateFilesSafe(this DirectoryInfo dirInfoRoot,
			string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly,
			Action<Exception> actionOnError = null)
		{
			if (null == dirInfoRoot) throw new ArgumentNullException("dirInfoRoot");
			if (null == actionOnError) actionOnError = exc => { };
			switch (searchOption)
			{
				case SearchOption.TopDirectoryOnly:
					try { return dirInfoRoot.EnumerateFiles(searchPattern); }
					catch (ArgumentNullException) { throw; }
					catch (DirectoryNotFoundException) { throw; }
					catch (Exception exc) { actionOnError(exc); return Enumerable.Empty<FileInfo>(); }
				case SearchOption.AllDirectories:
					return EnumerateAllFilesSafe(dirInfoRoot, searchPattern, actionOnError);
				default:
					throw new NotSupportedException();
			}
		}

		private static IEnumerable<FileInfo> EnumerateAllFilesSafe(this DirectoryInfo dirInfoRoot,
			string searchPattern, Action<Exception> actionOnError)
		{
			if (null == dirInfoRoot) throw new ArgumentNullException("dirInfoRoot");
			if (null == actionOnError) actionOnError = exc => { };
			
			IEnumerable<FileInfo> enumerFiles;
			try { enumerFiles = dirInfoRoot.EnumerateFiles(searchPattern); }
			catch (ArgumentNullException) { throw; }
			catch (DirectoryNotFoundException) { throw; }
			catch (Exception exc)
			{
				actionOnError(exc);
				enumerFiles = Enumerable.Empty<FileInfo>();
				return enumerFiles;
			}

			return enumerFiles.Concat(dirInfoRoot.EnumerateDirectoriesSafe("*",
				SearchOption.TopDirectoryOnly, actionOnError)
				.SelectMany(dir => dir.EnumerateAllFilesSafe(searchPattern, actionOnError)));
		}
	}
}
