using Microsoft.CodeAnalysis;

namespace XAMLator.Client
{
	/// <summary>
	/// Document changed event arguments.
	/// </summary>
	public class DocumentChangedEventArgs
	{
		public DocumentChangedEventArgs(string filename, string text,
										SyntaxTree syntaxTree, SemanticModel semanticModel)
		{
			Filename = filename;
			Text = text;
			SyntaxTree = syntaxTree;
			SemanticModel = semanticModel;
		}

		/// <summary>
		/// Gets the filename changed.
		/// </summary>
		/// <value>The filename.</value>
		public string Filename { get; }

		/// <summary>
		/// Gets the text contents of the file changed.
		/// </summary>
		/// <value>The text.</value>
		public string Text { get; }

		/// <summary>
		/// Gets the syntax tree.
		/// </summary>
		/// <value>The syntax tree.</value>
		public SyntaxTree SyntaxTree { get; }

		/// <summary>
		/// Gets the semantic model.
		/// </summary>
		/// <value>The semantic model.</value>
		public SemanticModel SemanticModel { get; }
	}
}