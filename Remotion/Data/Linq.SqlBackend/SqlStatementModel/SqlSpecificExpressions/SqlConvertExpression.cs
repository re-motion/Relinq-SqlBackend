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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// <see cref="SqlConvertExpression"/> is used to represent a convert expression.
  /// </summary>
  public class SqlConvertExpression : ExtensionExpression
  {
    private readonly Expression _source;
    private readonly Dictionary<Type, string> _sqlTypeMapper;

    public SqlConvertExpression (Type targetType, Expression source)
        : base (targetType)
    {
      ArgumentUtility.CheckNotNull ("source", source);

      _source = source;
      _sqlTypeMapper = new Dictionary<Type, string>
                       {
                           { typeof (string), "NVARCHAR" },
                           { typeof (int), "INT" },
                           { typeof (bool), "BIT" },
                       };
    }

    public Expression Source
    {
      get { return _source; }
    }

    public string GetSqlTypeName ()
    {
      if (_sqlTypeMapper.ContainsKey (Type))
        return _sqlTypeMapper[Type];
      else
        throw new KeyNotFoundException (string.Format ("No appropriate sql type for '{0}' found.", Type.Name));
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlResultExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlConvertExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}