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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend
{
  /// <summary>
  /// Provides utility services involving type comparisons.
  /// </summary>
  public class ConversionUtility
  {
    // Required to make a BinaryExpression where one operand is of an incompatible, but convertible type; eg, when only one side is nullable.
    public static BinaryExpression MakeBinaryWithOperandConversion (
        ExpressionType expressionType, 
        Expression left, 
        Expression right, 
        bool isLiftedToNull, 
        MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(left), left);
      ArgumentUtility.CheckNotNull (nameof(right), right);

      if (NeedsConversion (expressionType, left, right))
      {
        if (left.Type.IsAssignableFrom (right.Type))
          right = Expression.Convert (right, left.Type);
        else if (right.Type.IsAssignableFrom (left.Type))
          left = Expression.Convert (left, right.Type);
        else
        {
          left = Expression.Convert (left, typeof (object));
          right = Expression.Convert (right, typeof (object));
        }
      }

      return Expression.MakeBinary (expressionType, left, right, isLiftedToNull, methodInfo);
    }

    private static bool NeedsConversion (ExpressionType expressionType, Expression left, Expression right)
    {
      return expressionType != ExpressionType.Coalesce && left.Type != right.Type;
    }
  }
}