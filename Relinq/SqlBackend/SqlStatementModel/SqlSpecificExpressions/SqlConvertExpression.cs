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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// <see cref="SqlConvertExpression"/> is used to represent a convert expression.
  /// </summary>
  public class SqlConvertExpression : ExtensionExpression
  {
    private readonly Expression _source;
    private readonly Dictionary<Type, string> _sqlTypeMapping;

    public SqlConvertExpression (Type targetType, Expression source)
        : base (targetType)
    {
      ArgumentUtility.CheckNotNull ("source", source);

      _source = source;
      _sqlTypeMapping = new Dictionary<Type, string> 
                       {
                           { typeof (string), "NVARCHAR" },
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
    }

    public Expression Source
    {
      get { return _source; }
    }

    public string GetSqlTypeName ()
    {
      if (_sqlTypeMapping.ContainsKey (Type))
        return _sqlTypeMapping[Type];
      else
      {
        var message = string.Format (
            "Cannot obtain a SQL type for type '{0}'. Expression being converted: '{1}'", 
            Type.Name, 
            FormattingExpressionTreeVisitor.Format (_source));
        throw new NotSupportedException (message);
      }
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