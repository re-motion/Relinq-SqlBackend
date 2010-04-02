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
            typeof (Convert).GetMethod ("ToString", new[] { typeof (int) }),
            typeof (Convert).GetMethod ("ToString", new[] { typeof (bool) }), 
            typeof (Convert).GetMethod ("ToString", new[] { typeof (object) }),
            typeof (Convert).GetMethod ("ToInt64", new[] { typeof (string) }),
            typeof (Convert).GetMethod ("ToInt64", new[] { typeof (bool) }),
            typeof (Convert).GetMethod ("ToInt32", new[] { typeof (string) }),
            typeof (Convert).GetMethod ("ToInt32", new[] { typeof (bool) }),
            typeof (Convert).GetMethod ("ToInt16", new[] { typeof (string) }),
            typeof (Convert).GetMethod ("ToInt16", new[] { typeof (bool) }),
            typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (string) }),
            typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (int) }),
        };

    public Expression Transform (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      return new SqlConvertExpression (methodCallExpression.Type, methodCallExpression.Object);
    }
  }
}