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
using System.Linq.Expressions;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationFromExpressionVisitorTest
  {
    [Test]
    public void GetTableForFromExpression_ConstantExpression_ReturnsUnresolvedTable ()
    {
      var expression = Expression.Constant (new Cook[0]);
      var fromClause = new MainFromClause ("itemName", typeof (Cook), expression);

      var result = SqlPreparationFromExpressionVisitor.GetTableForFromClause (fromClause);

      Assert.That (result, Is.TypeOf (typeof (SqlTable)));

      var tableInfo = ((SqlTable) result).TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (UnresolvedTableInfo)));

      Assert.That (((UnresolvedTableInfo) tableInfo).ConstantExpression, Is.SameAs (expression));
      Assert.That (tableInfo.ItemType, Is.SameAs (typeof (Cook)));
    }
  }
}