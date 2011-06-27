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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
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
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);

      if (left.Type != right.Type)
      {
        if (left.Type.IsAssignableFrom (right.Type))
          right = Expression.Convert (right, left.Type);
        else if (right.Type.IsAssignableFrom (left.Type))
          left = Expression.Convert (left, right.Type);
      }

      return Expression.MakeBinary (expressionType, left, right, isLiftedToNull, methodInfo);
    }
  }
}