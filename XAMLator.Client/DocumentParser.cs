using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
		public static async Task<ClassDeclaration> ParseDocument(string fileName,
																 string text,
																 SyntaxTree syntaxTree,
																 SemanticModel semanticModel)
		{
			XAMLDocument xamlDocument = null;
			ClassDeclaration classDeclaration = null;

			if (!fileName.EndsWith(".xaml") && !fileName.EndsWith(".cs"))
			{
				return null;
			}

			// Check if we have already an instance of the class declaration for that file
			if (!ClassDeclarationsCache.TryGetByFileName(fileName, out classDeclaration))
			{
				if (fileName.EndsWith(".xaml"))
				{
					xamlDocument = XAMLDocument.Parse(fileName, text);
					// Check if we have an instance of class by namespace
					if (!ClassDeclarationsCache.TryGetByFullNamespace(xamlDocument.Type, out classDeclaration))
					{
						classDeclaration = await CreateFromXaml(xamlDocument);
					}
				}
				else
				{
					classDeclaration = await CreateFromCodeBehind(fileName, syntaxTree, semanticModel);
				}
			}

			if (classDeclaration == null)
			{
				return null;
			}

			// The document is a XAML file
			if (fileName.EndsWith(".xaml") && xamlDocument == null)
			{
				xamlDocument = XAMLDocument.Parse(fileName, text);
				await (classDeclaration as FormsViewClassDeclaration).UpdateXaml(xamlDocument);
			}
			// The document is code behind or a view without XAML
			if (fileName.EndsWith(".cs"))
			{
				var klass = ClassDeclaration.FindClass(syntaxTree, classDeclaration.ClassName);
				if (classDeclaration.NeedsClassInitialization)
				{
					classDeclaration.FillClassInfo(klass, semanticModel);
				}
				classDeclaration.UpdateCode(klass, semanticModel);
			}
			return classDeclaration;
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
			string xaml = null;
			string xamlFilePath = null;
			string codeBehindFilePath = null;
			string className = null;
			ClassDeclarationSyntax classDeclaration;
			XAMLDocument xamlDocument = null;

			codeBehindFilePath = fileName;
			var xamlCandidate = fileName.Substring(0, fileName.Length - 3);
			if (File.Exists(xamlCandidate))
			{
				xamlFilePath = xamlCandidate;
				xaml = File.ReadAllText(xamlFilePath);
			}

			// FIXME: Handle XF views without XAML
			if (xamlFilePath != null)
			{
				// Parse the XAML file 
				xamlDocument = XAMLDocument.Parse(xamlFilePath, xaml);
				className = xamlDocument.Type.Split('.').Last();
				classDeclaration = FormsViewClassDeclaration.FindClass(syntaxTree, className);
			}
			else
			{
				try
				{
					classDeclaration = FormsViewClassDeclaration.FindFormsViewClass(syntaxTree, semanticModel);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					Log.Error("The class is not a Xamarin.Forms View or Page");
					return null;
				}
			}
			var formsClass = new FormsViewClassDeclaration(classDeclaration, semanticModel,
														   codeBehindFilePath, xamlDocument);

			if (xamlDocument != null)
			{
				await formsClass.UpdateXaml(xamlDocument);
			}
			return formsClass;
		}
	}
}