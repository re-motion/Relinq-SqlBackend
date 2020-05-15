// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Linq;
using System.Security.Permissions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting.Sandboxing;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class MediumTrustSqlBackendIntegrationTest
  {
    [Test]
    public void MediumTrust () //REVIEW test fails: needs input from Michael
    {
      var permissions = PermissionSets
          .GetMediumTrust (AppDomain.CurrentDomain.BaseDirectory, Environment.MachineName)
          .Concat (new[] { new ReflectionPermission (ReflectionPermissionFlag.MemberAccess) })
          .ToArray();

      var types = (from t in typeof (MediumTrustSqlBackendIntegrationTest).Assembly.GetTypes ()
                   where t.Namespace == typeof (MediumTrustSqlBackendIntegrationTest).Namespace 
                       && t != typeof (MediumTrustSqlBackendIntegrationTest)
                       && !t.IsAbstract && t.IsDefined(typeof(TestFixtureAttribute), false)
                   select t).ToArray();

      var testFixtureResults = SandboxTestRunner.RunTestFixturesInSandbox (types, permissions, null); 
      var testResults = testFixtureResults.SelectMany (r => r.TestResults).ToArray();

      foreach (var testResult in testResults)
        testResult.EnsureNotFailed ();
      Assert.That (testResults.Count (r => r.Status == SandboxTestStatus.Succeeded), Is.GreaterThan (0));
    }
  }
}