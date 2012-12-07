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
using NUnit.Framework;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests.MethodCalls
{
  [TestFixture]
  public class ConversionMethodCallExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public new void ToString ()
    {
      CheckQuery (
          from c in Cooks select c.ID.ToString(),
          "SELECT CONVERT(NVARCHAR(MAX), [t0].[ID]) AS [value] FROM [CookTable] AS [t0]"
          );
    }

    [Test]
    public void Convert ()
    {
      CheckQuery (
          from c in Cooks select System.Convert.ToInt32 (c.FirstName),
          "SELECT CONVERT(INT, [t0].[FirstName]) AS [value] FROM [CookTable] AS [t0]"
          );

      CheckQuery (
          from c in Cooks select System.Convert.ToString (c.ID),
          "SELECT CONVERT(NVARCHAR(MAX), [t0].[ID]) AS [value] FROM [CookTable] AS [t0]"
          );
    }
  }
}