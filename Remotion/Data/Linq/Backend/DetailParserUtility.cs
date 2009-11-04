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
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Backend
{
  public class DetailParserUtility
  {
    public static void CheckNumberOfArguments (MethodCallExpression expression, string methodName, int expectedArgumentCount)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("expectedArgumentCount", expectedArgumentCount);

      if (expression.Arguments.Count != expectedArgumentCount)
      {
        throw new ParserException (
            "at least " + expectedArgumentCount + " argument",
            expression.Arguments.Count + " arguments",
            methodName + " method call");
      }
    }

    public static void CheckParameterType<T> (MethodCallExpression expression, string methodName, int parameterIndex)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("parameterIndex", parameterIndex);

      if (!(expression.Arguments[parameterIndex] is T))
      {
        throw new ParserException (
            typeof (T).Name,
            expression.Arguments[parameterIndex].GetType().Name + " (" + expression.Arguments[parameterIndex] + ")",
            "argument " + parameterIndex + " of " + methodName + " method call");
      }
    }
  }
}
