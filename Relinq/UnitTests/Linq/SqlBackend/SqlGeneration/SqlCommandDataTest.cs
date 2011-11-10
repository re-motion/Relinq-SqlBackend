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
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlCommandDataTest
  {
    private ParameterExpression _rowParameter;

    [SetUp]
    public void SetUp ()
    {
      _rowParameter = Expression.Parameter (typeof (IDatabaseResultRow), "row");
    }

    [Test]
    public void GetInMemoryProjection_NoConversionRequired ()
    {
      var body = Expression.Constant (0);
      var sqlCommandData = new SqlCommandData ("T", new CommandParameter[0], _rowParameter, body);

      var result = sqlCommandData.GetInMemoryProjection<int> ();

      var expectedExpression = Expression.Lambda<Func<IDatabaseResultRow, int>> (body, _rowParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void GetInMemoryProjection_ConversionRequired ()
    {
      var body = Expression.Constant (0);
      var sqlCommandData = new SqlCommandData ("T", new CommandParameter[0], _rowParameter, body);

      var result = sqlCommandData.GetInMemoryProjection<object> ();

      var expectedExpression = Expression.Lambda<Func<IDatabaseResultRow, object>> (Expression.Convert (body, typeof (object)), _rowParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
  }
}