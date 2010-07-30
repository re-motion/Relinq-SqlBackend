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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingExpressionVisitorTest
  {
    private IMappingResolver _resolverMock;
    private SqlTable _sqlTable;
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<IMappingResolutionStage>();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (Cook));
      _mappingResolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void VisitSqlTableReferenceExpression_ResolvesExpression ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();

      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (tableReferenceExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitSqlTableReferenceExpression_RevisitsResult ()
    {
      var tableReferenceExpression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveTableReferenceExpression (tableReferenceExpression, _mappingResolutionContext))
          .Return (fakeResult);
      _stageMock.Replay();
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (fakeResult))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (tableReferenceExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitMemberExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var expression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);

      var fakeResolvedExpression = new SqlLiteralExpression (1);
      _stageMock
          .Expect (mock => mock.ResolveMemberAccess (expression, memberInfo, _resolverMock, _mappingResolutionContext))
          .Return (fakeResolvedExpression);
      _stageMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResolvedExpression));
    }

    [Test]
    public void VisitMemberExpression_ResolvesSourceExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var expression = Expression.Constant (null, typeof (Cook));
      var memberExpression = Expression.MakeMemberAccess (expression, memberInfo);

      var fakeResolvedSourceExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (fakeResolvedSourceExpression);
      _resolverMock.Replay();

      var fakeResolvedExpression = new SqlLiteralExpression (1);
      _stageMock
          .Expect (mock => mock.ResolveMemberAccess (fakeResolvedSourceExpression, memberInfo, _resolverMock, _mappingResolutionContext))
          .Return (fakeResolvedExpression);
      _stageMock.Replay();

      ResolvingExpressionVisitor.ResolveExpression (memberExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void UnknownExpression ()
    {
      var unknownExpression = new CustomExpression (typeof (int));
      var result = ResolvingExpressionVisitor.ResolveExpression (unknownExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (unknownExpression));
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (sqlStatement, _mappingResolutionContext))
          .Return (sqlStatement);
      _stageMock.Replay();

      var result =
          (SqlSubStatementExpression) ResolvingExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result.SqlStatement, Is.EqualTo (expression.SqlStatement));
      _stageMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlSubStatementExpression_SimplifiesGroupAggregates ()
    {
      var simplifiableResolvedSqlStatement = CreateSimplifiableResolvedSqlStatement();

      var unresolvedSqlStatement = SqlStatementModelObjectMother.CreateSqlStatement (
          new AggregationExpression (
              typeof (int),
              new SqlTableReferenceExpression (simplifiableResolvedSqlStatement.SqlTables[0]),
              AggregationModifier.Count));
      var expression = new SqlSubStatementExpression (unresolvedSqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlStatement (unresolvedSqlStatement, _mappingResolutionContext))
          .Return (simplifiableResolvedSqlStatement);
      _stageMock
          .Expect (mock => mock.ResolveSelectExpression (Arg<Expression>.Is.Anything, Arg.Is (_mappingResolutionContext)))
          .Return (new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false));
      _stageMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (SqlColumnDefinitionExpression)));
    }

    [Test]
    public void VisitSqlFunctionExpression ()
    {
      var prefixExpression = Expression.Constant ("test");
      var argumentExpression = Expression.Constant (1);
      var sqlFunctionExpression = new SqlFunctionExpression (typeof (int), "FUNCNAME", prefixExpression, argumentExpression);

      var resolvedExpression = Expression.Constant ("resolved");
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (prefixExpression))
          .Return (resolvedExpression);
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (argumentExpression))
          .Return (resolvedExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlFunctionExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.TypeOf (typeof (SqlFunctionExpression)));
      Assert.That (((SqlFunctionExpression) result).Args[0], Is.SameAs (resolvedExpression));
      Assert.That (((SqlFunctionExpression) result).Args[1], Is.SameAs (resolvedExpression));
      _resolverMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitSqlConvertExpression ()
    {
      var expression = Expression.Constant (1);
      var sqlConvertExpression = new SqlConvertExpression (typeof (int), expression);

      var resolvedExpression = Expression.Constant ("1");
      _resolverMock
          .Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (resolvedExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlConvertExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.TypeOf (typeof (SqlConvertExpression)));
      Assert.That (result.Type, Is.EqualTo (typeof (int)));
      Assert.That (((SqlConvertExpression) result).Source, Is.SameAs (resolvedExpression));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitTypeBinaryExpression ()
    {
      var expression = Expression.Constant ("select");
      var typeBinaryExpression = Expression.TypeIs (expression, typeof (Chef));
      var resolvedTypeExpression = Expression.Constant ("resolved");

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (expression))
          .Return (expression);
      _resolverMock.Expect (mock => mock.ResolveTypeCheck (expression, typeof (Chef)))
          .Return (resolvedTypeExpression);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (resolvedTypeExpression))
          .Return (resolvedTypeExpression);
      _resolverMock.Replay();

      ResolvingExpressionVisitor.ResolveExpression (typeBinaryExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSqlEntityRefMemberExpression ()
    {
      var memberInfo = typeof (Cook).GetProperty ("ID");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var entityRefmemberExpression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      var result = ResolvingExpressionVisitor.ResolveExpression (entityRefmemberExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (entityRefmemberExpression));
    }

    [Test]
    public void VisitSqlEntityConstantExpression ()
    {
      var sqlEntityConstantExpression = new SqlEntityConstantExpression (typeof (Cook), "test", "key");

      var result = ResolvingExpressionVisitor.ResolveExpression (sqlEntityConstantExpression, _resolverMock, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (sqlEntityConstantExpression));
    }

    [Test]
    public void VisitBinaryExpression_NoNewExpressions ()
    {
      var leftExpression = Expression.Constant (1);
      var rightExpression = Expression.Constant (1);
      var expression = Expression.Equal (leftExpression, rightExpression);

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (leftExpression))
          .Return (leftExpression);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (rightExpression))
          .Return (rightExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage = "The results of constructor invocations can only be compared if the same ctors are used.")]
    public void VisitBinaryExpression_NewExpressionsWithDifferentCtors_ThrowsException ()
    {
      var leftArgumentExpression = Expression.Constant (1);
      var rightArgumentExpression1 = Expression.Constant (1);
      var rightArgumentExpression2 = Expression.Constant (2);
      var leftExpression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), leftArgumentExpression);
      var rightExpression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int), typeof (int) }), rightArgumentExpression1, rightArgumentExpression2);
      var expression = Expression.Equal (leftExpression, rightExpression);

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (leftArgumentExpression))
          .Return (leftArgumentExpression);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (rightArgumentExpression1))
          .Return (rightArgumentExpression1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (rightArgumentExpression2))
          .Return (rightArgumentExpression2);
      _resolverMock.Replay();

      ResolvingExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContext);
    }

    [Test]
    public void VisitBinaryExpression_NewExpressionWithOneArgument_ReturnsBinaryExpressionSequence ()
    {
      var leftArgumentExpression = Expression.Constant (1);
      var rightArgumentExpression = Expression.Constant (1);
      var leftExpression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), leftArgumentExpression);
      var rightExpression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), rightArgumentExpression);
      var expression = Expression.Equal (leftExpression, rightExpression);

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (leftArgumentExpression))
          .Return (leftArgumentExpression);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (rightArgumentExpression))
          .Return (rightArgumentExpression);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Equal));
      Assert.That (((BinaryExpression) result).Left, Is.SameAs (leftArgumentExpression));
      Assert.That (((BinaryExpression) result).Right, Is.SameAs (rightArgumentExpression));
    }

    [Test]
    public void VisitBinaryExpression_NewExpressionWithTwoArguments_ReturnsBinaryExpressionSequence ()
    {
      var leftArgumentExpression1 = Expression.Constant (1);
      var leftArgumentExpression2 = Expression.Constant (2);
      var rightArgumentExpression1 = Expression.Constant (1);
      var rightArgumentExpression2 = Expression.Constant (2);
      var leftExpression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int), typeof (int) }), leftArgumentExpression1, leftArgumentExpression2);
      var rightExpression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int), typeof (int) }), rightArgumentExpression1, rightArgumentExpression2);
      var expression = Expression.MakeBinary (ExpressionType.NotEqual, leftExpression, rightExpression);

      _resolverMock.Expect (mock => mock.ResolveConstantExpression (leftArgumentExpression1))
          .Return (leftArgumentExpression1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (rightArgumentExpression1))
          .Return (rightArgumentExpression1);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (leftArgumentExpression2))
          .Return (leftArgumentExpression2);
      _resolverMock.Expect (mock => mock.ResolveConstantExpression (rightArgumentExpression2))
          .Return (rightArgumentExpression2);
      _resolverMock.Replay();

      var result = ResolvingExpressionVisitor.ResolveExpression (expression, _resolverMock, _stageMock, _mappingResolutionContext);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.AndAlso));
      Assert.That (((BinaryExpression) result).Left, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) result).Right, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) result).Left.NodeType, Is.EqualTo (ExpressionType.NotEqual));
      Assert.That (((BinaryExpression) ((BinaryExpression) result).Left).Left, Is.SameAs (leftArgumentExpression1));
      Assert.That (((BinaryExpression) ((BinaryExpression) result).Left).Right, Is.SameAs (rightArgumentExpression1));
      Assert.That (((BinaryExpression) result).Right.NodeType, Is.EqualTo (ExpressionType.NotEqual));
      Assert.That (((BinaryExpression) ((BinaryExpression) result).Right).Left, Is.SameAs (leftArgumentExpression2));
      Assert.That (((BinaryExpression) ((BinaryExpression) result).Right).Right, Is.SameAs (rightArgumentExpression2));
    }

    private SqlStatement CreateSimplifiableResolvedSqlStatement ()
    {
      var dataInfo = new StreamedScalarValueInfo (typeof (int));

      var resolvedElementExpressionReference = new SqlColumnDefinitionExpression (typeof (string), "q0", "element", false);
      var resolvedSelectProjection = new AggregationExpression (
          typeof (int), resolvedElementExpressionReference, AggregationModifier.Min);

      var associatedGroupingSelectExpression = SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression();

      var resolvedJoinedGroupingSubStatement = SqlStatementModelObjectMother.CreateSqlStatement (associatedGroupingSelectExpression);
      var resolvedJoinedGroupingTable = new SqlTable (
          SqlStatementModelObjectMother.CreateResolvedJoinedGroupingTableInfo (resolvedJoinedGroupingSubStatement),
          JoinSemantics.Inner);

      return new SqlStatement (
          dataInfo,
          resolvedSelectProjection,
          new[] { resolvedJoinedGroupingTable },
          null,
          null,
          new Ordering[0],
          null,
          false,
          Expression.Constant (0),
          Expression.Constant (0));
    }
  }
}