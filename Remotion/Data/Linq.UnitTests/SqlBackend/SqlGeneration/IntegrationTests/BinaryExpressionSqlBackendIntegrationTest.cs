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
using Remotion.Data.Linq.Backend.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration.IntegrationTests
{
  [TestFixture]
  public class BinaryExpressionSqlBackendIntegrationTest : SqlBackendIntegrationTestBase
  {
    [Test]
    public void EqualExpression ()
    {
      CheckQuery (
          from c in Cooks where c.Name == "Huber" select c.FirstName,
          "SELECT [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[Name] = @1)",
          new CommandParameter ("@1", "Huber"));
    }

    [Test]
    public void EqualsAndAlsoExpression ()
    {
      CheckQuery (
          from c in Cooks where c.Name == "Huber" && c.FirstName == "Sepp" select c.FirstName,
          "SELECT [t0].[FirstName] FROM [CookTable] AS [t0] WHERE (([t0].[Name] = @1) AND ([t0].[FirstName] = @2))",
          new CommandParameter ("@1", "Huber"),
          new CommandParameter ("@2", "Sepp"));
    }

    [Test]
    public void EqualsExpressionWithNull ()
    {
      CheckQuery (
          from c in Cooks where c.Name == null select c.FirstName,
          "SELECT [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[Name] IS NULL)"
         );
    }

    [Test]
    public void GreaterThanExpressionInWhereClause ()
    {
      CheckQuery (
          from c in Cooks where c.ID > 0 select c.FirstName,
          "SELECT [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[ID] > @1)",
          new CommandParameter("@1", 0)
         );
    }

    [Test]
    public void BooleanTrueExpressionInWhereClause ()
    {
      CheckQuery (
          from c in Cooks where c.IsFullTimeCook == true select c.FirstName,
          "SELECT [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[IsFullTimeCook] = @1)",
          new CommandParameter ("@1", 1)
         );
    }

    [Test]
    public void BooleanFalseExpressionInWhereClause ()
    {
      CheckQuery (
          from c in Cooks where c.IsFullTimeCook == false select c.FirstName,
          "SELECT [t0].[FirstName] FROM [CookTable] AS [t0] WHERE ([t0].[IsFullTimeCook] = @1)",
          new CommandParameter ("@1", 0)
         );
    }


    //TODO: see SqlGeneratingExpressionVisitor.VisitBinaryExpressions for further tests
  }
}