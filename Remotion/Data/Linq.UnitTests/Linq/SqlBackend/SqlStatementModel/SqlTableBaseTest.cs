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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlTableBaseTest
  {
    [Test]
    public void GetOrAddJoin_NewEntry ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      var owningTableAlias = "c";
      var entityExpression = new SqlEntityExpression (typeof(Cook), owningTableAlias, new SqlColumnExpression (typeof (string), owningTableAlias, "Name", false));
      var unresolvedJoinInfo = new UnresolvedJoinInfo (entityExpression, memberInfo, JoinCardinality.One);

      var joinedTable = sqlTable.GetOrAddLeftJoin (unresolvedJoinInfo, memberInfo);
      Assert.That (joinedTable.JoinInfo, Is.TypeOf (typeof (UnresolvedJoinInfo)));
      Assert.That (((UnresolvedJoinInfo) joinedTable.JoinInfo).MemberInfo, Is.SameAs (memberInfo));
      Assert.That (((UnresolvedJoinInfo) joinedTable.JoinInfo).OriginatingEntity.TableAlias, Is.SameAs (owningTableAlias));
    }
    
    [Test]
    public void GetOrAddJoin_GetEntry_Twice ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      var memberInfo = typeof (Cook).GetProperty ("FirstName");
      var owningTableAlias = "c";
      var entityExpression = new SqlEntityExpression (typeof(Cook), owningTableAlias, new SqlColumnExpression (typeof (string), owningTableAlias, "Name", false));
      var unresolvedJoinInfo = new UnresolvedJoinInfo (entityExpression, memberInfo, JoinCardinality.One);

      var joinedTable1 = sqlTable.GetOrAddLeftJoin (unresolvedJoinInfo, memberInfo);
      var originalJoinInfo = joinedTable1.JoinInfo;

      var joinedTable2 = sqlTable.GetOrAddLeftJoin (originalJoinInfo, memberInfo);

      Assert.That (joinedTable1.JoinSemantics, Is.EqualTo (JoinSemantics.Left));
      Assert.That (joinedTable2.JoinSemantics, Is.EqualTo (JoinSemantics.Left));
      Assert.That (joinedTable2, Is.SameAs (joinedTable1));
      Assert.That (joinedTable2.JoinInfo, Is.SameAs (originalJoinInfo));
    }
  }
}