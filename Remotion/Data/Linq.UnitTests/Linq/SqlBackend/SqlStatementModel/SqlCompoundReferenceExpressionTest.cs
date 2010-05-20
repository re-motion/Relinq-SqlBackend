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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlCompoundReferenceExpressionTest
  {
    private SqlCompoundReferenceExpression _compoundExpression;

    [SetUp]
    public void SetUp ()
    {
      var referencedTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (Cook));
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var subStatementTableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var newExpression = Expression.New (typeof (TypeForNewExpression).GetConstructors ()[0], new[] { Expression.Constant (0) }, (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));

      _compoundExpression = new SqlCompoundReferenceExpression (typeof (Cook), "Test", referencedTable, subStatementTableInfo, newExpression);
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlCompoundReferenceExpression, ISqlCompoundReferenceExpressionVisitor> (
          _compoundExpression,
          mock => mock.VisitSqlCompoundReferenceExpression(_compoundExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_compoundExpression);
    }

    [Test]
    public void VisitChildren ()
    {
      var visitorMock = MockRepository.GenerateMock<ExpressionTreeVisitor> ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_compoundExpression, visitorMock);

      Assert.That (result, Is.SameAs (_compoundExpression));
    }
  }
}