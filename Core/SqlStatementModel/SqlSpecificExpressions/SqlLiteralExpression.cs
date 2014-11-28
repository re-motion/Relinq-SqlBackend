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
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// Represents a literal SQL constant value. This is similar to a <see cref="ConstantExpression"/>, but it isn't encapsulated as a SQL parameter,
  /// but rendered inline.
  /// </summary>
  public class SqlLiteralExpression : Expression
  {
    public static SqlLiteralExpression Null (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      if (!NullableTypeUtility.IsNullableType (type))
        throw new ArgumentException ("Type must be nullable.", "type");

      return new SqlLiteralExpression (null, type);
    }

    private readonly Type _type;

    [CanBeNull]
    private readonly object _value;

    public SqlLiteralExpression (int value, bool nullable = false)
        : this (value, nullable ? typeof (int?) : typeof (int))
    {
    }

    public SqlLiteralExpression (long value, bool nullable = false)
      : this (value, nullable ? typeof (long?) : typeof (long))
    {
    }

    public SqlLiteralExpression ([NotNull]string value)
      : this (ArgumentUtility.CheckNotNull ("value", value), typeof (string))
    {
    }

    public SqlLiteralExpression (double value, bool nullable = false)
      : this (value, nullable ? typeof (double?) : typeof (double))
    {
    }

    private SqlLiteralExpression ([CanBeNull]object value, [NotNull]Type type)
    {
      _type = type;
      _value = value;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _type; }
    }

    [CanBeNull]
    public object Value
    {
      get { return _value; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlLiteral (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      if (Value == null)
        return "NULL";
      else if (Value is string)
        return "\"" + Value + "\"";
      else
        return Value.ToString();
    }
  }
}