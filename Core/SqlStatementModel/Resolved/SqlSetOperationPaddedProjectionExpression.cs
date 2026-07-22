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

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlSetOperationPaddedProjectionExpression"/> represents a SELECT projection that has been extended with additional named
  /// slots (typically <c>NULL AS ...</c> placeholders) so that its rendered column list lines up positionally with the other side(s) of a
  /// set operation (e.g. UNION) whose entity types have a different set of mapped columns.
  /// One slot may be flagged as the "preserved" slot via <see cref="PreservedEntitySlotIndex"/>: for the outermost statement of a query, this
  /// must be the original whole-entity expression that is actually materialized into an object, so that the outer select-expression visitor can
  /// keep building its in-memory projection solely from that slot, while still rendering (and advancing column positions for) the extra padding
  /// slots. Combined-statement sides of a set operation are never materialized directly, so they never set this index.
  /// </summary>
  public class SqlSetOperationPaddedProjectionExpression : Expression
  {
    private readonly Type _type;
    private readonly ReadOnlyCollection<Expression> _slots;
    private readonly int _preservedEntitySlotIndex;

    public SqlSetOperationPaddedProjectionExpression (Type type, IEnumerable<Expression> slots, int preservedEntitySlotIndex = -1)
    {
      ArgumentUtility.CheckNotNull (nameof(type), type);
      ArgumentUtility.CheckNotNull (nameof(slots), slots);

      _slots = slots.ToList().AsReadOnly();
      _type = type;
      _preservedEntitySlotIndex = preservedEntitySlotIndex;

      if (_preservedEntitySlotIndex >= _slots.Count)
        throw new ArgumentOutOfRangeException (nameof(preservedEntitySlotIndex));
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _type; }
    }

    public ReadOnlyCollection<Expression> Slots
    {
      get { return _slots; }
    }

    /// <summary>
    /// The index within <see cref="Slots"/> of the slot that must be preserved as-is for in-memory materialization purposes, or -1 if no slot
    /// needs special materialization treatment (always the case for combined-statement sides of a set operation).
    /// </summary>
    public int PreservedEntitySlotIndex
    {
      get { return _preservedEntitySlotIndex; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var newSlots = visitor.VisitAndConvert (_slots, "VisitChildren");
      if (newSlots != _slots)
        return new SqlSetOperationPaddedProjectionExpression (_type, newSlots, _preservedEntitySlotIndex);
      else
        return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var specificVisitor = visitor as ISqlSetOperationPaddedProjectionExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlSetOperationPaddedProjection (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("({0})", string.Join (", ", _slots));
    }
  }
}
