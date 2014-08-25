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
using System.Linq.Expressions;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class VBSpecificSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void VBCompareStringExpression ()
    {
      var parameterExpression = Expression.Parameter (typeof (Cook), "c");
      var vbCompareStringExpression =
          Expression.Equal (
              Expression.Call (
                  typeof (Operators).GetMethod ("CompareString"), 
                  Expression.Constant ("string1"), 
                  Expression.MakeMemberAccess (parameterExpression, typeof (Cook).GetProperty ("Name")), 
                  Expression.Constant (true)),
              Expression.Constant (0));
      var query = Cooks
          .Where (Expression.Lambda<Func<Cook, bool>> (vbCompareStringExpression, parameterExpression))
          .Select (c => c.Name);

      CheckQuery (
          query,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE (@1 = [t0].[Name])",
          new CommandParameter ("@1", "string1"));
    }

    [Test]
    public void InformationIsNothingCall ()
    {
      var parameterExpression = Expression.Parameter (typeof (Cook), "c");
      var vbCompareStringExpression =
            Expression.Call (
                typeof (Information).GetMethod ("IsNothing"),
                Expression.MakeMemberAccess (parameterExpression, typeof (Cook).GetProperty ("Name")));
      var query = Cooks
          .Where (Expression.Lambda<Func<Cook, bool>> (vbCompareStringExpression, parameterExpression))
          .Select (c => c.Name);

      CheckQuery (
          query,
          "SELECT [t0].[Name] AS [value] FROM [CookTable] AS [t0] WHERE ([t0].[Name] IS NULL)");
    }
  }
}