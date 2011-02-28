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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// <see cref="SqlFunctionExpression"/> holds the sql specific function with its parameters.
  /// </summary>
  public class SqlFunctionExpression : ExtensionExpression
  {
    private readonly string _sqlFunctioName;
    private readonly ReadOnlyCollection<Expression> _args;

    public SqlFunctionExpression (Type type, string sqlFunctioName, params Expression[] args)
        : base (type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNullOrEmpty ("sqlFunctioName", sqlFunctioName);
      ArgumentUtility.CheckNotNull ("args", args);

      _args = Array.AsReadOnly(args);
      _sqlFunctioName = sqlFunctioName;
    }

    public string SqlFunctioName
    {
      get { return _sqlFunctioName; }
    }

    public ReadOnlyCollection<Expression> Args
    {
      get { return _args; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newPrefix = visitor.VisitExpression (_args[0]);
      var newArgs = visitor.VisitAndConvert (_args, "SqlFunctionExpression.VisitChildren");

      if ((_args != newArgs) || (_args[0] != newPrefix))
        return new SqlFunctionExpression (Type, _sqlFunctioName, newArgs.ToArray());
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlFunctionExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("{0}({1})", _sqlFunctioName, String.Join (",", _args.Select (arg => FormattingExpressionTreeVisitor.Format(arg)).ToArray()));
    }
  }
}