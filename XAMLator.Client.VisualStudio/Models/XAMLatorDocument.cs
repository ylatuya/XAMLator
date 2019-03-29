using Microsoft.CodeAnalysis;

namespace XAMLator.Client.VisualStudio.Models
{
    public class XAMLatorDocument
    {
        public string Path { get; set; }

        public string Code { get; set; }

        public SyntaxTree SyntaxTree { get; set; }

        public SemanticModel SemanticModel { get; set; }
    }
}