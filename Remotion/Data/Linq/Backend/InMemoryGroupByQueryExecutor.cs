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
  /// Takes a <see cref="QueryModel"/> with a <see cref="GroupResultOperator"/>, executes an equivalent query with a <see cref="SelectClause"/> that fetches 
  /// all elements, and then performs the grouping in memory.
  /// </summary>
  [Obsolete ("TODO 1319")]
  public class InMemoryGroupByQueryExecutor
  {
    private static readonly MethodInfo s_executeCollectionInPlaceMethod = 
        typeof (InMemoryGroupByQueryExecutor).GetMethod ("ExecuteCollectionWithGroupingInPlace", BindingFlags.Public | BindingFlags.Instance);
    private static readonly MethodInfo s_executeScalarInMemoryMethod = typeof (ScalarResultOperatorBase).GetMethod ("ExecuteInMemory");

    private readonly IQueryExecutor _innerExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryGroupByQueryExecutor"/> class.
    /// </summary>
    /// <param name="innerExecutor">The inner executor to be used for fetching the elements before grouping them in-memory.</param>
    public InMemoryGroupByQueryExecutor (IQueryExecutor innerExecutor)
    {
      ArgumentUtility.CheckNotNull ("innerExecutor", innerExecutor);

      _innerExecutor = innerExecutor;
    }

    public IQueryExecutor InnerExecutor
    {
      get { return _innerExecutor; }
    }

    /// <summary>
    /// Executes an equivalent Select query for the given <paramref name="queryModel"/>, then groups the results in-memory. The query is executed
    /// without any <see cref="QueryModel.ResultOperators"/>; those are also executed in memory after the grouping has been performed. This method
    /// does support one scalar operator at the end of the query, before that, only non-scalar operators are supported. If <typeparamref name="T"/> 
    /// does not match the type of the scalar operator, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value to be returned. This must match the scalar result operator in the 
    /// <paramref name="queryModel"/>.</typeparam>
    /// <param name="queryModel">The query model to be executed.</param>
    /// <returns>A scalar value that represents the result of the scalar result operator executed after the in-memory grouping operation.</returns>
    public virtual T ExecuteScalarWithGrouping<T> (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      
      ScalarResultOperatorBase scalarOperator = null;
      if (queryModel.ResultOperators.Count > 0)
        scalarOperator = queryModel.ResultOperators[queryModel.ResultOperators.Count - 1] as ScalarResultOperatorBase;
      
      if (scalarOperator == null)
        throw new ArgumentException ("ExecuteScalarWithGrouping requires a scalar result operator at the end of the queryModel's result operators.", "queryModel");

      var newQueryModel = queryModel.Clone();
      newQueryModel.ResultOperators.RemoveAt (queryModel.ResultOperators.Count - 1);
      object groupings = InvokeExecuteCollectionWithGroupingInPlace (newQueryModel);

      var groupClause = GetGroupClause (queryModel);
      var groupingType = typeof (IGrouping<,>).MakeGenericType (groupClause.KeySelector.Type, groupClause.ElementSelector.Type);
      return (T) s_executeScalarInMemoryMethod.MakeGenericMethod (groupingType, typeof (T)).Invoke (scalarOperator, new [] { groupings });
    }

    /// <summary>
    /// Executes an equivalent Select query for the given <paramref name="queryModel"/>, then groups the results in-memory. The query is executed
    /// without any <see cref="QueryModel.ResultOperators"/>; those are also executed in memory after the grouping has been performed. This method
    /// does not support scalar operators, only non-scalar operators are supported. The type of the <see cref="IGrouping{TKey,TElement}"/> instances
    /// to be returned is inferred from the <paramref name="queryModel"/>'s <see cref="GroupResultOperator"/>. If <typeparamref name="T"/> does not match
    /// that type, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IGrouping{TKey,TElement}"/> instances to be returned. This must match the 
    /// <paramref name="queryModel"/>'s <see cref="GroupResultOperator.KeySelector"/> (TKey) and <see cref="GroupResultOperator.ElementSelector"/> (TElement).</typeparam>
    /// <param name="queryModel">The query model to be executed.</param>
    /// <returns>An enumerable iterating over the <see cref="IGrouping{TKey,TElement}"/> instances that represent the result of the in-memory
    /// grouping operation.</returns>
    public virtual IEnumerable<T> ExecuteCollectionWithGrouping<T> (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var groupClause = GetGroupClause (queryModel);
      var keyType = groupClause.KeySelector.Type;
      var elementType = groupClause.ElementSelector.Type;

      var groupings = InvokeExecuteCollectionWithGroupingInPlace (queryModel.Clone());

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

    /// <summary>
    /// Executes an equivalent Select query for the given <paramref name="queryModel"/>, then groups the results in-memory. The query is executed
    /// without any <see cref="QueryModel.ResultOperators"/>; those are also executed in memory after the grouping has been performed. This method
    /// does not support scalar operators, only non-scalar operators are supported.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the <see cref="IGrouping{TKey,TElement}"/> to be returned.</typeparam>
    /// <typeparam name="TElement">The type of the elements in the <see cref="IGrouping{TKey,TElement}"/> to be returned.</typeparam>
    /// <param name="queryModel">The query model to execute.</param>
    /// <returns>An enumerable iterating over the results of the in-memory grouping operation.</returns>
    public virtual IEnumerable<IGrouping<TKey, TElement>> ExecuteCollectionWithGrouping<TKey, TElement> (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      return ExecuteCollectionWithGroupingInPlace<TKey, TElement> (queryModel.Clone ());
    }

    /// <summary>
    /// Executes an equivalent Select query for the given <paramref name="queryModel"/>, then groups the results in-memory. The query is executed
    /// without any <see cref="QueryModel.ResultOperators"/>; those are also executed in memory after the grouping has been performed. This method
    /// does not support scalar operators, only non-scalar operators are supported. This method may modify the given <paramref name="queryModel"/>.
    /// Call <see cref="ExecuteCollectionWithGrouping{TKey,TElement}"/> to avoid modifying the supplied <paramref name="queryModel"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the <see cref="IGrouping{TKey,TElement}"/> to be returned.</typeparam>
    /// <typeparam name="TElement">The type of the elements in the <see cref="IGrouping{TKey,TElement}"/> to be returned.</typeparam>
    /// <param name="queryModel">The query model to execute.</param>
    /// <returns>An enumerable iterating over the results of the in-memory grouping operation.</returns>
    public virtual IEnumerable<IGrouping<TKey, TElement>> ExecuteCollectionWithGroupingInPlace<TKey, TElement> (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      GroupResultOperator groupResultOperator = GetGroupClause (queryModel);

      var tupleConstructor = typeof (Tuple<,>)
          .MakeGenericType (typeof (TKey), typeof (TElement))
          .GetConstructor (new[] { typeof (TKey), typeof (TElement) });
      var newExpression = Expression.New (tupleConstructor, groupResultOperator.KeySelector, groupResultOperator.ElementSelector);

      queryModel.SelectClause = new SelectClause (newExpression);
      var resultOperators = new List<ResultOperatorBase> (queryModel.ResultOperators);
      queryModel.ResultOperators.Clear ();
      var collection = _innerExecutor.ExecuteCollection<Tuple<TKey, TElement>> (queryModel, new FetchRequestBase[0]);

      var groupings = from tuple in collection
                      group tuple.B by tuple.A;

      foreach (var resultOperator in resultOperators)
        groupings = GetNonScalarResultOperator (resultOperator).ExecuteInMemory (groupings);

      return groupings;
    }

    protected GroupResultOperator GetGroupClause (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var groupClause = queryModel.ResultOperators[0] as GroupResultOperator; // TODO 1319: This does not work.
      if (groupClause == null)
        throw new ArgumentException ("InMemoryGroupByQueryExecutor requires a GroupResultOperator in the query model.", "queryModel");
      return groupClause;
    }

    private NonScalarResultOperatorBase GetNonScalarResultOperator (ResultOperatorBase resultOperator)
    {
      var nonScalarResultOperator = resultOperator as NonScalarResultOperatorBase;
      if (nonScalarResultOperator == null)
      {
        var message = string.Format (
            "ExecuteCollectionWithGrouping only supports non-scalar result operators, found a '{0}'.", 
            resultOperator.GetType().Name);
        throw new NotSupportedException (message);
      }
      return nonScalarResultOperator;
    }

    private object InvokeExecuteCollectionWithGroupingInPlace (QueryModel queryModel)
    {
      var groupClause = GetGroupClause (queryModel);
      var keyType = groupClause.KeySelector.Type;
      var elementType = groupClause.ElementSelector.Type;
      return s_executeCollectionInPlaceMethod.MakeGenericMethod (keyType, elementType).Invoke (this, new object[] { queryModel });
    }
  }
}