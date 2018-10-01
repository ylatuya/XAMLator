using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace XAMLator
{
	public class XAMLDocument
	{
		public XAMLDocument(string xaml, string type)
		{
			XAML = xaml;
			Type = type;
		}

		public static XAMLDocument Parse(string xaml)
		{
			try
			{
				using (var stream = new StringReader(xaml))
				{
					var reader = XmlReader.Create(stream);
					var xdoc = XDocument.Load(reader);
					XNamespace x = "http://schemas.microsoft.com/winfx/2009/xaml";
					var classAttribute = xdoc.Root.Attribute(x + "Class");
					CleanAutomationIds(xdoc.Root);
					xaml = xdoc.ToString();
					return new XAMLDocument(xaml, classAttribute.Value);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return null;
			}
		}

		static void CleanAutomationIds(XElement xdoc)
		{
			xdoc.SetAttributeValue("AutomationId", null);
			foreach (var el in xdoc.Elements())
			{
				CleanAutomationIds(el);
			}
		}

		public string XAML { get; set; }

		public string Type { get; set; }
	}
}
