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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.SqlBackend
{
  [TestFixture]
  public class SqlBackendIntegrationTest
  {
    [Test]
    public void SimpleSqlQuery ()
    {
      var mainFromClause = new MainFromClause ("t", typeof (string), Expression.Constant ("Table"));
      var selectClause = ClauseObjectMother.CreateSelectClause (mainFromClause);
      var queryModel = new QueryModel (mainFromClause, selectClause);
      
      var queryModelVisitor = new SqlQueryModelVisitor();
      queryModelVisitor.VisitQueryModel (queryModel);
      var sqlStatement = queryModelVisitor.GetSqlStatement();
      
      var resolvingSqlStatementVisitor = new ResolvingSqlStatementVisitor (new SqlStatementResolverStub());
      resolvingSqlStatementVisitor.VisitSqlStatement (sqlStatement);

      var sqlTextGenerator = new SqlStatementTextGenerator();
      var result = sqlTextGenerator.Build (sqlStatement);

      Assert.That (result, Is.EqualTo ("SELECT [t].[ID],[t].[Name],[t].[City] FROM [Table] AS [t]"));
    }
  }
}