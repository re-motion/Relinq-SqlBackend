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
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class SqlSubStatementExpressionTest
  {
    private SqlSubStatementExpression _expression;
    private SqlStatement _sqlStatement;

    [SetUp]
    public void SetUp ()
    {
      _sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      _expression = new SqlSubStatementExpression (_sqlStatement);
    }

    [Test]
    public void Initialization_ItemType ()
    {
      Assert.That (_expression.Type, Is.EqualTo (_sqlStatement.DataInfo.DataType));
    }

    [Test]
    public void VisitChildren_ReturnsThis_WithoutCallingVisitMethods ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor>();
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      Assert.That (result, Is.SameAs (_expression));
      visitorMock.VerifyAllExpectations();
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlSubStatementExpression, ISqlSubStatementVisitor> (
          _expression,
          mock => mock.VisitSqlSubStatementExpression (_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public void To_String ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook));
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = expression.ToString();

      Assert.That (result, Is.EqualTo ("(SELECT [t0] FROM [Table] [t])"));
    }

    [Test]
    public void ConvertToSqlTable ()
    {
      var selectProjection = Expression.Constant (new Cook());
      var sqlStatement =
          new SqlStatementBuilder { DataInfo = new StreamedSingleValueInfo (typeof (Cook), false), SelectProjection = selectProjection }.
              GetSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = expression.ConvertToSqlTable ("q0");

      Assert.That (result.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
      Assert.That (result.TableInfo.GetResolvedTableInfo().TableAlias, Is.EqualTo ("q0"));
      Assert.That (result.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      
      var newSubStatement = ((ResolvedSubStatementTableInfo) result.TableInfo).SqlStatement;
      var expectedSubStatement = new SqlStatementBuilder (sqlStatement)
      {
        DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<Cook>), sqlStatement.SelectProjection)
      }.GetSqlStatement();

      Assert.That (newSubStatement, Is.EqualTo (expectedSubStatement));
    }

    [Test]
    public void ConvertToSqlTable_StreamedSequenceInfo ()
    {
      var selectProjection = Expression.Constant (new Cook ());
      var sqlStatement =
          new SqlStatementBuilder { DataInfo = new StreamedSequenceInfo(typeof (Cook[]), Expression.Constant(new Cook())), SelectProjection = selectProjection }.
              GetSqlStatement ();
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = expression.ConvertToSqlTable ("q0");

      Assert.That (result.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
      Assert.That (result.TableInfo.GetResolvedTableInfo ().TableAlias, Is.EqualTo ("q0"));
      Assert.That (result.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) result.TableInfo).SqlStatement, Is.EqualTo(sqlStatement));
    }

    [Test]
    public void ConvertToSqlTable_StreamedSingleValueInfo ()
    {
      var selectProjection = Expression.Constant (new Cook());
      var sqlStatement =
          new SqlStatementBuilder
          {
              DataInfo = new StreamedSingleValueInfo (typeof (Cook), false),
              SelectProjection = selectProjection,
              TopExpression = new SqlLiteralExpression (2),
              SqlTables = { new SqlTable(new ResolvedSimpleTableInfo(typeof(Cook), "CookTable", "c"),JoinSemantics.Inner) }
          }.GetSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = expression.ConvertToSqlTable ("q0");

      Assert.That (result.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
      Assert.That (result.TableInfo.GetResolvedTableInfo().TableAlias, Is.EqualTo ("q0"));
      Assert.That (result.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
     
      var newSubStatement = ((ResolvedSubStatementTableInfo) result.TableInfo).SqlStatement;
      var expectedSubStatement = new SqlStatementBuilder (sqlStatement) 
      { 
        DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<Cook>), sqlStatement.SelectProjection), 
        TopExpression = newSubStatement.TopExpression
      }.GetSqlStatement();

      SqlExpressionTreeComparer.CheckAreEqualTrees (new SqlLiteralExpression (1), newSubStatement.TopExpression);
      Assert.That (newSubStatement, Is.EqualTo (expectedSubStatement));
    }

    [Test]
    public void ConvertToSqlTable_StreamedSingleValueInfo_NoSqlTables ()
    {
      var selectProjection = Expression.Constant (new Cook ());
      var topExpression = new SqlLiteralExpression (2);
      var sqlStatement =
          new SqlStatementBuilder
          {
            DataInfo = new StreamedSingleValueInfo (typeof (Cook), false),
            SelectProjection = selectProjection,
            TopExpression = topExpression
          }.GetSqlStatement ();
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = expression.ConvertToSqlTable ("q0");

      Assert.That (result.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
      Assert.That (result.TableInfo.GetResolvedTableInfo ().TableAlias, Is.EqualTo ("q0"));
      Assert.That (result.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));

      var newSubStatement = ((ResolvedSubStatementTableInfo) result.TableInfo).SqlStatement;
      Assert.That (newSubStatement.TopExpression, Is.SameAs (topExpression));
    }

    [Test]
    public void ConvertToSqlTable_StreamedSingleValueInfoWithReturnsTrueIfEmpty_JoinSemanticIsChanged ()
    {
      var selectProjection = Expression.Constant (new Cook ());
      var sqlStatement =
          new SqlStatementBuilder
          {
            DataInfo = new StreamedSingleValueInfo (typeof (Cook), true),
            SelectProjection = selectProjection,
            TopExpression = new SqlLiteralExpression (2),
            SqlTables = { new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"), JoinSemantics.Inner) }
          }.GetSqlStatement ();
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = expression.ConvertToSqlTable ("q0");

      Assert.That (result.JoinSemantics, Is.EqualTo (JoinSemantics.Left));
      Assert.That (result.TableInfo.GetResolvedTableInfo ().TableAlias, Is.EqualTo ("q0"));
      Assert.That (result.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));

      var newSubStatement = ((ResolvedSubStatementTableInfo) result.TableInfo).SqlStatement;
      var expectedSubStatement = new SqlStatementBuilder (sqlStatement)
      {
        DataInfo = new StreamedSequenceInfo (typeof (IEnumerable<Cook>), sqlStatement.SelectProjection),
        TopExpression = newSubStatement.TopExpression
      }.GetSqlStatement ();

      SqlExpressionTreeComparer.CheckAreEqualTrees (new SqlLiteralExpression (1), newSubStatement.TopExpression);
      Assert.That (newSubStatement, Is.EqualTo (expectedSubStatement));
    }
  }
}