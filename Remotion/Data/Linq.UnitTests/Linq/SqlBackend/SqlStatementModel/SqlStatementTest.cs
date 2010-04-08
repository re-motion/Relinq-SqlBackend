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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlStatementTest
  {
    [Test]
    // TODO Review 2546: Change this test to call the ctor. Since this test is for the SqlStatement class, we shouldn't hide the details behind the ObjectMother here.
    [ExpectedException (typeof (ArgumentTypeException))]
    public void WhereCondition_ChecksType ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      SqlStatementModelObjectMother.CreateSqlStatementWithNewWhereCondition (sqlStatement, Expression.Constant (1));
    }

    [Test]
    // TODO Review 2546: Change this test to simply call the ctor with a null whereCondition; also add tests that check the same for all parameters that can be null
    public void WhereCondition_CanBeSetToNull ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithNewWhereCondition (sqlStatement, Expression.Constant (true));
      sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithNewWhereCondition (sqlStatement, null);
      
      Assert.That (sqlStatement.WhereCondition, Is.Null);
    }
  }
}