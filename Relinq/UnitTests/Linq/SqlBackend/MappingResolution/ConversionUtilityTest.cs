// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Linq.SqlBackend.MappingResolution;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ConversionUtilityTest
  {
    [Test]
    public void MakeBinaryWithOperandConversion_LeftOperand_LiftedToTypeOfRight ()
    {
      var left = Expression.Constant (0, typeof (int));
      var right = Expression.Constant (0, typeof (int?));

      var result = ConversionUtility.MakeBinaryWithOperandConversion (ExpressionType.Equal, left, right, false, null);

      var expectedExpression = BinaryExpression.Equal (Expression.Convert (left, typeof (int?)), right);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void MakeBinaryWithOperandConversion_RightOperand_LiftedToTypeOfLeft ()
    {
      var left = Expression.Constant (0, typeof (int?));
      var right = Expression.Constant (0, typeof (int));

      var result = ConversionUtility.MakeBinaryWithOperandConversion (ExpressionType.Equal, left, right, false, null);

      var expectedExpression = BinaryExpression.Equal (left, Expression.Convert (right, typeof (int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void MakeBinaryWithOperandConversion_NoLifting ()
    {
      var left = Expression.Constant (0, typeof (int?));
      var right = Expression.Constant (0, typeof (int?));

      var result = ConversionUtility.MakeBinaryWithOperandConversion (ExpressionType.Equal, left, right, false, null);

      var expectedExpression = BinaryExpression.Equal (left, right);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
  }
}