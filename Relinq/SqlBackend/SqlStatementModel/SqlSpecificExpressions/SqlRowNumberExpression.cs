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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// <see cref="SqlRowNumberExpression"/> represents the Sql ROW_NUMBER() function.
  /// </summary>
  public class SqlRowNumberExpression : ExtensionExpression
  {
    private readonly ReadOnlyCollection<Ordering> _orderings;

    public SqlRowNumberExpression (Ordering[] orderings)
        : base (typeof (int))
    {
      ArgumentUtility.CheckNotNull ("orderings", orderings);
      ArgumentUtility.CheckNotEmpty ("orderings", orderings);
      
      _orderings = Array.AsReadOnly (orderings);
    }

    public ReadOnlyCollection<Ordering> Orderings
    {
      get { return _orderings; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newOrderings = Orderings.Select (
          o =>
          {
            var newExpression = visitor.VisitExpression (o.Expression);
            return newExpression != o.Expression ? new Ordering (newExpression, o.OrderingDirection) : o;
          }).ToArray();

      if (!newOrderings.SequenceEqual (Orderings))
        return new SqlRowNumberExpression (newOrderings);
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlRowNumberExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("ROW_NUMBER() OVER (ORDER BY {0})", String.Join(",", Orderings.Select (o => o.ToString()).ToArray()));
    }
  }
}