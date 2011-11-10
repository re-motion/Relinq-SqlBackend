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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class ResolvedJoinInfoTest
  {
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
      var joinInfo = new ResolvedJoinInfo (foreignTableInfo, Expression.Constant (0), Expression.Constant (1));
      var result = joinInfo.ToString ();

      Assert.That (result, Is.EqualTo ("[CookTable] [c] ON 0 = 1"));
    }
  }
}