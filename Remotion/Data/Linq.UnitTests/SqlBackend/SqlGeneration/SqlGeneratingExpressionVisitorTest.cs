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
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratingExpressionVisitorTest
  {
    [Test]
    public void GenerateSql_VisitSqlColumnExpression ()
    {
      // TODO: Remove when SqlColumnExpression only takes a string
      
      var sqlColumnExpression = new SqlColumnExpression (typeof (int), "s", "ID");

      StringBuilder sb = new StringBuilder ();
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, sb);

      Assert.That (sb.ToString (), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnListExpression ()
    {
      var resolver = new SqlStatementResolverStub ();
      var tableSource = new ConstantTableSource (Expression.Constant ("Student", typeof (string)));
      var sqlTable = new SqlTable ();
      sqlTable.TableSource = tableSource;
      var tableReferenceExpression = new SqlTableReferenceExpression (sqlTable);

      SqlColumnListExpression sqlColumnListExpression = (SqlColumnListExpression) ResolvingExpressionVisitor.TranslateSqlTableReferenceExpressions (tableReferenceExpression, resolver);

      // TODO: var sqlColumnListExpression = new SqlColumnListExpression (typeof (Student), new[] { new SqlColumnExpression (typeof (string), sqlTable, "ID") , ...

      StringBuilder sb = new StringBuilder();
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnListExpression, sb);

      Assert.That (sb.ToString(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    // TODO: Test case where unsupported expression is passed to visitor
    // [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
    //     "The expression '...' cannot be translated to SQL text by this SQL generator. Expression type '...' is not supported.")]
  }
}