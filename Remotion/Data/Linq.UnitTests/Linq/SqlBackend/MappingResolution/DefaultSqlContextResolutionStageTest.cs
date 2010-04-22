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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class DefaultSqlContextResolutionStageTest
  {
    private DefaultSqlContextResolutionStage _stage;

    [SetUp]
    public void SetUp ()
    {
      _stage = new DefaultSqlContextResolutionStage();
    }

    [Test]
    public void ApplyContext_Expression ()
    {
      var result = _stage.ApplyContext (Expression.Constant (false), SqlExpressionContext.PredicateRequired);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((ConstantExpression) ((BinaryExpression) result).Left).Value, Is.EqualTo (0));
      Assert.That (((SqlLiteralExpression) ((BinaryExpression) result).Right).Value, Is.EqualTo (1));
    }

    [Test]
    public void ApplyContext_SqlStatement()
    {
      var sqlStatement = SqlStatementModelObjectMother.CreateSqlStatementWithCook();

      var result = _stage.ApplyContext (sqlStatement, SqlExpressionContext.ValueRequired);
      
      Assert.That (result, Is.Not.SameAs(sqlStatement));
    }

  }
}