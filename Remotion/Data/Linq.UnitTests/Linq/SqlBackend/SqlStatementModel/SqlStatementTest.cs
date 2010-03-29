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
using System.Globalization;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel
{
  [TestFixture]
  public class SqlStatementTest
  {
    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void WhereCondition_ChecksType ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      sqlStatement.WhereCondition = Expression.Constant (1);
    }

    [Test]
    public void WhereCondition_CanBeSetToNull ()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement();
      sqlStatement.WhereCondition = Expression.Constant (true);
      sqlStatement.WhereCondition = null;

      Assert.That (sqlStatement.WhereCondition, Is.Null);
    }
  }
}