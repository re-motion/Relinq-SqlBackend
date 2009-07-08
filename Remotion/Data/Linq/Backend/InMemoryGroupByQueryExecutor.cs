// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Collections.Generic;
using System.Linq;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Utilities;
using System.Linq.Expressions;
using System.Reflection;

namespace Remotion.Data.Linq.Backend
{
  /// <summary>
  /// Takes a <see cref="QueryModel"/> with a <see cref="GroupClause"/>, executes an equivalent query that fetches all elements, and then
  /// performs the grouping in memory.
  /// </summary>
  public class InMemoryGroupByQueryExecutor
  {
    private static readonly MethodInfo s_executeCollectionMethod = GetExecuteCollectionMethod();

    private static MethodInfo GetExecuteCollectionMethod ()
    {
      return typeof (InMemoryGroupByQueryExecutor)
          .GetMethods ()
          .Where (m => m.Name == "ExecuteCollectionWithGrouping" && m.GetGenericArguments ().Length == 2)
          .Single ();
    }

    private readonly IQueryExecutor _innerExecutor;

    public InMemoryGroupByQueryExecutor (IQueryExecutor innerExecutor)
    {
      ArgumentUtility.CheckNotNull ("innerExecutor", innerExecutor);

      _innerExecutor = innerExecutor;
    }

    public IEnumerable<T> ExecuteCollectionWithGrouping<T> (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var groupClause = GetGroupClause (queryModel);
      var keyType = groupClause.ByExpression.Type;
      var elementType = groupClause.GroupExpression.Type;

      var groupings = s_executeCollectionMethod.MakeGenericMethod (keyType, elementType).Invoke (this, new object[] { queryModel });

      var castGroupings = groupings as IEnumerable<T>;
      if (castGroupings == null)
      {
        string message = string.Format (
            "The query model returns groupings of type 'IGrouping<{0}, {1}>', but '{2}' was requested.", 
            keyType,
            elementType,
            typeof (T).FullName);
        throw new ArgumentTypeException (message, "T", typeof (IGrouping<,>).MakeGenericType (keyType, elementType), typeof (T));
      }

      return castGroupings;
      
    }

    public IEnumerable<IGrouping<TKey, TElement>> ExecuteCollectionWithGrouping<TKey, TElement> (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var newQueryModel = queryModel.Clone ();
      GroupClause groupClause = GetGroupClause (newQueryModel);

      var tupleConstructor = typeof (Tuple<,>)
          .MakeGenericType (typeof (TKey), typeof (TElement))
          .GetConstructor (new[] { typeof (TKey), typeof (TElement) });
      var newExpression = Expression.New (tupleConstructor, groupClause.ByExpression, groupClause.GroupExpression);

      newQueryModel.SelectOrGroupClause = new SelectClause (newExpression);
      newQueryModel.ResultOperators.Clear ();
      var collection = _innerExecutor.ExecuteCollection<Tuple<TKey, TElement>> (newQueryModel, new FetchRequestBase[0]);

      var groupings = from tuple in collection
                      group tuple.B by tuple.A;

      foreach (var resultOperator in queryModel.ResultOperators)
      {
        groupings = GetNonScalarResultOperator (resultOperator).ExecuteInMemory (groupings);
      }

      return groupings;
    }

    private GroupClause GetGroupClause (QueryModel queryModel)
    {
      var groupClause = queryModel.SelectOrGroupClause as GroupClause;
      if (groupClause == null)
        throw new ArgumentException ("InMemoryGroupByQueryExecutor requires a GroupClause in the query model.", "queryModel");
      return groupClause;
    }

    private NonScalarResultOperatorBase GetNonScalarResultOperator (ResultOperatorBase resultOperator)
    {
      var nonScalarResultOperator = resultOperator as NonScalarResultOperatorBase;
      if (nonScalarResultOperator == null)
      {
        var message = string.Format (
            "ExecuteCollectionWithGrouping does not support scalar result operators, found a '{0}'.", 
            resultOperator.GetType().Name);
        throw new NotSupportedException (message);
      }
      return nonScalarResultOperator;
    }
  }
}