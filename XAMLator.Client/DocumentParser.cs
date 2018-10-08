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
			// FIXME: Support any kind of types, not just Xamarin.Forms views
			if (!fileName.EndsWith(".xaml") && !fileName.EndsWith(".xaml.cs"))
			{
				return null;
			}

			if (!GetOrCreate(fileName, text, syntaxTree, semanticModel,
							 out FormsViewClassDeclaration xamlClass,
							 out XAMLDocument xamlDocument))
			{
				Log.Error("Could not handle document update");
				return null;
			}

			// The document is a XAML file
			if (fileName.EndsWith(".xaml"))
			{
				if (xamlDocument == null)
				{
					xamlDocument = XAMLDocument.Parse(text);
				}
				await xamlClass.UpdateXaml(xamlDocument.XAML);
			}
			// The document is code behind
			else if (fileName.EndsWith(".xaml.cs"))
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


		static bool GetOrCreate(string fileName,
								string text,
								SyntaxTree syntaxTree,
								SemanticModel semanticModel,
								out FormsViewClassDeclaration xamlClass,
								out XAMLDocument xamlDocument)
		{
			string xaml = null, xamlFilePath = null, codeBehindFilePath = null;
			xamlDocument = null;

			// Check if we have already an instance of the class declaration for that file
			if (FormsViewClassDeclaration.TryGetByFileName(fileName, out xamlClass))
			{
				return true;
			}

			if (fileName.EndsWith(".xaml"))
			{
				xaml = text;
				xamlFilePath = fileName;
				var candidate = xamlFilePath + ".cs";
				if (File.Exists(candidate))
				{
					codeBehindFilePath = candidate;
				}
			}
			else
			{
				codeBehindFilePath = fileName;
				var candidate = fileName.Substring(0, fileName.Length - 3);
				if (File.Exists(candidate))
				{
					xamlFilePath = candidate;
					xaml = File.ReadAllText(xamlFilePath);
				}
			}

			// FIXME: Handle XF views without XAML
			// Parse the XAML file 
			xamlDocument = XAMLDocument.Parse(xaml);
			if (xamlDocument == null)
			{
				Log.Error("Error parsing XAML");
				return false;
			}

			// Check if we have an instance of class by namespace
			if (FormsViewClassDeclaration.TryGetByFullNamespace(xamlDocument.Type, out xamlClass))
			{
				return true;
			}

			// This is the first time we have an update for this type

			// Create a new class declaration instance from the syntax tree
			if (syntaxTree != null)
			{
				var className = xamlDocument.Type.Split('.').Last();
				var classDeclaration = FormsViewClassDeclaration.FindClass(syntaxTree, className);
				xamlClass = new FormsViewClassDeclaration(classDeclaration, semanticModel,
													 codeBehindFilePath, xamlFilePath, xamlDocument.XAML);
			}
			// Create a new class declaration instance from the XAML
			else
			{
				xamlClass = new FormsViewClassDeclaration(codeBehindFilePath, xamlFilePath,
														 xamlDocument.XAML, xamlDocument.Type);

			}
			return true;
		}
	}
}