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
  public class SqlTextExpressionVisitorTest
  {
    [Test]
    public void VisitSqlColumnListExpression ()
    {
      var resolver = new SqlStatementResolverStub ();
      var sqlTable = new SqlTable (new ConstantTableSource (Expression.Constant ("Student", typeof (string))));
      var tableReferenceExpression = new SqlTableReferenceExpression (typeof (Student), sqlTable);

      SqlColumnListExpression sqlColumnListExpression = (SqlColumnListExpression) SqlExpressionVisitor.TranslateSqlTableReferenceExpressions (tableReferenceExpression, resolver);

      StringBuilder sb = new StringBuilder();
      SqlTextExpressionVisitor.TranslateSqlColumnListExpression (sqlColumnListExpression, sb);

      Assert.That (sb.ToString(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }
  }
}