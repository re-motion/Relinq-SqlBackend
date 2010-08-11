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
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.Utilities
{
  /// <summary>
  /// checkes the queryResult against a previously created resource
  /// </summary>
  public class CheckingTestExecutor : ITestExecutor
  {
    public void Execute (object queryResult, MethodBase executingMethod)
    {
      ArgumentUtility.CheckNotNull ("executingMethod", executingMethod);
      var referenceResult = GetReferenceResult (executingMethod);
      var actualResult = GetActualResult (queryResult);

      var result = TestResultChecker.Check (referenceResult, actualResult);
      // TODO: Assert??
      Assert.That (result.IsEqual, Is.EqualTo (true));
    }

    private string GetActualResult (object queryResult)
    {
      var stringWriter = new StringWriter();
      var serializer = new TestResultSerializer (stringWriter, info => !info.IsDefined (typeof (System.Data.Linq.Mapping.AssociationAttribute), false));
      serializer.Serialize (queryResult);
      return stringWriter.ToString();
    }

    private string GetReferenceResult (MethodBase executingMethod)
    {
      var resourceName = executingMethod.DeclaringType.Namespace + ".Resources." + executingMethod.DeclaringType.Name 
        + "." + executingMethod.Name + ".result";
      using (var resourceStream = executingMethod.DeclaringType.Assembly.GetManifestResourceStream (resourceName))
      {
        using (var streamReader = new StreamReader (resourceStream))
        {
          return streamReader.ReadToEnd();
        }
      }
    }
  }
}