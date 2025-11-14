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
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="MappingResolutionContext"/> holds context information required during mapping resolution stage.
  /// </summary>
  public class MappingResolutionContext : IMappingResolutionContext
  {
    private readonly Dictionary<SqlEntityExpression, SqlTableBase> _entityMapping;
    private readonly Dictionary<SqlGroupingSelectExpression, SqlTableBase> _groupReferenceMapping;

    public MappingResolutionContext ()
    {
      _entityMapping = new Dictionary<SqlEntityExpression, SqlTableBase>();
      _groupReferenceMapping = new Dictionary<SqlGroupingSelectExpression, SqlTableBase>();
    }

    public void AddSqlEntityMapping (SqlEntityExpression entityExpression, SqlTableBase sqlTable)
    {
      ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);
      ArgumentUtility.CheckNotNull (nameof(sqlTable), sqlTable);

      _entityMapping[entityExpression] = sqlTable;
    }

    public void AddGroupReferenceMapping (SqlGroupingSelectExpression groupingSelectExpression, SqlTableBase sqlTable)
    {
      ArgumentUtility.CheckNotNull (nameof(groupingSelectExpression), groupingSelectExpression);
      ArgumentUtility.CheckNotNull (nameof(sqlTable), sqlTable);

      _groupReferenceMapping[groupingSelectExpression] = sqlTable;
    }

    public SqlTableBase GetSqlTableForEntityExpression (SqlEntityExpression entityExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

      SqlTableBase result;
      if (_entityMapping.TryGetValue (entityExpression, out result))
        return result;

      var message = string.Format ("No associated table found for entity '{0}'.", entityExpression);
      throw new InvalidOperationException (message);
    }

    public SqlTableBase GetReferencedGroupSource (SqlGroupingSelectExpression groupingSelectExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(groupingSelectExpression), groupingSelectExpression);
      SqlTableBase result;
      if (_groupReferenceMapping.TryGetValue (groupingSelectExpression, out result))
        return result;

      var message = string.Format ("No associated table found for grouping select expression '{0}'.", groupingSelectExpression);
      throw new InvalidOperationException (message);
    }

    public SqlEntityExpression UpdateEntityAndAddMapping (SqlEntityExpression entityExpression, Type itemType, string tableAlias, string newName)
    {
      ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);
      ArgumentUtility.CheckNotNull (nameof(itemType), itemType);
      ArgumentUtility.CheckNotNullOrEmpty (nameof(tableAlias), tableAlias);

      var newEntityExpression = entityExpression.Update (itemType, tableAlias, newName);
      var tableForEntityExpression = GetSqlTableForEntityExpression (entityExpression);
      AddSqlEntityMapping (newEntityExpression, tableForEntityExpression);
      return newEntityExpression;
    }

    public SqlGroupingSelectExpression UpdateGroupingSelectAndAddMapping (
        SqlGroupingSelectExpression expression, Expression newKey, Expression newElement, IEnumerable<Expression> aggregations)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);
      ArgumentUtility.CheckNotNull (nameof(newKey), newKey);
      ArgumentUtility.CheckNotNull (nameof(newElement), newElement);
      ArgumentUtility.CheckNotNull (nameof(aggregations), aggregations);

      var newSqlGroupingSelectExpression = expression.Update (newKey, newElement, aggregations);
      SqlTableBase tableForGroupingSelectExpression; 
      if(_groupReferenceMapping.TryGetValue(expression, out tableForGroupingSelectExpression))
        AddGroupReferenceMapping (newSqlGroupingSelectExpression, tableForGroupingSelectExpression);
      return newSqlGroupingSelectExpression;
    }

    public void AddSqlTable (SqlTable sqlTable, SqlStatementBuilder sqlStatementBuilder)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlTable), sqlTable);
      ArgumentUtility.CheckNotNull (nameof(sqlStatementBuilder), sqlStatementBuilder);

      sqlStatementBuilder.SqlTables.Add (sqlTable);
    }

    public Expression RemoveNamesAndUpdateMapping (Expression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      while (expression is NamedExpression)
        expression = ((NamedExpression) expression).Expression;

      if (expression is SqlEntityExpression)
      {
        var sqlEntityExpression = (SqlEntityExpression) expression;
        expression = UpdateEntityAndAddMapping (sqlEntityExpression, sqlEntityExpression.Type, sqlEntityExpression.TableAlias, null);
      }

      return expression;
    }
  }
}