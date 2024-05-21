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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Expression representing an <see cref="ICollection"/>, an <see cref="ICollection{T}"/> or an <see cref="IReadOnlyCollection{T}"/>.
  /// </summary>
  public class ConstantCollectionExpression : Expression
  {
    /// <summary>
    /// The collection that was the original <see cref="ConstantExpression"/>'s <see cref="ConstantExpression.Value"/>.
    /// </summary>
    public object Collection => _items;

    /// <summary>
    /// Indicates whether the <see cref="ConstantCollectionExpression.Collection"/> is without elements.
    /// </summary>
    public bool IsEmptyCollection { get; }

    /// <summary>
    /// The <see cref="System.Type"/> of the <see cref="ConstantCollectionExpression.Collection"/>.
    /// </summary>
    public override Type Type { get; }

    private readonly IEnumerable _items;

    /// <summary>
    /// Creates a <see cref="ConstantCollectionExpression"/> for the given <paramref name="collection"/>.
    /// </summary>
    public ConstantCollectionExpression (IEnumerable collection)
    {
      ArgumentUtility.CheckNotNull (nameof(collection), collection);

      // ReSharper disable PossibleMultipleEnumeration
      _items = collection;
      Type = collection.GetType();

      var enumerator = collection.GetEnumerator();
      IsEmptyCollection = !enumerator.MoveNext();
      // ReSharper restore PossibleMultipleEnumeration

      if (enumerator is IDisposable disposable)
        disposable.Dispose();
    }

    public IEnumerable<object> GetItems ()
    {
      return _items.Cast<object>();
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      if (visitor is IConstantCollectionExpressionVisitor collectionExpressionVisitor)
        return collectionExpressionVisitor.VisitConstantCollection (this);

      return base.Accept (visitor);
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      return this;
    }

    public override ExpressionType NodeType => ExpressionType.Constant;
    
    public override string ToString ()
    {
      var enumerator = _items.GetEnumerator();
      var stringBuilder = new StringBuilder (1024);
      stringBuilder.Append ('[');
      for (int i = 0; i < 5; i++)
      {
        if (!enumerator.MoveNext())
          break;

        if (i > 0)
          stringBuilder.Append (", ");

        stringBuilder.Append (enumerator.Current);
      }

      if (enumerator.MoveNext())
        stringBuilder.Append (", ...");

      stringBuilder.Append (']');

      if (enumerator is IDisposable disposable)
        disposable.Dispose();

      return stringBuilder.ToString();
    }
  }
}
