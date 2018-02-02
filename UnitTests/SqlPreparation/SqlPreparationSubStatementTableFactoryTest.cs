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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationSubStatementTableFactoryTest
  {
    private ISqlPreparationStage _stageMock;
    private ISqlPreparationContext _context;
    private UniqueIdentifierGenerator _generator;
    private SqlPreparationSubStatementTableFactory _factory;
    private SqlStatement _statementWithOrderings;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlPreparationStage>();
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
      _generator = new UniqueIdentifierGenerator();
      _factory = new SqlPreparationSubStatementTableFactory (_stageMock, _context, _generator);

      var builderForStatementWithOrderings = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook ())
      {
        Orderings = {
            new Ordering (Expression.Constant ("order1"), OrderingDirection.Desc),
            new Ordering (Expression.Constant ("order2"), OrderingDirection.Asc),
        }
      };
      _statementWithOrderings = builderForStatementWithOrderings.GetSqlStatement ();
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithoutOrderings ()
    {
      var statementWithoutOrderings = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      var result = _factory.CreateSqlTableForStatement (
          statementWithoutOrderings,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      _stageMock.VerifyAllExpectations ();

      var tableInfo = result.SqlTable.TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));

      var subStatement = ((ResolvedSubStatementTableInfo) tableInfo).SqlStatement;
      Assert.That (subStatement, Is.SameAs (statementWithoutOrderings));

      Assert.That (result.WhereCondition, Is.Null);
      Assert.That (result.ExtractedOrderings, Is.Empty);

      var expectedItemSelector = new SqlTableReferenceExpression (result.SqlTable);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedItemSelector, result.ItemSelector);
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithOrderings_AndExtractOrderingsPolicy_ReturnsTableWithoutOrderings_WithNewProjection ()
    {
      var fakeSelectProjection = Expression.Constant (new KeyValuePair<Cook, KeyValuePair<string, string>> ());
      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg.Is (_context)))
          .Return (fakeSelectProjection);
      _stageMock.Replay ();

      var result = _factory.CreateSqlTableForStatement (
          _statementWithOrderings,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      _stageMock.VerifyAllExpectations ();

      var tableInfo = result.SqlTable.TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));

      var subStatement = ((ResolvedSubStatementTableInfo) tableInfo).SqlStatement;
      Assert.That (subStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) subStatement.DataInfo).ItemExpression, Is.SameAs (subStatement.SelectProjection));
      Assert.That (((StreamedSequenceInfo) subStatement.DataInfo).DataType, 
          Is.SameAs (typeof (IQueryable<KeyValuePair<Cook, KeyValuePair<string, string>>>)));

      var expectedSubStatementBuilder = new SqlStatementBuilder (_statementWithOrderings) 
          { 
            SelectProjection = fakeSelectProjection, 
            DataInfo = subStatement.DataInfo
          };
      expectedSubStatementBuilder.Orderings.Clear();
      Assert.That (subStatement, Is.EqualTo (expectedSubStatementBuilder.GetSqlStatement()));

      Assert.That (result.WhereCondition, Is.Null);
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithOrderings_AndExtractOrderingsPolicy_ItemSelector ()
    {
      var fakeSelectProjection = Expression.Constant (new KeyValuePair<Cook, KeyValuePair<string, string>> ());
      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg.Is (_context)))
          .Return (fakeSelectProjection);
      _stageMock.Replay ();

      var result = _factory.CreateSqlTableForStatement (
          _statementWithOrderings,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      _stageMock.VerifyAllExpectations ();

      var expectedItemSelector = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (result.SqlTable),
          result.SqlTable.ItemType.GetProperty ("Key"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedItemSelector, result.ItemSelector);
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithOrderings_AndExtractOrderingsPolicy_ExtractedOrderings ()
    {
      var fakeSelectProjection = Expression.Constant (new KeyValuePair<Cook, KeyValuePair<string, string>> ());
      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg.Is (_context)))
          .Return (fakeSelectProjection);
      _stageMock.Replay ();

      var result = _factory.CreateSqlTableForStatement (
          _statementWithOrderings,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      _stageMock.VerifyAllExpectations ();

      Assert.That (result.ExtractedOrderings.Count, Is.EqualTo (2));

      var valueMemberAccess1 = Expression.MakeMemberAccess (
          new SqlTableReferenceExpression (result.SqlTable),
          result.SqlTable.ItemType.GetProperty ("Value"));
      var expectedOrdering1 = Expression.MakeMemberAccess (valueMemberAccess1, valueMemberAccess1.Type.GetProperty ("Key"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedOrdering1, result.ExtractedOrderings[0].Expression);
      Assert.That (result.ExtractedOrderings[0].OrderingDirection, Is.EqualTo (OrderingDirection.Desc));

      var expectedOrdering2 = Expression.MakeMemberAccess (valueMemberAccess1, valueMemberAccess1.Type.GetProperty ("Value"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedOrdering2, result.ExtractedOrderings[1].Expression);
      Assert.That (result.ExtractedOrderings[1].OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithOrderings_AndExtractOrderingsPolicy_NewProjection_ContainsOrderings ()
    {
      var outerTupleCtor = typeof (KeyValuePair<Cook, KeyValuePair<string, string>>).GetConstructor (new[] { typeof (Cook), typeof (KeyValuePair<string, string>) });
      Debug.Assert (outerTupleCtor != null);
      var middleTupleCtor = typeof (KeyValuePair<string, string>).GetConstructor (new[] { typeof (string), typeof (string) });
      Debug.Assert (middleTupleCtor != null);

      var _middleTupleKeyGetter = GetTupleMethod (middleTupleCtor, "get_Key");
      var _middleTupleValueGetter = GetTupleMethod (middleTupleCtor, "get_Value");
      var _outerTupleKeyGetter = GetTupleMethod (outerTupleCtor, "get_Key");
      var _outerTupleValueGetter = GetTupleMethod (outerTupleCtor, "get_Value");

      var expectedSelectProjection = Expression.New (
          outerTupleCtor,
          new[] {
              _statementWithOrderings.SelectProjection,
              Expression.New (
                  middleTupleCtor,
                  new[] { 
                      _statementWithOrderings.Orderings[0].Expression,
                      _statementWithOrderings.Orderings[1].Expression},
                  _middleTupleKeyGetter,
                  _middleTupleValueGetter)},
          _outerTupleKeyGetter,
          _outerTupleValueGetter);

      var fakeSelectProjection = Expression.Constant (new KeyValuePair<Cook, KeyValuePair<string, string>> ());
      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (
              Arg<Expression>.Is.Anything, 
              Arg.Is (_context)))
          .WhenCalled (mi => SqlExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, (Expression) mi.Arguments[0]))
          .Return (fakeSelectProjection);
      _stageMock.Replay ();

      _factory.CreateSqlTableForStatement (
          _statementWithOrderings,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithOrderings_AndExtractOrderingsPolicy_WithTopExpression ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
      {
        TopExpression = Expression.Constant ("top"),
        Orderings = { new Ordering (Expression.Constant ("order1"), OrderingDirection.Asc) }
      };
      var statement = builder.GetSqlStatement ();
      var fakeSelectProjection = Expression.Constant (new KeyValuePair<Cook, string> ());

      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Matches (c => c == _context)))
          .Return (fakeSelectProjection);
      _stageMock.Replay ();

      var result = _factory.CreateSqlTableForStatement (
          statement,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);

      _stageMock.VerifyAllExpectations ();

      var sqlTable = result.SqlTable;
      Assert.That (sqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement.Orderings.Count, Is.EqualTo (1));
      Assert.That (result.ExtractedOrderings.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "The SQL Preparation stage must not change the type of the select projection.")]
    public void CreateSqlTableForSubStatement_WithOrderings_AndExtractOrderingsPolicy_InvalidPreparedExpression ()
    {
      var fakeSelectProjection = Expression.Constant (0);
      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg.Is (_context)))
          .Return (fakeSelectProjection);
      _stageMock.Replay ();

      _factory.CreateSqlTableForStatement (
          _statementWithOrderings,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithOrderings_AndDoNotExtractOrderingsPolicy_ReturnsTableWithoutOrderings_WithOriginalProjection ()
    {
      var result = _factory.CreateSqlTableForStatement (
          _statementWithOrderings,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.DoNotExtractOrderings);

      _stageMock.VerifyAllExpectations ();

      Assert.That (result.ExtractedOrderings, Is.Empty);
      
      var tableInfo = result.SqlTable.TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));

      var subStatement = ((ResolvedSubStatementTableInfo) tableInfo).SqlStatement;
      Assert.That (subStatement.Orderings, Is.Empty);
      Assert.That (subStatement.SelectProjection, Is.SameAs (_statementWithOrderings.SelectProjection));
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithOrderingsAndTopExpression_AndDoNotExtractOrderingsPolicy_ReturnsTableWithOrderings_WithOriginalProjection ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook())
      {
        TopExpression = Expression.Constant ("top"),
        Orderings = { new Ordering (Expression.Constant ("order1"), OrderingDirection.Asc) }
      };
      var statementWithOrderingsAndTopExpression = builder.GetSqlStatement ();

      var result = _factory.CreateSqlTableForStatement (
          statementWithOrderingsAndTopExpression,
          info => new SqlTable (info, JoinSemantics.Inner),
          OrderingExtractionPolicy.DoNotExtractOrderings);

      _stageMock.VerifyAllExpectations ();

      Assert.That (result.ExtractedOrderings, Is.Empty);
      
      var tableInfo = result.SqlTable.TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));

      var subStatement = ((ResolvedSubStatementTableInfo) tableInfo).SqlStatement;
      Assert.That (subStatement.Orderings, Is.Not.Empty);
      Assert.That (subStatement.SelectProjection, Is.SameAs (statementWithOrderingsAndTopExpression.SelectProjection));
    }

    private static MethodInfo GetTupleMethod (ConstructorInfo _middleTupleCtor, string methodName)
    {
      Debug.Assert (_middleTupleCtor.DeclaringType != null, "_middleTupleCtor.DeclaringType != null");
      return _middleTupleCtor.DeclaringType.GetMethod (methodName);
    }
  }
}