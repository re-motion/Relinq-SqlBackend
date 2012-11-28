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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  public class SqlLiteralExpression : ExtensionExpression
  {
    private readonly object _value;

    // TODO 3335: Refactor to use strongly typed ctors, allow optional specification of type.
    public SqlLiteralExpression (object value)
        : base (ArgumentUtility.CheckNotNull ("value", value).GetType())
    {
      if ((Type != typeof (int)) && (Type != typeof (string)) && Type != typeof (double))
      {
        var message = string.Format ("SqlLiteralExpression does not support values of type '{0}'.", Type);
        throw new ArgumentTypeException (message, "value", typeof (int), Type);
      }
      else
        _value = value;
    }

    public object Value
    {
      get { return _value; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlLiteralExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      if (Value is string)
        return "\"" + Value + "\"";
      else
        return Value.ToString();
    }
  }
}