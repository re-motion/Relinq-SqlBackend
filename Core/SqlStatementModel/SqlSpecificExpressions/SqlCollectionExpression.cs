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
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// Represents a collection of values, each of which is itself represented by an <see cref="Expression"/>.
  /// </summary>
  public class SqlCollectionExpression : Expression
  {
    private readonly ReadOnlyCollection<Expression> _items;
    private readonly Type _type;

    public SqlCollectionExpression (Type type, IEnumerable<Expression> items)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("items", items);

      _type = type;
      _items = items.ToList().AsReadOnly();
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _type; }
    }

    public ReadOnlyCollection<Expression> Items
    {
      get { return _items; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var newItems = visitor.VisitAndConvert (_items, "SqlCollectionExpression.VisitChildren");
      if (newItems != _items)
        return new SqlCollectionExpression (Type, newItems);

      return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var collectionExpressionVisitor = visitor as ISqlCollectionExpressionVisitor;
      if (collectionExpressionVisitor != null)
        return collectionExpressionVisitor.VisitSqlCollectionExpression (this);

      return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return "(" + string.Join (",", _items.Select (e => e.ToString())) + ")";
    }
  }
}