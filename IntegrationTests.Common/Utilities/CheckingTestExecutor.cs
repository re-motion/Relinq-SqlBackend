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
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Remotion.Utilities;

namespace Remotion.Linq.IntegrationTests.Common.Utilities
{
  /// <summary>
  /// Checks the queryResult against a previously created resource
  /// </summary>
  public class CheckingTestExecutor : ITestExecutor
  {
    private readonly Func<MethodBase, string> _resourceNameGenerator;

    public CheckingTestExecutor (Func<MethodBase, string> resourceNameGenerator)
    {
      ArgumentUtility.CheckNotNull (nameof(resourceNameGenerator), resourceNameGenerator);
      _resourceNameGenerator = resourceNameGenerator;
    }

    public void Execute (object queryResult, MethodBase executingMethod)
    {
      ArgumentUtility.CheckNotNull (nameof(executingMethod), executingMethod);

      var referenceResult = GetReferenceResult (executingMethod);
      var actualResult = GetActualResult (queryResult);

      var result = new ComparisonResult (referenceResult, actualResult);
      Assert.That (result.IsEqual, Is.True, "The actual results differ from the expected results." + Environment.NewLine + result.GetDiffSet());
    }

    private static string GetActualResult (object queryResult)
    {
      var stringWriter = new StringWriter();
      // Ignore bidirectional associations - we are only interested in the foreign key properties of associations (eg., CategoryID rather than Category)
      var serializer = new TestResultSerializer (stringWriter, info => !info.IsDefined (typeof (System.Data.Linq.Mapping.AssociationAttribute), false));
      serializer.Serialize (queryResult);
      return stringWriter.ToString();
    }

    private string GetReferenceResult (MethodBase executingMethod)
    {
      var resultFileName = Path.Combine (TestContext.CurrentContext.TestDirectory, _resourceNameGenerator (executingMethod));

      try
      {
        return File.ReadAllText (resultFileName);
      }
      catch (Exception)
      {
        var message = string.Format (
              "No reference result exists for method: '{0}.{1}': File '{2}' could not be found.",
              executingMethod.DeclaringType,
              executingMethod.Name,
              resultFileName);
        throw new FileNotFoundException (message);
      }
    }
  }
}
