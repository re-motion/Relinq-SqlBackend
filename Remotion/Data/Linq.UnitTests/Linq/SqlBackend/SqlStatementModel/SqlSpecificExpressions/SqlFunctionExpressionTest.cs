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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  [TestFixture]
  public class SqlFunctionExpressionTest
  {
    private SqlFunctionExpression _sqlFunctionExpression;

    [SetUp]
    public void SetUp ()
    {
      _sqlFunctionExpression = new SqlFunctionExpression (typeof (string), "Test", Expression.Constant("test"), Expression.Constant (1), Expression.Constant (2));
    }

    [Test]
    public void VisitChildren ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();
      var expression = Expression.Constant (3);

      visitorMock
          .Expect (mock => mock.VisitExpression (_sqlFunctionExpression.Prefix))
          .Return (_sqlFunctionExpression.Prefix);
      visitorMock
          .Expect (mock => mock.VisitExpression (_sqlFunctionExpression.Args[0]))
          .Return (expression);
      visitorMock
          .Expect (mock => mock.VisitExpression (_sqlFunctionExpression.Args[1]))
          .Return (expression);
      visitorMock.Replay();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlFunctionExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.Not.SameAs (_sqlFunctionExpression));
    }


  }
}