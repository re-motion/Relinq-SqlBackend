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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlStatementBuilderTest
  {
    [Test]
    public void GetSqlStatement ()
    {
      var statementBuilder = new SqlStatementBuilder();
      var constantExpression = Expression.Constant ("test");
      statementBuilder.SelectProjection = constantExpression;
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable();
      statementBuilder.SqlTables.Add (sqlTable);

      var result = statementBuilder.GetSqlStatement();

      Assert.That (result.SelectProjection, Is.SameAs (constantExpression));
      Assert.That (result.SqlTables.Count, Is.EqualTo (1));
      Assert.That (result.SqlTables[0], Is.SameAs(sqlTable));
    }

    // TODO Review: Add a test that checks that all properties are correctly set by GetSqlStatment
    // TODO Review: Add a test that checks that all properties are correctly taken over by SqlStatementBuilder ctor taking a SqlStatement
  }
}