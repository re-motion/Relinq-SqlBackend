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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlGroupingReferenceExpression"/> represents a reference to a sub-statement that has a <see cref="SqlGroupingSelectExpression"/>
  /// as its <see cref="SqlStatement.SelectProjection"/>.
  /// </summary>
  public class SqlGroupingReferenceExpression : ExtensionExpression
  {
    private readonly SqlGroupingSelectExpression _referencedExpression;
    private readonly string _owningTableAlias;

    public SqlGroupingReferenceExpression (SqlGroupingSelectExpression referencedExpression, string owningTableAlias)
        : base (ArgumentUtility.CheckNotNull ("referencedExpression", referencedExpression).Type)
    {
      ArgumentUtility.CheckNotNull ("owningTableAlias", owningTableAlias);

      _referencedExpression = referencedExpression;
      _owningTableAlias = owningTableAlias;
    }

    public SqlGroupingSelectExpression ReferencedExpression
    {
      get { return _referencedExpression; }
    }

    public string OwningTableAlias
    {
      get { return _owningTableAlias; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlGroupingReferenceExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlGroupingReferenceExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("GROUPING-REF({0})", _owningTableAlias);
    }
  }
}