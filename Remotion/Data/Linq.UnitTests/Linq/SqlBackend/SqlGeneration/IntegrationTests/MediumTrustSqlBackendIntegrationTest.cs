// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Security.Permissions;
using NUnit.Framework;
using Remotion.Data.Linq.UnitTests.Sandboxing;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class MediumTrustSqlBackendIntegrationTest
  {
    [Test]
    public void MediumTrust ()
    {
      var mediumTrust = PermissionSets.GetMediumTrust (AppDomain.CurrentDomain.BaseDirectory, Environment.MachineName);
      var permissions = mediumTrust.Concat (new[] { new ReflectionPermission (ReflectionPermissionFlag.MemberAccess) }).ToArray();

      var types = (from t in typeof (MediumTrustSqlBackendIntegrationTest).Assembly.GetTypes ()
                   where t.Namespace == typeof (MediumTrustSqlBackendIntegrationTest).Namespace 
                       && t != typeof (MediumTrustSqlBackendIntegrationTest)
                       && !t.IsAbstract && !t.IsNested // TODO Review 2813: instead of !IsNested, check that TestFixtureAttribute is defined
                   select t).ToArray();

      var testFixtureResults = SandboxTestRunner.RunTestFixturesInSandbox (
          types, 
          permissions, 
          null); 
      var testResults = testFixtureResults.SelectMany (r => r.TestResults);

      foreach (var testResult in testResults)
        testResult.EnsureNotFailed ();
      // TODO Review 2813: Assert that number of succeeded tests > 0 (to ensure that we actually run anything)
    }
  }
}