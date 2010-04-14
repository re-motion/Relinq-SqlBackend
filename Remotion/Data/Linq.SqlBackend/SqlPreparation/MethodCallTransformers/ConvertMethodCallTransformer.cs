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
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="ConvertMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for the convert method.
  /// </summary>
  public class ConvertMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods =
        new[]
        {
          // TODO Review 2510: Add more overloads/convert methods
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (int)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (bool)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (object)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (string)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (bool)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (string)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (bool)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (string)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (bool)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (string)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (int))
        };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      MethodCallTransformerUtility.CheckArgumentCount (methodCallExpression, 1);
      MethodCallTransformerUtility.CheckStaticMethod (methodCallExpression);

      return new SqlConvertExpression (methodCallExpression.Type, methodCallExpression.Arguments[0]);
    }
  }
}