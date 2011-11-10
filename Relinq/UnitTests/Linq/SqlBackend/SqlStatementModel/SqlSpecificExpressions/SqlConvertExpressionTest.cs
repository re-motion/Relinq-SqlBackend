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
using NUnit.Framework;
using System.Linq.Expressions;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlConvertExpressionTest
  {
    private SqlConvertExpression _convertExpresion;

    [SetUp]
    public void SetUp ()
    {
      _convertExpresion = new SqlConvertExpression (typeof (int), Expression.Constant ("1"));
    }

    [Test]
    public void GetSqlTypeName ()
    {
      var result = _convertExpresion.GetSqlTypeName();
      Assert.That (result, Is.EqualTo ("INT"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "Cannot obtain a SQL type for type 'Cook'. Expression being converted: '\"1\"'")]
    public void GetSqlTypeName_KeyNotFound_ThrowsException ()
    {
      var convertExpression = new SqlConvertExpression (typeof (Cook), Expression.Constant ("1"));
      convertExpression.GetSqlTypeName ();
    }

    [Test]
    public void GetSqlTypeName_AllMappedTypes ()
    {
      CheckSqlTypeName (typeof (string), "NVARCHAR(MAX)");
      CheckSqlTypeName (typeof (int), "INT");
      CheckSqlTypeName (typeof (bool), "BIT");
      CheckSqlTypeName (typeof (long), "BIGINT");
      CheckSqlTypeName (typeof (char), "CHAR");
      CheckSqlTypeName (typeof (DateTime), "DATETIME");
      CheckSqlTypeName (typeof (decimal), "DECIMAL");
      CheckSqlTypeName (typeof (double), "FLOAT");
      CheckSqlTypeName (typeof (short), "SMALLINT");
      CheckSqlTypeName (typeof (Guid), "UNIQUEIDENTIFIER");
    }

    [Test]
    public void VisitChildren_NewSource ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();
      var newPrefix = Expression.Constant (3);

      visitorMock
          .Expect (mock => mock.VisitExpression (_convertExpresion.Source))
          .Return (newPrefix);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_convertExpresion, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.Not.SameAs (_convertExpresion));
    }

    [Test]
    public void VisitChildren_SameSource ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_convertExpresion.Source))
          .Return (_convertExpresion.Source);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_convertExpresion, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (_convertExpresion));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlConvertExpression, ISqlSpecificExpressionVisitor> (
          _convertExpresion,
          mock => mock.VisitSqlConvertExpression (_convertExpresion));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_convertExpresion);
    }

    [Test]
    public void To_String ()
    {
      var result = _convertExpresion.ToString();

      Assert.That (result, Is.EqualTo ("CONVERT(INT, \"1\")"));
    }
 
    private void CheckSqlTypeName (Type type, string expectedSqlTypeName)
    {
      var result = new SqlConvertExpression (type, new CustomExpression (type));
      Assert.That (result.GetSqlTypeName(), Is.EqualTo (expectedSqlTypeName));
    }
  }
}