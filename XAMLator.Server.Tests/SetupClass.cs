using System;
using NUnit.Framework;
using Xamarin.Forms;

namespace XAMLator.Server.Tests
{
	[SetUpFixture]
	public class SetupClass
	{
		[OneTimeSetUp]
		public void Setup()
		{
			Forms.Init();
		}
	}
}
