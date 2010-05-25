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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlColumnReferenceExpressionTest
  {
    private SqlEntityDefinitionExpression _entityExpression;
    private SqlColumnReferenceExpression _columnExpression;

    [SetUp]
    public void SetUp ()
    {
      _entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", true));
      
      _columnExpression = new SqlColumnReferenceExpression (typeof (string), "c", "columnName", false, _entityExpression);
    }

    [Test]
    public void Initialize_SetReferenceEntity ()
    {
      Assert.That (_columnExpression.ReferencedEntity, Is.SameAs (_entityExpression));
    }

    [Test]
    public void Update ()
    {
      var result = _columnExpression.Update (typeof (char), "f", "test", false);

      var expectedResult = new SqlColumnReferenceExpression (typeof (char), "f", "test", false, _entityExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (result, expectedResult);
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<SqlColumnReferenceExpression, ISqlColumnExpressionVisitor> (
          _columnExpression,
          mock => mock.VisitSqlColumnReferenceExpression (_columnExpression));
    }

    [Test]
    public void To_String ()
    {
      var result = _columnExpression.ToString();

      Assert.That (result, Is.EqualTo ("[c].[columnName] FROM [c].[Cook]"));
    }
  }
}