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
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlEntityReferenceExpressionTest
  {
    [Test]
    public void Initialize ()
    {
      var columns = new[] { new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false) };
      var entityDefinitionExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true), columns);

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", entityDefinitionExpression);

      var exptecedPrimaryColumn = new SqlColumnReferenceExpression (typeof (int), "t", "ID", true, entityDefinitionExpression);
      var exppectedColumn = new SqlColumnReferenceExpression (typeof (string), "t", "Name", false, entityDefinitionExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (exptecedPrimaryColumn, entityReferenceExpression.PrimaryKeyColumn);
      ExpressionTreeComparer.CheckAreEqualTrees (exppectedColumn, entityReferenceExpression.Columns[0]);
    }

    [Test]
    public void GetColumn ()
    {
      var columns = new[] { new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false) };
      var entityDefinitionExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true), columns);

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", entityDefinitionExpression);

      var result = entityReferenceExpression.GetColumn (typeof (string), "Test", false);

      var exprectedresult = new SqlColumnReferenceExpression (typeof (string), "t", "Test", false, entityDefinitionExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (exprectedresult, result);
    }

    [Test]
    public void CreateReference ()
    {
      var columns = new[] { new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false) };
      var entityDefinitionExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true), columns);

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", entityDefinitionExpression);
      
      var exptectedResult = new SqlEntityReferenceExpression (typeof (Cook), "t", entityDefinitionExpression);

      var result = entityReferenceExpression.CreateReference ("t");

      ExpressionTreeComparer.CheckAreEqualTrees (exptectedResult, result);
    }

    [Test]
    public void Update ()
    {
      var columns = new[] { new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false) };
      var entityDefinitionExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true), columns);

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", entityDefinitionExpression);

      var result = entityReferenceExpression.Update (typeof (Kitchen), "f", null);

      var exptectedResult = new SqlEntityReferenceExpression (typeof (Kitchen), "f", entityDefinitionExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (exptectedResult, result);
    }
  }
}