using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using Microsoft.Build.Framework;

namespace XAMLator.Build.Tasks.Tests
{
	[TestFixture]
	public class AssemblyWeaverScenario
	{
		[Test]
		public void The_xamlator_assembly_is_weaved_with_the_current_ip()
		{
			var task = new AssemblyWeaver();
			task.Path = BuildConstants.XAMLATOR_ASSEMBLY;
			task.BuildEngine = Mock.Of<IBuildEngine>();
			task.Execute();

			var assembly = Assembly.LoadFrom(BuildConstants.XAMLATOR_ASSEMBLY);
			using (Stream stream = assembly.GetManifestResourceStream(BuildConstants.IDE_IP_RESOURCE_NAME))
			using (StreamReader reader = new StreamReader(stream))
			{
				var ip = reader.ReadLine();
				Assert.AreEqual(BuildNetworkUtils.DeviceIps().First(), ip);
			}
		}
	}
}
