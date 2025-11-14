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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// <see cref="SqlRowNumberExpression"/> represents the Sql ROW_NUMBER() function.
  /// </summary>
  public class SqlRowNumberExpression : Expression
  {
    private readonly ReadOnlyCollection<Ordering> _orderings;

    public SqlRowNumberExpression (Ordering[] orderings)
    {
      ArgumentUtility.CheckNotNull (nameof(orderings), orderings);
      ArgumentUtility.CheckNotEmpty (nameof(orderings), orderings);
      
      _orderings = Array.AsReadOnly (orderings);
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return typeof(int); }
    }

    public ReadOnlyCollection<Ordering> Orderings
    {
      get { return _orderings; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var newOrderings = Orderings.Select (
          o =>
          {
            var newExpression = visitor.Visit (o.Expression);
            return newExpression != o.Expression ? new Ordering (newExpression, o.OrderingDirection) : o;
          }).ToArray();

      if (!newOrderings.SequenceEqual (Orderings))
        return new SqlRowNumberExpression (newOrderings);
      else
        return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlRowNumber (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("ROW_NUMBER() OVER (ORDER BY {0})", String.Join(",", Orderings.Select (o => o.ToString()).ToArray()));
    }
  }
}