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
using System.Linq;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Retrieves <see cref="IMethodCallTransformer"/> instances based on an attribute implementing <see cref="IMethodCallTransformerAttribute"/>.
  /// </summary>
  public class AttributeBasedMethodCallTransformerProvider : IMethodCallTransformerProvider
  {
    public IMethodCallTransformer GetTransformer (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      var attributes = methodCallExpression.Method.GetCustomAttributes (typeof (IMethodCallTransformerAttribute), true);
      if (attributes.Length == 0)
        return null;

      if (attributes.Length == 1)
        return GetTransformer ((IMethodCallTransformerAttribute) attributes[0], methodCallExpression.Method);

      var message = string.Format (
          "The method '{0}.{1}' is attributed with more than one IMethodCallTransformerAttribute: {2}. Only one such attribute is allowed.", 
          methodCallExpression.Method.DeclaringType,
          methodCallExpression.Method.Name,
          SeparatedStringBuilder.Build (", ", attributes.Select (a => a.GetType().Name).OrderBy (name => name)));
      throw new NotSupportedException (message);
    }

    private IMethodCallTransformer GetTransformer (IMethodCallTransformerAttribute attribute, MethodInfo methodInfo)
    {
      try
      {
        return attribute.GetTransformer ();
      }
      catch (InvalidOperationException ex)
      {
        var message = string.Format (
            "The method '{0}.{1}' cannot be transformed to SQL. {2}", 
            methodInfo.DeclaringType, 
            methodInfo.Name, 
            ex.Message);
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}