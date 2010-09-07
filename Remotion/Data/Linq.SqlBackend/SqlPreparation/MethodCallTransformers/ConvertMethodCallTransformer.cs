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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  /// <summary>
  /// <see cref="ConvertMethodCallTransformer"/> implements <see cref="IMethodCallTransformer"/> for different methods of the <see cref="Convert"/>
  /// class.
  /// </summary>
  public class ConvertMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods =
        new[]
        {
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (int)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (bool)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (object)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (decimal)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (double)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (float)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (long)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (short)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (char)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToString", typeof (byte)),
          
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (string)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (bool)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (byte)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (char)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (DateTime)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (decimal)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (float)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (long)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (object)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt64", typeof (short)),

          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (string)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (bool)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (byte)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (char)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (DateTime)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (decimal)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (float)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (long)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (object)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt32", typeof (short)),

          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (string)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (bool)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (byte)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (char)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (DateTime)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (decimal)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (float)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (long)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (object)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToInt16", typeof (short)),

          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (string)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (int)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (char)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (byte)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (DateTime)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (decimal)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (double)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (float)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (long)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (object)),
          MethodCallTransformerUtility.GetStaticMethod (typeof (Convert), "ToBoolean", typeof (short))
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