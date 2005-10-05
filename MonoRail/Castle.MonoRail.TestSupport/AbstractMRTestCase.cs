// Copyright 2004-2005 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MonoRail.TestSupport
{
	using System;
	using System.IO;
	using System.Text;
	using System.Web.Hosting;
	using System.Configuration;
	
	using NUnit.Framework;


	public abstract class AbstractMRTestCase
	{
		private static readonly String PhysicalWebDirConfigKey = "web.physical.dir";
		private static readonly String VirtualWebDirConfigKey = "web.virtual.dir";

		private WebAppHost host;
		private TestRequest request;
		private TestResponse response;
		private StringBuilder outputBuffer = new StringBuilder();

		[TestFixtureSetUp]
		public virtual void FixtureInitialize()
		{
			String virDir = GetVirtualDir();
			String physicalDir = GetPhysicalDir();

			host = (WebAppHost) ApplicationHost.CreateApplicationHost( 
				typeof(WebAppHost), virDir, physicalDir );

			host.Configure(virDir, physicalDir);
		}

		[SetUp]
		public virtual void Initialize()
		{
			request = new TestRequest();
		}

		[TearDown]
		public virtual void Terminate()
		{
			outputBuffer.Length = 0;
		}

		[TestFixtureTearDown]
		public virtual void FixtureTerminate()
		{
			if (host != null) host.Dispose();
		}

		public void DoGet(String path, params String[] queryStringParams)
		{
			if (queryStringParams.Length != 0) Request.QueryStringParams = queryStringParams;

			Request.Url = path;

			StringWriter writer = new StringWriter(outputBuffer);

			response = host.Process( Request, writer );

			// Console.WriteLine( "Contents " + writer.GetStringBuilder().ToString() );
		}

		public TestRequest Request
		{
			get { return request; }
		}

		public TestResponse Response
		{
			get { return response; }
		}

		protected void AssertSuccess()
		{
			Assert.IsNotNull(response, "No requests performed with DoGet or DoPost (?)");
			Assert.IsTrue(response.StatusCode < 400, "Status code different than > 400");
		}

		protected void AssertReplyEqualsTo(String expectedContents)
		{
			Assert.AreEqual( expectedContents, outputBuffer.ToString() );
		}

		protected virtual string GetPhysicalDir()
		{
			String dir = ConfigurationSettings.AppSettings[PhysicalWebDirConfigKey];

			if (dir == null)
			{
				String message = String.Format("Could not find a configuration key " + 
					"defining the web application physical directory. You must create " + 
					"a key ('{0}') on your configuration file or override the method " + 
					"AbstractMRTestCase.GetPhysicalDir", PhysicalWebDirConfigKey);

				throw new ConfigurationException(message);
			}

			if (!Path.IsPathRooted(dir))
			{
				DirectoryInfo dinfo = new DirectoryInfo( Path.Combine( AppDomain.CurrentDomain.SetupInformation.ApplicationBase, dir ) );

				dir = dinfo.FullName;
			}

			return dir;
		}

		protected virtual string GetVirtualDir()
		{
			String dir = ConfigurationSettings.AppSettings[VirtualWebDirConfigKey];

			if (dir == null)
			{
				dir = "/";
			}

			return dir;
		}
	}
}
