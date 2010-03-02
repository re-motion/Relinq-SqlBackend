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
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratingExpressionVisitorTest
  {
    private SqlCommandBuilder _commandBuilder;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new SqlCommandBuilder();
    }

    [Test]
    public void GenerateSql_VisitSqlColumnExpression ()
    {
      var sqlColumnExpression = new SqlColumnExpression (typeof (int), "s", "ID");
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnListExpression ()
    {
      var sqlColumnListExpression = new SqlColumnListExpression (
          typeof (Cook),
          new[]
          {
              new SqlColumnExpression (typeof (string), "t", "ID"),
              new SqlColumnExpression (typeof (string), "t", "Name"),
              new SqlColumnExpression (typeof (string), "t", "City")
          });
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnListExpression, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandText () , Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void VisitConstantExpression_TrueParameter ()
    {
      var expression = Expression.Constant (true);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandParameters ().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitConstantExpression_FalseParameter ()
    {
      var expression = Expression.Constant (false);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandParameters ().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo (0));
    }

    [Test]
    public void VisitConstantExpression_StringParameter ()
    {
      var expression = Expression.Constant ("Test");
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder);

      Assert.That (_commandBuilder.GetCommandParameters ().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters ()[0].Value, Is.EqualTo ("Test"));
    }

    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
         "The expression '[2147483647]' cannot be translated to SQL text by this SQL generator. Expression type 'NotSupportedExpression' is not supported.")]
    [Test]
    public void GenerateSql_UnsupportedExpression ()
    {
      var unknownExpression = new NotSupportedExpression (typeof (int));
      SqlGeneratingExpressionVisitor.GenerateSql (unknownExpression, _commandBuilder);
    }


  }
}