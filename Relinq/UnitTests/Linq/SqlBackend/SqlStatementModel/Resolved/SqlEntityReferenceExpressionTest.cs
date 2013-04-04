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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
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

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", null, entityDefinitionExpression);

      var exptecedPrimaryColumn = new SqlColumnReferenceExpression (typeof (int), "t", "ID", true, entityDefinitionExpression);
      var exppectedColumn = new SqlColumnReferenceExpression (typeof (string), "t", "Name", false, entityDefinitionExpression);

      ExpressionTreeComparer.CheckAreEqualTrees (exptecedPrimaryColumn, entityReferenceExpression.PrimaryKey);
      ExpressionTreeComparer.CheckAreEqualTrees (exppectedColumn, entityReferenceExpression.Columns[0]);
    }

    [Test]
    public void GetColumn ()
    {
      var columns = new[] { new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false) };
      var entityDefinitionExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true), columns);

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", null, entityDefinitionExpression);

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

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", null, entityDefinitionExpression);
      
      var exptectedResult = new SqlEntityReferenceExpression (typeof (Cook), "t", null, entityReferenceExpression);

      var result = entityReferenceExpression.CreateReference ("t", entityDefinitionExpression.Type);

      ExpressionTreeComparer.CheckAreEqualTrees (exptectedResult, result);
    }

    [Test]
    public void Update ()
    {
      var columns = new[] { new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false) };
      var entityDefinitionExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", true), columns);

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", null, entityDefinitionExpression);

      var result = entityReferenceExpression.Update (typeof (Kitchen), "f", "testName");

      var exptectedResult = new SqlEntityReferenceExpression (typeof (Kitchen), "f", "testName", entityDefinitionExpression);

      Assert.That (result.Name, Is.EqualTo ("testName"));
      ExpressionTreeComparer.CheckAreEqualTrees (exptectedResult, result);
    }

    [Test]
    public void ToString_UnnamedReferencedEntity ()
    {
      var referencedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), null); 

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "q0", null, referencedEntity);

      var result = entityReferenceExpression.ToString();

      Assert.That (result, Is.EqualTo ("[q0] (ENTITY-REF)"));
    }

    [Test]
    public void ToString_NamedReferencedEntity ()
    {
      var referencedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), "e1");

      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "q0", null, referencedEntity);

      var result = entityReferenceExpression.ToString ();

      Assert.That (result, Is.EqualTo ("[q0].[e1] (ENTITY-REF)"));
    }
  }
}