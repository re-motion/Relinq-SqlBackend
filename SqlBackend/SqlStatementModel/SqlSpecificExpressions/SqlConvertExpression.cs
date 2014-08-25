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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// <see cref="SqlConvertExpression"/> is used to represent a convert expression.
  /// </summary>
  public class SqlConvertExpression : ExtensionExpression
  {
    private readonly Expression _source;

    private static readonly Dictionary<Type, string> s_sqlTypeMapping = new Dictionary<Type, string> 
                                                          {
                                                              { typeof (string), "NVARCHAR(MAX)" },
                                                              { typeof (int), "INT" },
                                                              { typeof (bool), "BIT" },
                                                              { typeof (long), "BIGINT" },
                                                              { typeof (char), "CHAR" },
                                                              { typeof (DateTime), "DATETIME" },
                                                              { typeof (decimal), "DECIMAL" },
                                                              { typeof (double), "FLOAT" },
                                                              { typeof (short), "SMALLINT" },
                                                              { typeof (Guid), "UNIQUEIDENTIFIER" }
                                                          };

    public static string GetSqlTypeName (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      if (s_sqlTypeMapping.ContainsKey (type))
        return s_sqlTypeMapping[type];

      var underlyingType = Nullable.GetUnderlyingType (type);
      if (underlyingType != null)
        return GetSqlTypeName (underlyingType);

      return null;
    }

    public SqlConvertExpression (Type targetType, Expression source)
        : base (targetType)
    {
      ArgumentUtility.CheckNotNull ("source", source);

      _source = source;
    }

    public Expression Source
    {
      get { return _source; }
    }

    public string GetSqlTypeName ()
    {
      var typeName = GetSqlTypeName(Type);
      if (typeName == null)
      {
        var message = string.Format (
            "Cannot obtain a SQL type for type '{0}'. Expression being converted: '{1}'",
            Type.Name,
            FormattingExpressionTreeVisitor.Format (_source));
        throw new NotSupportedException (message);
      }

      return typeName;
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newSource = visitor.VisitExpression (_source);

      if (newSource != _source)
        return new SqlConvertExpression (Type, newSource);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlConvertExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("CONVERT({0}, {1})", GetSqlTypeName (), FormattingExpressionTreeVisitor.Format (_source));
    }
  }
}