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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.NUnit;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Resolved
{
  [TestFixture]
  public class ResolvedJoinInfoTest
  {
    [Test]
    public void Initialization_BooleanJoinCondition ()
    {
      var foreignTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo();
      var joinCondition = ExpressionHelper.CreateExpression (typeof (bool));
      
      var joinInfo = new ResolvedJoinInfo (foreignTableInfo, joinCondition);

      Assert.That (joinInfo.ForeignTableInfo, Is.SameAs (foreignTableInfo));
      Assert.That (joinInfo.JoinCondition, Is.SameAs (joinCondition));
    }

    [Test]
    public void Initialization_NullableBooleanJoinCondition ()
    {
      var foreignTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo ();
      var joinCondition = ExpressionHelper.CreateExpression (typeof (bool?));

      var joinInfo = new ResolvedJoinInfo (foreignTableInfo, joinCondition);

      Assert.That (joinInfo.ForeignTableInfo, Is.SameAs (foreignTableInfo));
      Assert.That (joinInfo.JoinCondition, Is.SameAs (joinCondition));
    }

    [Test]
    public void Initialization_NonBooleanJoinCondition ()
    {
      var foreignTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo ();
      var joinCondition = ExpressionHelper.CreateExpression (typeof (int));

      Assert.That (
          () => new ResolvedJoinInfo (foreignTableInfo, joinCondition),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
            "The join condition must have boolean (or nullable boolean) type.", "joinCondition"));
    }

    [Test]
    public void GetResolvedJoinInfo ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();

      var result = joinInfo.GetResolvedJoinInfo().ForeignTableInfo;

      Assert.That (result, Is.SameAs (joinInfo.ForeignTableInfo));
    }

    [Test]
    public new void ToString ()
    {
      var foreignTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var joinInfo = new ResolvedJoinInfo (foreignTableInfo, Expression.Equal (Expression.Constant (0), Expression.Constant (1)));
      var result = joinInfo.ToString ();

      Assert.That (result, Is.EqualTo ("[CookTable] [c] ON (0 == 1)"));
    }
  }
}