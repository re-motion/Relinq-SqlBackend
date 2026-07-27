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
  /// One slot may be flagged as the "preserved" slot via cccc: for the outermost statement of a query, this
  /// must be the original whole-entity expression that is actually materialized into an object, so that the outer select-expression visitor can
  /// keep building its in-memory projection solely from that slot, while still rendering (and advancing column positions for) the extra padding
  /// slots. Combined-statement sides of a set operation are never materialized directly, so they never set this index.
  /// </summary>
  public class SqlSetOperationPaddedProjectionExpression : SqlEntityExpression
  {
    private readonly SqlEntityExpression _innerExpression;
    private readonly Type _type;
    private readonly ReadOnlyCollection<Expression> _slots;
    private readonly bool _isPrimaryExpression;

    public SqlSetOperationPaddedProjectionExpression (SqlEntityExpression innerExpression, Type type, IEnumerable<Expression> slots, bool isPrimaryExpression)
        : base(type, innerExpression.TableAlias, innerExpression.Name, innerExpression.IdentityExpressionGenerator)
    {
      ArgumentUtility.CheckNotNull (nameof(innerExpression), innerExpression);
      ArgumentUtility.CheckNotNull (nameof(type), type);
      ArgumentUtility.CheckNotNull (nameof(slots), slots);

      _innerExpression = innerExpression;
      _type = type;
      _slots = slots.ToList().AsReadOnly();
      _isPrimaryExpression = isPrimaryExpression;
    }

    public override ReadOnlyCollection<SqlColumnExpression> Columns
    {
        get { return _innerExpression.Columns; }
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public SqlEntityExpression InnerExpression
    {
        get { return _innerExpression; }
    }

    public override Type Type
    {
        get { return _type; }
    }

    public IReadOnlyList<Expression> Slots
    {
        get { return _slots; }
    }

    public bool IsPrimaryExpression
    {
      get { return _isPrimaryExpression; }
    }

    public override SqlColumnExpression GetColumn (Type type, string columnName, bool isPrimaryKeyColumn)
    {
        return _innerExpression.GetColumn(type, columnName, isPrimaryKeyColumn);
    }

    public override SqlEntityExpression CreateReference (string newTableAlias, Type newType)
    {
        return _innerExpression.CreateReference(newTableAlias, newType);
    }

    public override SqlEntityExpression Update (Type itemType, string tableAlias, string entityName)
    {
        return _innerExpression.Update(itemType, tableAlias, entityName);
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var newSlots = visitor.VisitAndConvert (_slots, "VisitChildren");
      if (newSlots != _slots)
        return new SqlSetOperationPaddedProjectionExpression (_innerExpression, _type, newSlots, _isPrimaryExpression);
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
