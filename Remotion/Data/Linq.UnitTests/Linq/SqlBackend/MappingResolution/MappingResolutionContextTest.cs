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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class MappingResolutionContextTest
  {
    private SqlEntityExpression _entityExpression;
    private SqlGroupingSelectExpression _groupingSelectExpression;
    private MappingResolutionContext _context;
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _context = new MappingResolutionContext();
      _entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      _groupingSelectExpression = new SqlGroupingSelectExpression (Expression.Constant ("key"), Expression.Constant ("element"));
      _sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner);
    }

    [Test]
    public void AddMapping_KeyExists ()
    {
      _context.AddSqlEntityMapping (_entityExpression, _sqlTable);
      _context.AddGroupReferenceMapping (_groupingSelectExpression, _sqlTable);

      Assert.That (_context.GetSqlTableForEntityExpression (_entityExpression), Is.SameAs (_sqlTable));
      Assert.That (_context.GetReferencedGroupSource (_groupingSelectExpression), Is.SameAs (_sqlTable));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "No associated table found for entity 'Cook'.")]
    public void GetSqlTableForEntityExpression_EntityDoesNotExist ()
    {
      _context.GetSqlTableForEntityExpression (_entityExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage = "No associated table found for grouping select expression 'IGrouping`2'.")]
    public void GetReferencedGroupSource_GroupingSelectExpressionDoesNotExist ()
    {
      _context.GetReferencedGroupSource (_groupingSelectExpression);
    }

    [Test]
    public void UpdateEntityAndAddMapping ()
    {
      var entity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      _context.AddSqlEntityMapping (entity, sqlTable);

      var result = (SqlEntityDefinitionExpression) _context.UpdateEntityAndAddMapping (entity, entity.Type, "newAlias", "newName");

      Assert.That (result.TableAlias, Is.EqualTo ("newAlias"));
      Assert.That (result.Name, Is.EqualTo ("newName"));
      Assert.That (_context.GetSqlTableForEntityExpression (result), Is.SameAs (sqlTable));
    }

    [Test]
    public void UpdateGroupingSelectAndAddMapping ()
    {
      var newKeyExpression = Expression.Constant ("newKey");
      var newElementExpression = Expression.Constant ("newElement");
      var newAggregationExpression1 = Expression.Constant ("agg1");
      var newAggregationExpression2 = Expression.Constant ("agg2");

      _context.AddGroupReferenceMapping (_groupingSelectExpression, _sqlTable);

      var result = _context.UpdateGroupingSelectAndAddMapping (
          _groupingSelectExpression, newKeyExpression, newElementExpression, new[] {newAggregationExpression1, newAggregationExpression2});

      Assert.That (result.KeyExpression, Is.SameAs (newKeyExpression));
      Assert.That (result.ElementExpression, Is.SameAs (newElementExpression));
      Assert.That (result.AggregationExpressions[0], Is.SameAs (newAggregationExpression1));
      Assert.That (result.AggregationExpressions[1], Is.SameAs (newAggregationExpression2));
      Assert.That (_context.GetReferencedGroupSource (result), Is.SameAs (_sqlTable));
    }
  }
}