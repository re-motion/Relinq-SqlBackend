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
using System.Security;
using System.Security.Permissions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.IntegrationTests.MediumTrust.Sandboxing;
using Remotion.Linq.SqlBackend.UnitTests;

namespace Remotion.Linq.SqlBackend.IntegrationTests.MediumTrust
{
  [TestFixture]
  [Ignore ("Serialization using ISafeSerializationData should actually work without the SerializationFormatter permission.")]
  public class MediumTrustUnmappedItemExceptionIntegrationTest
  {
    [Test]
    public void MediumTrust ()
    {
      var permissions = PermissionSets
          .GetMediumTrust (AppDomain.CurrentDomain.BaseDirectory, Environment.MachineName)
          .Concat (
              new IPermission[]
              {
                  new ReflectionPermission (ReflectionPermissionFlag.MemberAccess),
                  // new SecurityPermission (SecurityPermissionFlag.SerializationFormatter)
              })
          .ToArray();

      var testFixtureResults = SandboxTestRunner.RunTestFixturesInSandbox (new[] { typeof (UnmappedItemExceptionTest) }, permissions, null);
      var testResults = testFixtureResults.SelectMany (r => r.TestResults).ToArray();

      foreach (var testResult in testResults)
      {
        try
        {
          testResult.EnsureNotFailed();
        }
        catch (TestFailedException)
        {
          var securityException = testResult.Exception as SecurityException;
          if (securityException != null)
          {
            Console.WriteLine ("Action:");
            Console.WriteLine (securityException.Action);
            Console.WriteLine ("Demanded:");
            Console.WriteLine (securityException.Demanded);
          }
          throw;
        }
      }
      Assert.That (testResults.Count (r => r.Status == SandboxTestStatus.Succeeded), Is.GreaterThan (0));
    }
  }
}