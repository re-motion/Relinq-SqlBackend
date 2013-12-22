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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// Represents a collection of values, each of which is itself represented by an <see cref="Expression"/>.
  /// </summary>
  public class SqlCollectionExpression : ExtensionExpression
  {
    private readonly ReadOnlyCollection<Expression> _items;

    public SqlCollectionExpression (Type type, IEnumerable<Expression> items)
        : base (type)
    {
      _items = items.ToList().AsReadOnly();
    }

    public ReadOnlyCollection<Expression> Items
    {
      get { return _items; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newItems = visitor.VisitAndConvert (_items, "SqlCollectionExpression.VisitChildren");
      if (newItems != _items)
        return new SqlCollectionExpression (Type, newItems);

      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var collectionExpressionVisitor = visitor as ISqlCollectionExpressionVisitor;
      if (collectionExpressionVisitor != null)
        return collectionExpressionVisitor.VisitSqlCollectionExpression (this);

      return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return "(" + SeparatedStringBuilder.Build (",", _items.Select (FormattingExpressionTreeVisitor.Format)) + ")";
    }
  }
}