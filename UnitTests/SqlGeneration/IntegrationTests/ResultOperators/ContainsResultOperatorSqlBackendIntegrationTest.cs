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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration.IntegrationTests.ResultOperators
{
  public class MyContainsResultOperatorHandler : ResultOperatorHandler<ContainsResultOperator>
  {
    public override void HandleResultOperator (ContainsResultOperator resultOperator, SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)

    {

      var dataInfo = sqlStatementBuilder.DataInfo;

      var preparedItemExpression = stage.PrepareResultOperatorItemExpression (resultOperator.Item, context);

      // No name required for the select projection inside of an IN expression

      // (If the expression is a constant collection, a name would even be fatal.)

      var sqlSubStatement = sqlStatementBuilder.GetStatementAndResetBuilder ();
      var subStatementExpression = sqlSubStatement.CreateExpression();
      var xmlParameter = Expression.Constant ("XML-VALUE");

      var items = (IEnumerable<string>) ((ConstantExpression) sqlSubStatement.SelectProjection).Value;



      sqlStatementBuilder.SelectProjection = new SqlInExpression (
          preparedItemExpression,
          new SqlCompositeCustomTextGeneratorExpression (
              typeof (object),
              new SqlCustomTextExpression ("(SELECT value FROM ", typeof (object)),
              new SqlFunctionExpression (typeof (object), "GetValuesFromXml", Expression.Constant ("XML-VALUE")),
              new SqlCustomTextExpression (")", typeof (object))));

      

      UpdateDataInfo (resultOperator, sqlStatementBuilder, dataInfo);

    }

  }

  [TestFixture]
  public class ContainsResultOperatorSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void Contains_WithConstantCollection1 ()
    {
      var cookNames = new[] { "hugo", "hans", "heinz" };
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (SELECT value FROM GetValuesFromXml(@1))",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));
    }

    [Test]
    public void Contains_WithQuery ()
    {
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Contains (s) select s.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CookTable] AS [t0] "
          + "WHERE [t0].[ID] "
          + "IN (SELECT [t1].[ID] FROM [CookTable] AS [t1])");
    }

    [Test]
    public void Contains_WithConstant ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where (from s2 in Cooks select s2).Contains (cook) select s.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1])",
          new CommandParameter("@1", 23));
    }

    [Test]
    public void Contains_WithDerivedType ()
    {
      var chef = new Chef { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where s.Assistants.Contains (chef) select s.ID,
          "SELECT [t0].[ID] AS [value] "
          + "FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID]))",
          new CommandParameter ("@1", 23));
    }

    [Test]
    public void Contains_WithConstantAndDependentQuery ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          from s in Cooks where s.Assistants.Contains (cook) select s.ID,
          "SELECT [t0].[ID] AS [value] "
          +"FROM [CookTable] AS [t0] WHERE @1 IN (SELECT [t1].[ID] FROM [CookTable] AS [t1] WHERE ([t0].[ID] = [t1].[AssistedID]))",
          new CommandParameter("@1", 23));
    }

    [Test]
    public void Contains_OnTopLevel ()
    {
      var cook = new Cook { ID = 23, FirstName = "Hugo", Name = "Heinrich" };
      CheckQuery (
          ( () => (from s in Cooks select s).Contains(cook)),
          "SELECT CONVERT(BIT, CASE WHEN @1 IN (SELECT [t0].[ID] FROM [CookTable] AS [t0]) THEN 1 ELSE 0 END) AS [value]",
          row => (object) row.GetValue<bool> (new ColumnID ("value", 0)),
          new CommandParameter("@1", 23) );
    }

    [Test]
    public void Contains_WithConstantCollection ()
    {
      var cookNames = new[] { "hugo", "hans", "heinz" };
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (@1, @2, @3)",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));

      IEnumerable<string> cookNamesAsEnumerable = new[] { "hugo", "hans", "heinz" };
      CheckQuery (
          from c in Cooks where cookNamesAsEnumerable.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (@1, @2, @3)",
          new CommandParameter ("@1", "hugo"),
          new CommandParameter ("@2", "hans"),
          new CommandParameter ("@3", "heinz"));
    }

    [Test]
    public void Contains_WithEmptyCollection ()
    {
      var cookNames = new string[] { };
      CheckQuery (
          from c in Cooks where cookNames.Contains (c.FirstName) select c.FirstName,
          "SELECT [t0].[FirstName] AS [value] FROM [CookTable] AS [t0] WHERE [t0].[FirstName] IN (SELECT NULL WHERE 1 = 0)");
    }
  }
}