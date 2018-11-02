using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace XAMLator.Build.Tasks
{
	public class AssemblyWeaver : Task
	{
		[Required]
		public string Path
		{
			get;
			set;
		}

		public override bool Execute()
		{
			var xamlatorAssembly = System.IO.Path.GetFullPath(Path.Trim());
			var xamlatorAssemblyTmp = xamlatorAssembly + ".tmp";
			Log.LogMessage(MessageImportance.Normal, $"Weaving assembly {xamlatorAssembly}");
			AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(xamlatorAssembly);
			var resources = assemblyDef.MainModule.Resources;
			var selectedResource = resources.FirstOrDefault(x => x.Name == BuildConstants.IDE_IP_RESOURCE_NAME);
			if (selectedResource != null)
			{
				var ips = BuildNetworkUtils.DeviceIps();
				string ipsString = String.Join("-", ips);
				if (string.IsNullOrEmpty(ipsString))
				{
					ipsString = "127.0.0.1";
				}
				Log.LogMessage(MessageImportance.Normal, $"XAMLator weaved with ips {String.Join("-", ips)}");
				var currentIps = Encoding.ASCII.GetBytes(String.Join("\n", BuildNetworkUtils.DeviceIps()));
				var newResource = new EmbeddedResource(BuildConstants.IDE_IP_RESOURCE_NAME, selectedResource.Attributes, currentIps);
				resources.Remove(selectedResource);
				resources.Add(newResource);
				assemblyDef.Write(xamlatorAssemblyTmp);
			}
			else
			{
				Log.LogError($"Resource {BuildConstants.IDE_IP_RESOURCE_NAME} not found in assembly {xamlatorAssembly}");
			}
			assemblyDef.Dispose();

			File.Replace(xamlatorAssemblyTmp, xamlatorAssembly, xamlatorAssembly + ".backup");
			return true;
		}
	}
}
