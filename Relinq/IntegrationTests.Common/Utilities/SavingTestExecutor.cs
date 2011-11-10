// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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

namespace Remotion.Linq.IntegrationTests.Common.Utilities
{
  /// <summary>
  /// Saves <see cref="TestResultSerializer"/> output to a file with correct naming and extension into a specified directory -
  /// as a resource for later use with <see cref="CheckingTestExecutor"/>.
  /// </summary>
  public class SavingTestExecutor : ITestExecutor
  {
    private readonly string _directory;
    private readonly Func<MethodBase, string> _resourceFileNameGenerator;

    public SavingTestExecutor (string directory, Func<MethodBase, string> resourceFileNameGenerator)
    {
      _directory = directory;
      _resourceFileNameGenerator = resourceFileNameGenerator;
    }

    public void Execute (object queryResult, MethodBase executingMethod)
    {
      var resourceName = _resourceFileNameGenerator (executingMethod);
      if (!Directory.Exists (_directory))
        Directory.CreateDirectory (_directory);

      var fileName = Path.Combine (_directory, resourceName);
      using (var writer = File.CreateText(fileName))
      {
        var serializer = new TestResultSerializer (writer, info => !info.IsDefined (typeof (System.Data.Linq.Mapping.AssociationAttribute), false));
        serializer.Serialize (queryResult);
      }
    }
  }
}