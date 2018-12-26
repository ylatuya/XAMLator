using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace XAMLator.Client
{
	public static class DocumentParser
	{
		/// <summary>
		/// Parses document updates from the IDE and return the class associated
		/// with the changes.
		/// </summary>
		/// <returns>The class declaration that changed.</returns>
		/// <param name="fileName">File name changed.</param>
		/// <param name="text">Text changed.</param>
		/// <param name="syntaxTree">Syntax tree.</param>
		/// <param name="semanticModel">Semantic model.</param>
		public static async Task<FormsViewClassDeclaration> ParseDocument(string fileName,
																		  string text,
																		  SyntaxTree syntaxTree,
																		  SemanticModel semanticModel)
		{
			XAMLDocument xamlDocument = null;
			FormsViewClassDeclaration xamlClass = null;
			bool created;

			// FIXME: Support any kind of types, not just Xamarin.Forms views
			if (!fileName.EndsWith(".xaml") && !fileName.EndsWith(".xaml.cs"))
			{
				return null;
			}

			// Check if we have already an instance of the class declaration for that file
			if (!FormsViewClassDeclaration.TryGetByFileName(fileName, out xamlClass))
			{
				if (fileName.EndsWith(".xaml"))
				{
					xamlDocument = XAMLDocument.Parse(fileName, text);
					// Check if we have an instance of class by namespace
					if (!FormsViewClassDeclaration.TryGetByFullNamespace(xamlDocument.Type, out xamlClass))
					{
						xamlClass = await CreateFromXaml(xamlDocument);
					}
				}
				else
				{
					xamlClass = await CreateFromCodeBehind(fileName, syntaxTree, semanticModel);
				}
			}

			if (xamlClass == null)
			{
				return null;
			}

			// The document is a XAML file
			if (fileName.EndsWith(".xaml") && xamlDocument == null)
			{
				xamlDocument = XAMLDocument.Parse(fileName, text);
				await xamlClass.UpdateXaml(xamlDocument);
			}
			// The document is code behind
			if (fileName.EndsWith(".xaml.cs"))
			{
				var classDeclaration = FormsViewClassDeclaration.FindClass(syntaxTree, xamlClass.ClassName);
				if (xamlClass.NeedsClassInitialization)
				{
					xamlClass.FillClassInfo(classDeclaration, semanticModel);
				}
				xamlClass.UpdateCode(classDeclaration);
			}
			return xamlClass;
		}

		static async Task<FormsViewClassDeclaration> CreateFromXaml(XAMLDocument xamlDocument)

		{
			string codeBehindFilePath = xamlDocument.FilePath + ".cs";
			if (!File.Exists(codeBehindFilePath))
			{
				Log.Error("XAML file without code behind");
				return null;
			}
			var xamlClass = new FormsViewClassDeclaration(codeBehindFilePath, xamlDocument);
			await xamlClass.UpdateXaml(xamlDocument);
			return xamlClass;
		}

		static async Task<FormsViewClassDeclaration> CreateFromCodeBehind(string fileName,
			SyntaxTree syntaxTree, SemanticModel semanticModel)

		{
			string xaml = null, xamlFilePath = null, codeBehindFilePath = null;
			XAMLDocument xamlDocument;

			codeBehindFilePath = fileName;
			var xamlCandidate = fileName.Substring(0, fileName.Length - 3);
			if (File.Exists(xamlCandidate))
			{
				xamlFilePath = xamlCandidate;
				xaml = File.ReadAllText(xamlFilePath);
			}

			// FIXME: Handle XF views without XAML
			// Parse the XAML file 
			xamlDocument = XAMLDocument.Parse(xamlFilePath, xaml);

			var className = xamlDocument.Type.Split('.').Last();
			var classDeclaration = FormsViewClassDeclaration.FindClass(syntaxTree, className);
			var xamlClass = new FormsViewClassDeclaration(classDeclaration, semanticModel,
													codeBehindFilePath, xamlDocument);

			await xamlClass.UpdateXaml(xamlDocument);
			return xamlClass;
		}
	}
}