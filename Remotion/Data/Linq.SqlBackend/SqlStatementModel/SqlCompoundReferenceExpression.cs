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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  public class SqlCompoundReferenceExpression : ExtensionExpression
  {
    private readonly string _name;
    private readonly SqlTableBase _referencedTable;
    private readonly ResolvedSubStatementTableInfo _subStatementTableInfo;
    private readonly NewExpression _referencedNewExpression;

    public SqlCompoundReferenceExpression (
        Type type, string name, SqlTableBase referencedTable, ResolvedSubStatementTableInfo subStatementTableInfo, NewExpression referencedNewExpression)
        : base (ArgumentUtility.CheckNotNull ("type", type))
    {
      ArgumentUtility.CheckNotNull ("referencedTable", referencedTable);
      ArgumentUtility.CheckNotNull ("subStatementTableInfo", subStatementTableInfo);
      ArgumentUtility.CheckNotNull ("referencedNewExpression", referencedNewExpression);

      _name = name;
      _referencedTable = referencedTable;
      _subStatementTableInfo = subStatementTableInfo;
      _referencedNewExpression = referencedNewExpression;
    }

    public string Name
    {
      get { return _name; }
    }

    public SqlTableBase ReferencedTable
    {
      get { return _referencedTable; }
    }

    public ResolvedSubStatementTableInfo SubStatementTableInfo
    {
      get { return _subStatementTableInfo; }
    }

    public NewExpression ReferencedNewExpression
    {
      get { return _referencedNewExpression; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlCompoundReferenceExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlCompoundReferenceExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}