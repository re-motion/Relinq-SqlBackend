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
using System.Linq.Expressions;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
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