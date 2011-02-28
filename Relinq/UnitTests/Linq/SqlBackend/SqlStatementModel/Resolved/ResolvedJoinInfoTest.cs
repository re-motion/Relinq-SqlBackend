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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class ResolvedJoinInfoTest
  {
    [Test]
    public void GetResolvedTableInfo ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo();

      var result = joinInfo.GetResolvedLeftJoinInfo().ForeignTableInfo;

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