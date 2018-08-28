using System;
namespace XAMLator
{
    public class XAMLDocument
    {
        public XAMLDocument(string xaml, string type)
        {
            XAML = xaml;
            Type = type;
        }

        public string XAML { get; set; }

        public string Type { get; set; }
    }
}
