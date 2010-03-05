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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlBackendIntegrationTest
  {
    private IQueryable<Cook> _cooks;
    private IQueryable<Kitchen> _kitchen;
    
    [SetUp]
    public void SetUp ()
    {
      _cooks = ExpressionHelper.CreateCookQueryable();
      _kitchen = ExpressionHelper.CreateKitchenQueryable();
    }

    [Test]
    public void SimpleSqlQuery_SelectAllFromQueryable ()
    {
      var queryable = SelectTestQueryGenerator.CreateSimpleQuery (_cooks);
      var result = GenerateSql (queryable);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [c].[ID],[c].[Name],[c].[City] FROM [Cook] AS [c]"));
    }

    [Test]
    public void SimpleSqlQuery_SelctPropertyFromQueryable ()
    {
      IQueryable<string> queryable = SelectTestQueryGenerator.CreateSimpleQueryWithProjection (_cooks);
      var result = GenerateSql (queryable);

      Assert.That (result.CommandText, Is.EqualTo ("SELECT [c].[FirstName] FROM [Cook] AS [c]"));
    }

    [Test]
    public void SelectQuery_WithJoin ()
    {
      var queryable = from c in _cooks select c.Substitution.FirstName;
      var result = GenerateSql (queryable);

      Assert.That (
          result.CommandText,
          Is.EqualTo (
          "SELECT [c].[FirstName] FROM [CookTable] AS [c] JOIN [SubstitutionTable] AS [s] ON [c].[ID] = [s].[SubstitutionID]"));
    }

    private SqlCommand GenerateSql<T> (IQueryable<T> queryable)
    {
      var queryModel = ExpressionHelper.ParseQuery (queryable.Expression);

      var queryModelVisitor = new SqlPreparationQueryModelVisitor();
      queryModelVisitor.VisitQueryModel (queryModel);
      var sqlStatement = queryModelVisitor.GetSqlStatement();

      var resolvingSqlStatementVisitor = new ResolvingSqlStatementVisitor (new SqlStatementResolverStub());
      resolvingSqlStatementVisitor.VisitSqlStatement (sqlStatement);

      var sqlTextGenerator = new SqlStatementTextGenerator();
      return sqlTextGenerator.Build (sqlStatement);
    }

    
    
  }
}