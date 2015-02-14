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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationQueryModelVisitor"/> generates a <see cref="SqlStatement"/> from a query model.
  /// </summary>
  public class SqlPreparationQueryModelVisitor : QueryModelVisitorBase
  {
    public static SqlStatement TransformQueryModel (
        QueryModel queryModel,
        ISqlPreparationContext parentPreparationContext,
        ISqlPreparationStage stage,
        UniqueIdentifierGenerator generator,
        ResultOperatorHandlerRegistry resultOperatorHandlerRegistry)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("resultOperatorHandlerRegistry", resultOperatorHandlerRegistry);

      var visitor = new SqlPreparationQueryModelVisitor (parentPreparationContext, stage, generator, resultOperatorHandlerRegistry);
      queryModel.Accept (visitor);

      return visitor.GetSqlStatement();
    }

    private readonly ISqlPreparationContext _context;
    private readonly ISqlPreparationStage _stage;

    private readonly SqlStatementBuilder _sqlStatementBuilder;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly ResultOperatorHandlerRegistry _resultOperatorHandlerRegistry;

    protected SqlPreparationQueryModelVisitor (
        ISqlPreparationContext parentContext,
        ISqlPreparationStage stage,
        UniqueIdentifierGenerator generator,
        ResultOperatorHandlerRegistry resultOperatorHandlerRegistry)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("resultOperatorHandlerRegistry", resultOperatorHandlerRegistry);

      _stage = stage;
      _generator = generator;
      _resultOperatorHandlerRegistry = resultOperatorHandlerRegistry;

      _sqlStatementBuilder = new SqlStatementBuilder();
      _context = new SqlPreparationContext (parentContext, _sqlStatementBuilder);
    }

    public ISqlPreparationContext Context
    {
      get { return _context; }
    }

    protected ISqlPreparationStage Stage
    {
      get { return _stage; }
    }

    protected SqlStatementBuilder SqlStatementBuilder
    {
      get { return _sqlStatementBuilder; }
    }

    public SqlStatement GetSqlStatement ()
    {
      return SqlStatementBuilder.GetSqlStatement();
    }

    public override void VisitQueryModel (QueryModel queryModel)
    {
      var constantCollection = GetConstantCollectionValue (queryModel);
      if (constantCollection != null)
      {
        // If the query is a constant collection, transform it to a trivial SqlStatement with only a select projection. In the SQL generation, this
        // will become something like (1, 2, 3, 4) - used primarily for IN expressions.
        // In this specific case, the select projection is not named because such a list of values cannot be named in SQL.
        SqlStatementBuilder.SelectProjection = Expression.Constant (constantCollection);
        SqlStatementBuilder.DataInfo = queryModel.SelectClause.GetOutputDataInfo ();
        VisitResultOperators (queryModel.ResultOperators, queryModel);
      }
      else
      {
        base.VisitQueryModel (queryModel);
      }

      // Always name the select projection - null indicates the default name that will later be removed if the inner expression already has a name.
      // The name is required to be able to access the result from the executed SQL afterwards. The resolution stage will consolidate names around
      // NewExpressions, entities, more than one name, etc.
      SqlStatementBuilder.SelectProjection = new NamedExpression (null, SqlStatementBuilder.SelectProjection);

      // We get the DataInfo incrementally when we handle the SelectClause and ResultOperators, so we need to manually adjust the data type if 
      // required. (We can't simply call queryModel.GetOutputDataInfo() because some of the result operator handlers might have changed the 
      // SqlStatementBuilder.DataInfo.)
      if (queryModel.ResultTypeOverride != null)
        SqlStatementBuilder.DataInfo = SqlStatementBuilder.DataInfo.AdjustDataType (queryModel.ResultTypeOverride);
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      AddQuerySource (fromClause, fromClause.FromExpression);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      AddQuerySource (fromClause, fromClause.FromExpression);
    }

    public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var translatedExpression = _stage.PrepareWhereExpression (whereClause.Predicate, _context);
      SqlStatementBuilder.AddWhereCondition (translatedExpression);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var preparedExpression = _stage.PrepareSelectExpression (selectClause.Selector, _context);

      SqlStatementBuilder.SelectProjection = preparedExpression;
      SqlStatementBuilder.DataInfo = selectClause.GetOutputDataInfo();
    }

    public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var orderings = from ordering in orderByClause.Orderings
                      let orderByExpression = _stage.PrepareOrderByExpression (ordering.Expression, _context)
                      select new Ordering (orderByExpression, ordering.OrderingDirection);
      SqlStatementBuilder.Orderings.InsertRange (0, orderings);
    }

    public override void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      AddJoinClause (joinClause);
    }

    public SqlTable AddJoinClause (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);

      var table = AddQuerySource (joinClause, joinClause.InnerSequence);

      var whereCondition = Expression.Equal (joinClause.OuterKeySelector, joinClause.InnerKeySelector);
      SqlStatementBuilder.AddWhereCondition (_stage.PrepareWhereExpression (whereCondition, _context));

      return table;
    }

    public override void VisitGroupJoinClause (GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
    {
      //the joins for the group join clauses ared added in SqlPreparationContextVisitor on demand
    }

    public override void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var operatorType = resultOperator.GetType();
      
      var resultOperatorHandler = _resultOperatorHandlerRegistry.GetItem (operatorType);
      if (resultOperatorHandler == null)
      {
        string message = string.Format (
            "The result operator '{0}' is not supported and no custom handler has been registered.",
            operatorType.Name);
        throw new NotSupportedException (message);
      }

      resultOperatorHandler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);
    }

    // TODO RMLNQSQL-78: Remove return values of these methods.
    public SqlTable AddQuerySource (IQuerySource source, Expression fromExpression)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);

      var fromExpressionInfo = _stage.PrepareFromExpression (
          fromExpression,
          _context,
          OrderingExtractionPolicy.ExtractOrderingsIntoProjection);
      AddPreparedFromExpression (fromExpressionInfo);  
      
      _context.AddExpressionMapping (new QuerySourceReferenceExpression (source), fromExpressionInfo.ItemSelector);
      return fromExpressionInfo.AppendedTable.SqlTable;
    }

    public void AddPreparedFromExpression (FromExpressionInfo fromExpressionInfo)
    {
      ArgumentUtility.CheckNotNull ("fromExpressionInfo", fromExpressionInfo);

      if (fromExpressionInfo.WhereCondition != null)
        SqlStatementBuilder.AddWhereCondition (fromExpressionInfo.WhereCondition);

      foreach (var ordering in fromExpressionInfo.ExtractedOrderings)
        SqlStatementBuilder.Orderings.Add (ordering);

      SqlStatementBuilder.SqlTables.Add (fromExpressionInfo.AppendedTable);
    }

    private ICollection GetConstantCollectionValue (QueryModel queryModel)
    {
      var fromExpressionAsConstant = (queryModel.MainFromClause.FromExpression) as ConstantExpression;
      if (queryModel.IsIdentityQuery () && fromExpressionAsConstant != null)
      {
        if (fromExpressionAsConstant.Value is ICollection)
          return (ICollection) fromExpressionAsConstant.Value;
        
        if (fromExpressionAsConstant.Value == null)
          throw new NotSupportedException ("Data sources cannot be null.");
      }

      return null;
    }
  }
}