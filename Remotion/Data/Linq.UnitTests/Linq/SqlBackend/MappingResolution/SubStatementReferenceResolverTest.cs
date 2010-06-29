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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
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
    public void CreateReferenceExpression_CreatesCompoundExpression_ForNewExpressions ()
    {
      var newExpression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor(new[]{typeof(int)}),
          new[] { new NamedExpression ("const", Expression.Constant (0)) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection = newExpression,
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          newExpression, tableInfo, sqlTable, newExpression.Type, _context);

      var expectedResult = Expression.New (
          typeof (TypeForNewExpression).GetConstructors ()[0],
          new Expression[] { new NamedExpression ("A", new SqlColumnDefinitionExpression (typeof (int), "q0", "const", false) ) },
          newExpression.Members);


      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateReferenceExpression_CreatesSqlEntityExpression ()
    {
      var entityDefinitionExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection =
                                 entityDefinitionExpression,
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          entityDefinitionExpression, tableInfo, sqlTable, entityDefinitionExpression.Type, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlEntityReferenceExpression)));
      Assert.That (_context.GetSqlTableForEntityExpression ((SqlEntityReferenceExpression) result), Is.SameAs (sqlTable));

      var expectedResult = ((SqlEntityExpression) tableInfo.SqlStatement.SelectProjection).CreateReference (
        "q0", tableInfo.SqlStatement.SelectProjection.Type);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateReferenceExpression_CreatesSqlValueReferenceExpression ()
    {
      var namedExpression = new NamedExpression ("test", Expression.Constant (5));
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection = namedExpression,
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          namedExpression, tableInfo, sqlTable, namedExpression.Type, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlColumnDefinitionExpression)));
      Assert.That (((SqlColumnDefinitionExpression) result).ColumnName, Is.EqualTo ("test"));
      Assert.That (((SqlColumnDefinitionExpression) result).OwningTableAlias, Is.EqualTo (tableInfo.TableAlias));
      Assert.That (result.Type, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void CreateReferenceExpression_CreatesSqlGroupingReferenceExpression ()
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

      var result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          groupingSelectExpression, tableInfo, sqlTable, groupingSelectExpression.Type, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlGroupingSelectExpression)));

      var referencedKeyExpression = ((SqlGroupingSelectExpression) result).KeyExpression;
      var expectedReferencedKeyExpression = new SqlEntityReferenceExpression (
          typeof (Cook), 
          "q0", 
          null, 
          (SqlEntityExpression) groupingSelectExpression.KeyExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReferencedKeyExpression, referencedKeyExpression);

      var referencedElementExpression = ((SqlGroupingSelectExpression) result).ElementExpression;
      var expectedReferencedElementExpression = new SqlColumnDefinitionExpression (typeof (int), "q0", "element", false);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReferencedElementExpression, referencedElementExpression);

      Assert.That (((SqlGroupingSelectExpression) result).AggregationExpressions.Count, Is.EqualTo (1));
      var referencedAggregationExpression = ((SqlGroupingSelectExpression) result).AggregationExpressions[0];
      var expectedReferencedAggregationExpression = new SqlColumnDefinitionExpression (typeof (int), "q0", "a0", false);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReferencedAggregationExpression, referencedAggregationExpression);
    }
  }
}