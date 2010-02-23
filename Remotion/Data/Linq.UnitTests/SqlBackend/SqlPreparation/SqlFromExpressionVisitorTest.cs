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
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlFromExpressionVisitorTest
  {
    [Test]
    public void VisitConstantExpression_CreatesSqlTableExpression ()
    {
      var mainFromClause = new MainFromClause ("x", typeof (Student), Expression.Constant ("source"));
      var result = SqlFromExpressionVisitor.TranslateFromExpression (mainFromClause.FromExpression);

      Assert.That (result.TableSource, Is.TypeOf (typeof (ConstantTableSource)));
      Assert.That (((ConstantTableSource) result.TableSource).ConstantExpression, Is.SameAs (mainFromClause.FromExpression));
      Assert.That (result.Type, Is.SameAs (mainFromClause.FromExpression.Type));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The given expression type 'BinaryExpression' is not supported in from clauses. (Expression: '(0 & 0)')")]
    public void VisitNotSupportedExpression_ThrowsNotImplentedException ()
    {
      var expression = BinaryExpression.And (Expression.Constant (0), Expression.Constant (0));
      SqlFromExpressionVisitor.TranslateFromExpression (expression);
    }

    
  }
}