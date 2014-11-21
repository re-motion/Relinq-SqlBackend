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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
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
      _entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null, "c");
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
    public void GetSqlTableForEntityExpression_EntityDoesNotExist ()
    {
      Assert.That (
          () => _context.GetSqlTableForEntityExpression (_entityExpression),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "No associated table found for entity '[c]'."));
    }

    [Test]
    public void GetReferencedGroupSource_GroupingSelectExpressionDoesNotExist ()
    {
      Assert.That (
          () => _context.GetReferencedGroupSource (_groupingSelectExpression),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "No associated table found for grouping select expression 'GROUPING (KEY: \"key\", ELEMENT: \"element\", AGGREGATIONS: ())'."));
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

    [Test]
    public void UpdateGroupingSelectAndAddMapping_MappingDoesNotExist ()
    {
      var newKeyExpression = Expression.Constant ("newKey");
      var newElementExpression = Expression.Constant ("newElement");
      var newAggregationExpression1 = Expression.Constant ("agg1");
      var newAggregationExpression2 = Expression.Constant ("agg2");

      var result = _context.UpdateGroupingSelectAndAddMapping (
          _groupingSelectExpression, newKeyExpression, newElementExpression, new[] { newAggregationExpression1, newAggregationExpression2 });

      Assert.That (result.KeyExpression, Is.SameAs (newKeyExpression));
      Assert.That (result.ElementExpression, Is.SameAs (newElementExpression));
      Assert.That (result.AggregationExpressions[0], Is.SameAs (newAggregationExpression1));
      Assert.That (result.AggregationExpressions[1], Is.SameAs (newAggregationExpression2));
    }

    [Test]
    public void RemoveNamesAndUpdateMapping_Unnamed ()
    {
      var wrappedExpression = Expression.Constant (0);

      var result = _context.RemoveNamesAndUpdateMapping (wrappedExpression);

      Assert.That (result, Is.SameAs (wrappedExpression));
    }

    [Test]
    public void RemoveNamesAndUpdateMapping_Named ()
    {
      var wrappedExpression = Expression.Constant (0);
      var namedExpression = new NamedExpression ("X", wrappedExpression);

      var result = _context.RemoveNamesAndUpdateMapping (namedExpression);

      Assert.That (result, Is.SameAs (wrappedExpression));
    }

    [Test]
    public void RemoveNamesAndUpdateMapping_DoubleNamed ()
    {
      var wrappedExpression = Expression.Constant (0);
      var namedExpression = new NamedExpression ("X", wrappedExpression);
      var namedNamedExpression = new NamedExpression ("Y", namedExpression);

      var result = _context.RemoveNamesAndUpdateMapping (new NamedExpression ("outer", namedNamedExpression));
      Assert.That (result, Is.SameAs (wrappedExpression));
    }

    [Test]
    public void RemoveNamesAndUpdateMapping_Entity ()
    {
      var entity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (name: "X");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      _context.AddSqlEntityMapping (entity, sqlTable);

      var result = _context.RemoveNamesAndUpdateMapping (entity);

      Assert.That (result, Is.TypeOf (entity.GetType ()));
      Assert.That (result.Type, Is.SameAs (entity.Type));
      Assert.That (((SqlEntityExpression) result).TableAlias, Is.EqualTo (entity.TableAlias));
      Assert.That (((SqlEntityExpression) result).Name, Is.Null);

      Assert.That (_context.GetSqlTableForEntityExpression ((SqlEntityExpression) result), Is.SameAs (sqlTable));
    }

    [Test]
    public void RemoveNamesAndUpdateMapping_NameAroundEntity ()
    {
      var entity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (name: "X");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      _context.AddSqlEntityMapping (entity, sqlTable);

      var result = _context.RemoveNamesAndUpdateMapping (new NamedExpression ("X", entity));

      Assert.That (result, Is.TypeOf (entity.GetType ()));
      Assert.That (((SqlEntityExpression) result).Name, Is.Null);
      Assert.That (_context.GetSqlTableForEntityExpression ((SqlEntityExpression) result), Is.SameAs (sqlTable));
    }

    [Test]
    public void AddOriginatingEntityMappingForUnresolvedCollectionJoinTableInfo_AllowsEntityToBeLookedUp ()
    {
      var unresolvedCollectionJoinTableInfo = SqlStatementModelObjectMother.CreateUnresolvedCollectionJoinTableInfo_RestaurantCooks();
      var resolvedOriginatingEntity = SqlStatementModelObjectMother.CreateSqlEntityExpression();

      _context.AddOriginatingEntityMappingForUnresolvedCollectionJoinTableInfo (unresolvedCollectionJoinTableInfo, resolvedOriginatingEntity);

      var result = _context.GetOriginatingEntityForUnresolvedCollectionJoinTableInfo (unresolvedCollectionJoinTableInfo);
      Assert.That (result, Is.SameAs (resolvedOriginatingEntity));
    }

    [Test]
    public void AddOriginatingEntityMappingForUnresolvedCollectionJoinTableInfo_Twice_SecondMappingOverwritesFirst ()
    {
      var unresolvedCollectionJoinTableInfo = SqlStatementModelObjectMother.CreateUnresolvedCollectionJoinTableInfo_RestaurantCooks();
      var resolvedOriginatingEntity1 = SqlStatementModelObjectMother.CreateSqlEntityExpression();
      var resolvedOriginatingEntity2 = SqlStatementModelObjectMother.CreateSqlEntityExpression();

      _context.AddOriginatingEntityMappingForUnresolvedCollectionJoinTableInfo (unresolvedCollectionJoinTableInfo, resolvedOriginatingEntity1);
      _context.AddOriginatingEntityMappingForUnresolvedCollectionJoinTableInfo (unresolvedCollectionJoinTableInfo, resolvedOriginatingEntity2);

      var result = _context.GetOriginatingEntityForUnresolvedCollectionJoinTableInfo (unresolvedCollectionJoinTableInfo);
      Assert.That (result, Is.SameAs (resolvedOriginatingEntity2));
    }

    [Test]
    public void GetOriginatingEntityForUnresolvedCollectionJoinTableInfo_WithoutAddedMapping_ThrowsKeyNotFoundException ()
    {
      var unresolvedCollectionJoinTableInfo = SqlStatementModelObjectMother.CreateUnresolvedCollectionJoinTableInfo_RestaurantCooks();

      Assert.That (
          () => _context.GetOriginatingEntityForUnresolvedCollectionJoinTableInfo (unresolvedCollectionJoinTableInfo),
          Throws.TypeOf<KeyNotFoundException>().With.Message.EqualTo (
              "An originating entity for the giben UnresolvedCollectionJoinTableInfo has not been registered. "
              + "Make sure the UnresolvedCollectionJoinTableInfo is resolved before the referencing UnresolvedCollectionJoinConditionExpression is."));
    }
  }
}