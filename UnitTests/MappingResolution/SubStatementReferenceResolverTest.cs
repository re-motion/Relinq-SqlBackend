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
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class SubStatementReferenceResolverTest
  {
    private IMappingResolutionContext _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new MappingResolutionContext();
    }

    [Test]
    public void ResolveSubStatementReferenceExpression_CreatesColumnExpression_ForNamedExpression ()
    {
      var referencedExpression = new NamedExpression ("test", Expression.Constant (0));
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (referencedExpression);

      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          referencedExpression, tableInfo, sqlTable, _context);

      var expectedResult = new SqlColumnDefinitionExpression (typeof (int), "q0", "test", false);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

   [Test]
    public void ResolveSubStatementReferenceExpression_CreatesNewExpressionWithReferences_ForNewExpressions ()
    {
      var newExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof(int)),
          new[] { new NamedExpression ("const", Expression.Constant (0)) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (newExpression);

      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (newExpression, tableInfo, sqlTable, _context);

      var expectedResult = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)),
          new Expression[] { new NamedExpression ("A", new SqlColumnDefinitionExpression (typeof (int), "q0", "const", false) ) },
          newExpression.Members);

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ResolveSubStatementReferenceExpression_CreatesNewExpressionWithReferences_ForNewExpressions_WithoutMembers ()
    {
      var newExpression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)),
          new[] { new NamedExpression ("const", Expression.Constant (0)) });

      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (newExpression);

      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (newExpression, tableInfo, sqlTable, _context);

      var expectedResult = Expression.New (
          TypeForNewExpression.GetConstructor (typeof (int)),
          new Expression[] { new NamedExpression ("m0", new SqlColumnDefinitionExpression (typeof (int), "q0", "const", false)) });

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ResolveSubStatementReferenceExpression_CreatesEntityReference_ForEntities ()
    {
      var entityDefinitionExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection = entityDefinitionExpression,
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (entityDefinitionExpression, tableInfo, sqlTable, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlEntityReferenceExpression)));
      Assert.That (_context.GetSqlTableForEntityExpression ((SqlEntityReferenceExpression) result), Is.SameAs (sqlTable));

      var expectedResult = entityDefinitionExpression.CreateReference ("q0", tableInfo.SqlStatement.SelectProjection.Type);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ResolveSubStatementReferenceExpression_CreatesSqlGroupingReferenceExpression_ForSqlGroupingSelectExpression ()
    {
      var groupingSelectExpression = new SqlGroupingSelectExpression (
          SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), "key"),  
          new NamedExpression ("element", Expression.Constant (0)), 
          new[] { new NamedExpression ("a0", Expression.Constant (1)) });
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
      {
        SelectProjection = groupingSelectExpression,
        DataInfo = new StreamedSequenceInfo (typeof (IGrouping<int, int>[]), Expression.Constant (null, typeof (IGrouping<int, int>)))
      }.GetSqlStatement ();
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (groupingSelectExpression, tableInfo, sqlTable, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlGroupingSelectExpression)));

      var referencedKeyExpression = ((SqlGroupingSelectExpression) result).KeyExpression;
      var expectedReferencedKeyExpression = new NamedExpression("key", new SqlEntityReferenceExpression (
          typeof (Cook), 
          "q0", 
          null, 
          (SqlEntityExpression) groupingSelectExpression.KeyExpression));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedReferencedKeyExpression, referencedKeyExpression);

      var referencedElementExpression = ((SqlGroupingSelectExpression) result).ElementExpression;
      var expectedReferencedElementExpression = new NamedExpression("element", new SqlColumnDefinitionExpression (typeof (int), "q0", "element", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedReferencedElementExpression, referencedElementExpression);

      Assert.That (((SqlGroupingSelectExpression) result).AggregationExpressions.Count, Is.EqualTo (1));
      var referencedAggregationExpression = ((SqlGroupingSelectExpression) result).AggregationExpressions[0];
      var expectedReferencedAggregationExpression = new NamedExpression("a0", new SqlColumnDefinitionExpression (typeof (int), "q0", "a0", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedReferencedAggregationExpression, referencedAggregationExpression);

      Assert.That (_context.GetReferencedGroupSource (((SqlGroupingSelectExpression) result)), Is.SameAs (sqlTable));
    }

    [Test]
    public void ResolveSubStatementReferenceExpression_CreatesSqlGroupingSelectExpressionWithNamedExpressions ()
    {
      var expression = new SqlGroupingSelectExpression (
          Expression.Constant ("key"), Expression.Constant ("element"), new[] { Expression.Constant ("aggregation") });
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)));
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable(typeof(Cook));

      var exprectedResult = new SqlGroupingSelectExpression (
          new NamedExpression ("key", Expression.Constant ("key")),
          new NamedExpression ("element", Expression.Constant ("element")),
          new[] { new NamedExpression ("a0", Expression.Constant ("aggregation")) });

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (expression, tableInfo, sqlTable, _context);

      SqlExpressionTreeComparer.CheckAreEqualTrees (result, exprectedResult);
      Assert.That (_context.GetReferencedGroupSource (((SqlGroupingSelectExpression) result)), Is.SameAs (sqlTable));
    }

    [Test]
    public void ResolveSubStatementReferenceExpression_CopiesExpression_ForUnknownExpression ()
    {
      var referencedExpression = Expression.Constant (0);
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (referencedExpression);

      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          referencedExpression, tableInfo, sqlTable, _context);

      Assert.That (result, Is.SameAs (referencedExpression));
    }

    [Test]
    public void ResolveSubStatementReferenceExpression_ResolvesChildren_ForUnknownExpression ()
    {
      var referencedExpression = new SqlConvertedBooleanExpression (new NamedExpression ("test", Expression.Constant (0)));
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (referencedExpression);

      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          referencedExpression, tableInfo, sqlTable, _context);

      var expectedResult = new SqlConvertedBooleanExpression (new SqlColumnDefinitionExpression (typeof (int), "q0", "test", false));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ResolveSubStatementReferenceExpression_RegistersEntityTable_WithinUnknownExpression ()
    {
      var referencedExpression = Expression.Convert (SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook)), typeof (Chef));
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (referencedExpression);

      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          referencedExpression, tableInfo, sqlTable, _context);

      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Convert));
      var entityReference = (SqlEntityExpression) ((UnaryExpression) result).Operand;
      Assert.That (_context.GetSqlTableForEntityExpression (entityReference), Is.SameAs (sqlTable));
      
    }
  }
}