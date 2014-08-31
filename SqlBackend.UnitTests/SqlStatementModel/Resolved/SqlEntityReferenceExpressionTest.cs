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
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Resolved
{
  [TestFixture]
  public class SqlEntityReferenceExpressionTest
  {
    private SqlEntityDefinitionExpression _entityDefinitionExpression;

    [SetUp]
    public void SetUp ()
    {
      var columns = new[] { new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false) };
      _entityDefinitionExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, e => e, columns);
    }

    [Test]
    public void Initialize ()
    {
      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", null, _entityDefinitionExpression);

      var expectedColumn = new SqlColumnReferenceExpression (typeof (string), "t", "Name", false, _entityDefinitionExpression);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedColumn, entityReferenceExpression.Columns[0]);

      Assert.That (entityReferenceExpression.IdentityExpressionGenerator, Is.SameAs (_entityDefinitionExpression.IdentityExpressionGenerator));
    }

    [Test]
    public void GetColumn ()
    {
      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", null, _entityDefinitionExpression);

      var result = entityReferenceExpression.GetColumn (typeof (string), "Test", false);

      var exprectedresult = new SqlColumnReferenceExpression (typeof (string), "t", "Test", false, _entityDefinitionExpression);

      SqlExpressionTreeComparer.CheckAreEqualTrees (exprectedresult, result);
    }

    [Test]
    public void CreateReference ()
    {
      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", null, _entityDefinitionExpression);
      
      var exptectedResult = new SqlEntityReferenceExpression (typeof (Cook), "t", null, entityReferenceExpression);

      var result = entityReferenceExpression.CreateReference ("t", _entityDefinitionExpression.Type);

      SqlExpressionTreeComparer.CheckAreEqualTrees (exptectedResult, result);
    }

    [Test]
    public void Update ()
    {
      var entityReferenceExpression = new SqlEntityReferenceExpression (typeof (Cook), "t", null, _entityDefinitionExpression);

      var result = entityReferenceExpression.Update (typeof (Kitchen), "f", "testName");

      var exptectedResult = new SqlEntityReferenceExpression (typeof (Kitchen), "f", "testName", _entityDefinitionExpression);

      Assert.That (result.Name, Is.EqualTo ("testName"));
      SqlExpressionTreeComparer.CheckAreEqualTrees (exptectedResult, result);
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