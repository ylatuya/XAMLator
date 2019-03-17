using System;
using System.Collections.Generic;
using System.Linq;

namespace XAMLator.Client
{
	public class ClassDeclarationsCache
	{
		/// <summary>
		/// The xaml classes.
		/// </summary>
		internal static readonly List<ClassDeclaration> classesCache = new List<ClassDeclaration>();

		/// <summary>
		/// Reset the cache.
		/// </summary>
		public static void Reset()
		{
			classesCache.Clear();
		}

		public static void Add(ClassDeclaration classDeclaration)
		{
			classesCache.Add(classDeclaration);
		}

		/// <summary>
		/// Tries to find a cached class declaration with the same full namespace.
		/// </summary>
		/// <returns><c>true</c>, if we found matching class, <c>false</c> otherwise.</returns>
		/// <param name="fullNamespace">Full namespace.</param>
		/// <param name="viewClass">View class.</param>
		internal static bool TryGetByFullNamespace(string fullNamespace, out ClassDeclaration viewClass)
		{
			viewClass = classesCache.SingleOrDefault(x => x.FullNamespace == fullNamespace);
			return viewClass != null;
		}

		/// <summary>
		/// Tries to find a cached class declaration using this file path.
		/// </summary>
		/// <returns><c>true</c>, if we found matching class, <c>false</c> otherwise.</returns>
		/// <param name="filePath">File path.</param>
		/// <param name="viewClass">View class.</param>
		internal static bool TryGetByFileName(string filePath, out ClassDeclaration viewClass)
		{
			viewClass = classesCache.SingleOrDefault(x => x.Files.Contains(filePath));
			return viewClass != null;
		}
	}
}
