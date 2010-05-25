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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using System.Linq.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
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
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage = "No appropriate sql type for 'Cook' found.")]
    public void GetSqlTypeName_KeyNotFound_ThrowsException ()
    {
      var convertExpression = new SqlConvertExpression (typeof (Cook), Expression.Constant ("1"));
      convertExpression.GetSqlTypeName ();
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
    
  }
}