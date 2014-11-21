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
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
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
    private readonly Dictionary<UnresolvedCollectionJoinTableInfo, SqlEntityExpression> _unresolvedCollectionJoinTableInfoToOriginatingEntityMapping;

    public MappingResolutionContext ()
    {
      _entityMapping = new Dictionary<SqlEntityExpression, SqlTableBase>();
      _groupReferenceMapping = new Dictionary<SqlGroupingSelectExpression, SqlTableBase>();
      _unresolvedCollectionJoinTableInfoToOriginatingEntityMapping = new Dictionary<UnresolvedCollectionJoinTableInfo, SqlEntityExpression>();
    }

    public void AddSqlEntityMapping (SqlEntityExpression entityExpression, SqlTableBase sqlTable)
    {
      ArgumentUtility.CheckNotNull ("entityExpression", entityExpression);
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      _entityMapping[entityExpression] = sqlTable;
    }

    public void AddGroupReferenceMapping (SqlGroupingSelectExpression groupingSelectExpression, SqlTableBase sqlTable)
    {
      ArgumentUtility.CheckNotNull ("groupingSelectExpression", groupingSelectExpression);
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      _groupReferenceMapping[groupingSelectExpression] = sqlTable;
    }

    public SqlTableBase GetSqlTableForEntityExpression (SqlEntityExpression entityExpression)
    {
      ArgumentUtility.CheckNotNull ("entityExpression", entityExpression);

      SqlTableBase result;
      if (_entityMapping.TryGetValue (entityExpression, out result))
        return result;

      var message = string.Format ("No associated table found for entity '{0}'.", entityExpression);
      throw new InvalidOperationException (message);
    }

    public SqlTableBase GetReferencedGroupSource (SqlGroupingSelectExpression groupingSelectExpression)
    {
      ArgumentUtility.CheckNotNull ("groupingSelectExpression", groupingSelectExpression);
      SqlTableBase result;
      if (_groupReferenceMapping.TryGetValue (groupingSelectExpression, out result))
        return result;

      var message = string.Format ("No associated table found for grouping select expression '{0}'.", groupingSelectExpression);
      throw new InvalidOperationException (message);
    }

    public SqlEntityExpression UpdateEntityAndAddMapping (SqlEntityExpression entityExpression, Type itemType, string tableAlias, string newName)
    {
      ArgumentUtility.CheckNotNull ("entityExpression", entityExpression);
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNullOrEmpty ("tableAlias", tableAlias);

      var newEntityExpression = entityExpression.Update (itemType, tableAlias, newName);
      var tableForEntityExpression = GetSqlTableForEntityExpression (entityExpression);
      AddSqlEntityMapping (newEntityExpression, tableForEntityExpression);
      return newEntityExpression;
    }

    public SqlGroupingSelectExpression UpdateGroupingSelectAndAddMapping (
        SqlGroupingSelectExpression expression, Expression newKey, Expression newElement, IEnumerable<Expression> aggregations)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("newKey", newKey);
      ArgumentUtility.CheckNotNull ("newElement", newElement);
      ArgumentUtility.CheckNotNull ("aggregations", aggregations);

      var newSqlGroupingSelectExpression = expression.Update (newKey, newElement, aggregations);
      SqlTableBase tableForGroupingSelectExpression; 
      if(_groupReferenceMapping.TryGetValue(expression, out tableForGroupingSelectExpression))
        AddGroupReferenceMapping (newSqlGroupingSelectExpression, tableForGroupingSelectExpression);
      return newSqlGroupingSelectExpression;
    }

    public void AddSqlTable (SqlTable sqlTable, SqlStatementBuilder sqlStatementBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);

      sqlStatementBuilder.SqlTables.Add (sqlTable);
    }

    public Expression RemoveNamesAndUpdateMapping (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      while (expression is NamedExpression)
        expression = ((NamedExpression) expression).Expression;

      if (expression is SqlEntityExpression)
      {
        var sqlEntityExpression = (SqlEntityExpression) expression;
        expression = UpdateEntityAndAddMapping (sqlEntityExpression, sqlEntityExpression.Type, sqlEntityExpression.TableAlias, null);
      }

      return expression;
    }

    // This mapping is used to store the intermediate result from resolving an UnresolvedCollectionJoinTableInfo for sharing it between
    // ResolvingTableInfoVisitor and ResolvingExpressionVisitor.
    public void AddOriginatingEntityMappingForUnresolvedCollectionJoinTableInfo (
        UnresolvedCollectionJoinTableInfo unresolvedCollectionJoinTableInfo,
        SqlEntityExpression resolvedOriginatingEntity)
    {
      ArgumentUtility.CheckNotNull ("unresolvedCollectionJoinTableInfo", unresolvedCollectionJoinTableInfo);
      ArgumentUtility.CheckNotNull ("resolvedOriginatingEntity", resolvedOriginatingEntity);

      _unresolvedCollectionJoinTableInfoToOriginatingEntityMapping[unresolvedCollectionJoinTableInfo] = resolvedOriginatingEntity;
    }

    public SqlEntityExpression GetOriginatingEntityForUnresolvedCollectionJoinTableInfo (
        UnresolvedCollectionJoinTableInfo unresolvedCollectionJoinTableInfo)
    {
      ArgumentUtility.CheckNotNull ("unresolvedCollectionJoinTableInfo", unresolvedCollectionJoinTableInfo);
      try
      {
        return _unresolvedCollectionJoinTableInfoToOriginatingEntityMapping[unresolvedCollectionJoinTableInfo];
      }
      catch (KeyNotFoundException ex)
      {
        var message = "An originating entity for the giben UnresolvedCollectionJoinTableInfo has not been registered. Make sure the "
            + "UnresolvedCollectionJoinTableInfo is resolved before the referencing UnresolvedCollectionJoinConditionExpression is.";
        throw new KeyNotFoundException (message, ex);
      }
    }
  }
}