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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationFromExpressionVisitorTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private SqlPreparationContext _context;
    private MethodCallTransformerRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _generator = new UniqueIdentifierGenerator();
      _context = new SqlPreparationContext();
      _registry = MethodCallTransformerRegistry.CreateDefault();
    }

    [Test]
    public void GetTableForFromExpression_ConstantExpression_ReturnsUnresolvedTable ()
    {
      var expression = Expression.Constant (new Cook[0]);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));

      var tableInfo = ((SqlTable) result.SqlTable).TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (UnresolvedTableInfo)));

      Assert.That (tableInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void GetTableForFromExpression_SqlMemberExpression_ReturnsSqlTableWithJoinedTable ()
    {
      // from r in Restaurant => sqlTable 
      // from c in r.Cooks => MemberExpression (QSRExpression (r), "Cooks") => Join: sqlTable.Cooks

      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (memberInfo.DeclaringType);
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant (new Restaurant()), memberInfo);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          memberExpression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));
      Assert.That (((SqlTable) result.SqlTable).TableInfo, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (sqlTable.JoinedTables.ToArray().Contains (result.SqlTable), Is.False);
      Assert.That (((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinSemantics, Is.EqualTo (JoinSemantics.Inner));

      var joinInfo = ((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinInfo;

      Assert.That (joinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));

      Assert.That (((UnresolvedCollectionJoinInfo) joinInfo).MemberInfo, Is.EqualTo (memberInfo));
      Assert.That (joinInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Expressions of type 'CustomExpression' cannot be used as the SqlTables of a from clause.")]
    public void GetTableForFromExpression_UnsupportedExpression_Throws ()
    {
      var customExpression = new CustomExpression (typeof (Cook[]));

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          customExpression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);
    }

    [ExpectedException (typeof (NotSupportedException))]
    [Test]
    public void VisitEntityRefMemberExpression_ThrowsNotSupportException ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
                         {
                             DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook()))
                         }.GetSqlStatement();

      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);

      var result = (SqlTable) SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          sqlSubStatementExpression,
          new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())),
          _stageMock,
          _generator,
          _registry,
          _context).SqlTable;

      Assert.That (result.TableInfo, Is.InstanceOfType (typeof (ResolvedSubStatementTableInfo)));
      var condition = (ResolvedSubStatementTableInfo) result.TableInfo;
      Assert.That (condition.SqlStatement, Is.EqualTo (sqlStatement));
      Assert.That (condition.TableAlias, Is.EqualTo ("q0"));
      Assert.That (condition.ItemType, Is.EqualTo (typeof (Cook)));
    }

    [Test]
    public void VisitSqlSubStatementExpression_WithOrderingsAndNoTopExpression ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
                    {
                        SelectProjection = Expression.Constant(new Cook()),
                        TopExpression = null,
                        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook()))
                    };
      builder.Orderings.Add (new Ordering (Expression.Constant ("order1"), OrderingDirection.Asc));
      var statement = builder.GetSqlStatement();
      var sqlSubStatementExpression = new SqlSubStatementExpression (statement);
      
      Expression newSelectProjection = Expression.Constant (null);
      Type tupleType;

      for (var i = statement.Orderings.Count - 1; i >= 0; --i)
      {
        tupleType = typeof (KeyValuePair<,>).MakeGenericType (statement.Orderings[i].Expression.Type, newSelectProjection.Type);
        newSelectProjection =
            Expression.New (
                tupleType.GetConstructors ()[0],
                new[] { statement.Orderings[i].Expression, newSelectProjection },
                new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });
      }

      tupleType = typeof (KeyValuePair<,>).MakeGenericType (statement.SelectProjection.Type, newSelectProjection.Type);
      var fakeSelectProjection = Expression.New (
          tupleType.GetConstructors ()[0],
          new[] { statement.SelectProjection, newSelectProjection },
          new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });
      
      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Matches(c=>c==_context)))
          .Return (fakeSelectProjection);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          sqlSubStatementExpression,
          new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())),
          _stageMock,
          _generator,
          _registry,
          _context);

      Assert.That (result.ItemSelector, Is.TypeOf (typeof (MemberExpression)));
      Assert.That (result.ExtractedOrderings.Count, Is.EqualTo (1));
    }

    [Test]
    public void VisitMemberExpression ()
    {
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant (new Cook()), typeof (Cook).GetProperty ("IllnessDays"));
      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          memberExpression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);

      Assert.That (result.SqlTable, Is.TypeOf (typeof (SqlTable)));
      Assert.That (((SqlTable) result.SqlTable).TableInfo, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));
      Assert.That (
          ((UnresolvedCollectionJoinInfo) ((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinInfo).SourceExpression,
          Is.EqualTo (memberExpression.Expression));
      Assert.That (
          ((UnresolvedCollectionJoinInfo) ((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo).JoinInfo).MemberInfo,
          Is.EqualTo (memberExpression.Member));
      var expectedWherecondition = new JoinConditionExpression (((SqlJoinedTable) ((SqlTable) result.SqlTable).TableInfo));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedWherecondition, result.WhereCondition);
    }

    [Test]
    public void VisitSqlTableReferenceExpression ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (memberInfo.DeclaringType);
      var expression = new SqlTableReferenceExpression (sqlTable);

      var result = SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);

      Assert.That (result.SqlTable, Is.SameAs (sqlTable));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityRefMemberExpression ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityConstantExpression ()
    {
      var expression = new SqlEntityConstantExpression (typeof (Cook), "test", "test");

      SqlPreparationFromExpressionVisitor.AnalyzeFromExpression (
          expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);
    }
  }
}