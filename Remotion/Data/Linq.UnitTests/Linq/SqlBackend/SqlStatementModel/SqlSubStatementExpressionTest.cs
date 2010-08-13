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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
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

      Assert.That (result, Is.EqualTo ("(SELECT [t] FROM [Table] [t])"));
    }

    [Test]
    public void CreateSqlTableForSubStatement ()
    {
      var selectProjection = Expression.Constant (new Cook());
      var sqlStatement =
          new SqlStatementBuilder() { DataInfo = new StreamedSingleValueInfo (typeof (Cook), false), SelectProjection = selectProjection }.
              GetSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = expression.CreateSqlTableForSubStatement (expression, JoinSemantics.Inner, "q0");

      Assert.That (result.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
      Assert.That (result.TableInfo.GetResolvedTableInfo().TableAlias, Is.EqualTo ("q0"));
      Assert.That (result.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      var newSubStatement = ((ResolvedSubStatementTableInfo) result.TableInfo).SqlStatement;
      Assert.That (newSubStatement.SelectProjection, Is.SameAs (sqlStatement.SelectProjection));
      Assert.That (newSubStatement.TopExpression, Is.Null);
      Assert.That (newSubStatement.SqlTables, Is.EqualTo (sqlStatement.SqlTables));
      Assert.That (newSubStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) newSubStatement.DataInfo).DataType, Is.SameAs (typeof (IEnumerable<>).MakeGenericType (typeof (Cook))));
      Assert.That (((StreamedSequenceInfo) newSubStatement.DataInfo).ItemExpression, Is.SameAs (selectProjection));
    }

    [Test]
    public void CreateSqlTableForSubStatement_StreamedForceSingleValueInfo ()
    {
      var selectProjection = Expression.Constant (new Cook());
      var sqlStatement =
          new SqlStatementBuilder()
          {
              DataInfo = new StreamedForcedSingleValueInfo (typeof (Cook), false),
              SelectProjection = selectProjection,
              TopExpression = new SqlLiteralExpression (2)
          }.GetSqlStatement();
      var expression = new SqlSubStatementExpression (sqlStatement);

      var result = expression.CreateSqlTableForSubStatement (expression, JoinSemantics.Inner, "q0");

      Assert.That (result.JoinSemantics, Is.EqualTo (JoinSemantics.Inner));
      Assert.That (result.TableInfo.GetResolvedTableInfo().TableAlias, Is.EqualTo ("q0"));
      Assert.That (result.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      var newSubStatement = ((ResolvedSubStatementTableInfo) result.TableInfo).SqlStatement;
      Assert.That (newSubStatement.SelectProjection, Is.SameAs (sqlStatement.SelectProjection));
      Assert.That (newSubStatement.SqlTables, Is.EqualTo (sqlStatement.SqlTables));
      Assert.That (newSubStatement.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) newSubStatement.DataInfo).DataType, Is.SameAs (typeof (IEnumerable<>).MakeGenericType (typeof (Cook))));
      Assert.That (((StreamedSequenceInfo) newSubStatement.DataInfo).ItemExpression, Is.SameAs (selectProjection));
      ExpressionTreeComparer.CheckAreEqualTrees (new SqlLiteralExpression (1), newSubStatement.TopExpression);
    }
  }
}