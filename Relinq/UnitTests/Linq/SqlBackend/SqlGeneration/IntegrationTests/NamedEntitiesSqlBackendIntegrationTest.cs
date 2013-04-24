// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class NamedEntitiesSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void NamedEntity ()
    {
      // from c in Cooks select c [with name = "inner"]
      var subMainFromClause = new MainFromClause ("c", typeof(Cook), Expression.Constant (Cooks));
      var subSelectClause = new SelectClause (new NamedExpression ("inner", new QuerySourceReferenceExpression (subMainFromClause)));
      var subQuery = new QueryModel (subMainFromClause, subSelectClause);

      // from x in (subQuery) where x.ID == null select x
      var outerMainFromClause = new MainFromClause ("x", typeof(Cook), new SubQueryExpression (subQuery));
      var outerWhereClause = new WhereClause(Expression.Equal(Expression.MakeMemberAccess (new QuerySourceReferenceExpression (outerMainFromClause), typeof (Cook).GetProperty ("ID")), Expression.Constant (1)));
      var outerSelectClause = new SelectClause (new QuerySourceReferenceExpression (outerMainFromClause));
      var outerQuery = new QueryModel (outerMainFromClause, outerSelectClause);
      outerQuery.BodyClauses.Add (outerWhereClause);

      CheckQuery (
          outerQuery, 
          "SELECT [q0].[inner_ID] AS [ID],[q0].[inner_FirstName] AS [FirstName],[q0].[inner_Name] AS [Name],"
          + "[q0].[inner_IsStarredCook] AS [IsStarredCook],[q0].[inner_IsFullTimeCook] AS [IsFullTimeCook],"
          + "[q0].[inner_SubstitutedID] AS [SubstitutedID],[q0].[inner_KitchenID] AS [KitchenID],"
          + "[q0].[inner_KnifeID] AS [KnifeID],[q0].[inner_KnifeClassID] AS [KnifeClassID] " 
          + "FROM (SELECT [t1].[ID] AS [inner_ID],"
          + "[t1].[FirstName] AS [inner_FirstName],[t1].[Name] AS [inner_Name],[t1].[IsStarredCook] AS [inner_IsStarredCook],"
          + "[t1].[IsFullTimeCook] AS [inner_IsFullTimeCook],[t1].[SubstitutedID] AS [inner_SubstitutedID],"
          + "[t1].[KitchenID] AS [inner_KitchenID],[t1].[KnifeID] AS [inner_KnifeID],[t1].[KnifeClassID] AS [inner_KnifeClassID] "
          + "FROM [CookTable] AS [t1]) AS [q0] WHERE ([q0].[inner_ID] = @1)",
          new CommandParameter("@1", 1));
    }

  }
}