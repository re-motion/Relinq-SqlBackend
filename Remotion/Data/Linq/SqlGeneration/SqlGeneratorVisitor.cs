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
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class SqlGeneratorVisitor : QueryModelVisitorBase
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly DetailParserRegistries _detailParserRegistries;
    private readonly ParseContext _parseContext;

    public SqlGeneratorVisitor (
        IDatabaseInfo databaseInfo, ParseMode parseMode, DetailParserRegistries detailParserRegistries, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("parseContext", parseMode);
      ArgumentUtility.CheckNotNull ("detailParser", detailParserRegistries);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);

      _databaseInfo = databaseInfo;
      _detailParserRegistries = detailParserRegistries;
      _parseContext = parseContext;

      SqlGenerationData = new SqlGenerationData { ParseMode = parseMode };
    }

    public SqlGenerationData SqlGenerationData { get; private set; }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      SqlGenerationData.AddFromClause (_parseContext.JoinedTableContext.GetColumnSource (fromClause));
      base.VisitMainFromClause (fromClause, queryModel);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var columnSource = _parseContext.JoinedTableContext.GetColumnSource (fromClause);
      SqlGenerationData.AddFromClause (columnSource);

      // if the from clause contains a member expressions (e.g. from s1 in ... from s2 in s1.Friends), we'll parse the expression and add joins as needed
      var memberExpression = fromClause.FromExpression as MemberExpression;
      if (memberExpression != null)
      {
        var parser = _detailParserRegistries.WhereConditionParser.GetParser (memberExpression.Expression);
        var leftSide = parser.Parse (memberExpression.Expression, _parseContext);
        var foreignKeyName = DatabaseInfoUtility.GetJoinColumnNames (_databaseInfo, memberExpression.Member).B;
        var rightSide = new Column (columnSource, foreignKeyName);

        ICriterion criterion = new BinaryCondition (leftSide, rightSide, BinaryCondition.ConditionKind.Equal);
        SqlGenerationData.AddWhereClause (criterion, _parseContext.FieldDescriptors);
      }

      base.VisitAdditionalFromClause (fromClause, queryModel, index);
    }

    public override void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, FromClauseBase fromClause, int index)
    {
      throw new NotSupportedException ("Join clauses are not supported by this SQL generator.");
    }

    public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      ICriterion criterion = _detailParserRegistries.WhereConditionParser.GetParser (whereClause.Predicate).Parse (whereClause.Predicate, _parseContext);
      SqlGenerationData.AddWhereClause (criterion, _parseContext.FieldDescriptors);

      base.VisitWhereClause (whereClause, queryModel, index);
    }

    protected override void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      ArgumentUtility.CheckNotNull ("orderings", orderings);

      var fieldParser = new OrderingFieldParser (_databaseInfo);

      var orderingFields = new List<OrderingField> ();
      foreach (var ordering in orderings)
        orderingFields.Add (fieldParser.Parse (ordering.Expression, _parseContext, ordering.OrderingDirection));

      SqlGenerationData.PrependOrderingFields (orderingFields);

      base.VisitOrderings (orderings, queryModel, orderByClause);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      IEvaluation evaluation =
        _detailParserRegistries.SelectProjectionParser.GetParser (selectClause.Selector)
        .Parse (selectClause.Selector, _parseContext);
      SqlGenerationData.SetSelectEvaluation (evaluation, _parseContext.FieldDescriptors);

      base.VisitSelectClause (selectClause, queryModel);
    }

    protected override void VisitResultModifications (ObservableCollection<ResultModificationBase> resultModifications, QueryModel queryModel, SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("resultModifications", resultModifications);

      SqlGenerationData.ResultModifiers = new List<ResultModificationBase> (resultModifications);
      base.VisitResultModifications (resultModifications, queryModel, selectClause);
    }

    public override void VisitGroupClause (GroupClause groupClause, QueryModel queryModel)
    {
      throw new NotSupportedException ("Group clauses are not supported by this SQL generator.");
    }
  }
}
