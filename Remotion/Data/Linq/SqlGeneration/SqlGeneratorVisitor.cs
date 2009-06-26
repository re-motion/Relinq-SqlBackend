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

    public override void VisitMainFromClause (MainFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      SqlGenerationData.AddFromClause (fromClause.GetColumnSource (_databaseInfo));
      base.VisitMainFromClause (fromClause);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      SqlGenerationData.AddFromClause (fromClause.GetColumnSource (_databaseInfo));
      base.VisitAdditionalFromClause (fromClause);
    }

    public override void VisitMemberFromClause (MemberFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      SqlGenerationData.AddFromClause (fromClause.GetColumnSource (_databaseInfo));

      var memberExpression = fromClause.MemberExpression;
      var leftSide = _detailParserRegistries.WhereConditionParser.GetParser (memberExpression.Expression).Parse (memberExpression.Expression, _parseContext);
      var foreignKeyName = DatabaseInfoUtility.GetJoinColumnNames (_databaseInfo, memberExpression.Member).B;
      var rightSide = new Column (fromClause.GetColumnSource (_databaseInfo), foreignKeyName);

      ICriterion criterion = new BinaryCondition (leftSide, rightSide, BinaryCondition.ConditionKind.Equal);
      SqlGenerationData.AddWhereClause (criterion, _parseContext.FieldDescriptors);
      base.VisitMemberFromClause (fromClause);
    }

    public override void VisitSubQueryFromClause (SubQueryFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      SqlGenerationData.AddFromClause (fromClause.GetColumnSource (_databaseInfo));
      base.VisitSubQueryFromClause (fromClause);
    }

    public override void VisitJoinClause (JoinClause joinClause)
    {
      throw new NotSupportedException ("Join clauses are not supported by this SQL generator.");
    }

    public override void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);

      ICriterion criterion = _detailParserRegistries.WhereConditionParser.GetParser (whereClause.Predicate).Parse (whereClause.Predicate, _parseContext);
      SqlGenerationData.AddWhereClause (criterion, _parseContext.FieldDescriptors);

      base.VisitWhereClause (whereClause);
    }

    protected override void VisitOrderings (OrderByClause orderByClause, IList<Ordering> orderings)
    {
      var fieldParser = new OrderingFieldParser (_databaseInfo);

      var orderingFields = new List<OrderingField> ();
      foreach (var ordering in orderings)
        orderingFields.Add (fieldParser.Parse (ordering.Expression, _parseContext, ordering.OrderingDirection));

      SqlGenerationData.PrependOrderingFields (orderingFields);

      base.VisitOrderings (orderByClause, orderings);
    }

    public override void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      IEvaluation evaluation =
        _detailParserRegistries.SelectProjectionParser.GetParser (selectClause.Selector)
        .Parse (selectClause.Selector, _parseContext);
      SqlGenerationData.SetSelectEvaluation (evaluation, _parseContext.FieldDescriptors);

      base.VisitSelectClause (selectClause);
    }

    protected override void VisitResultModifications (SelectClause selectClause, IList<ResultModificationBase> resultModifications)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("resultModifications", resultModifications);

      SqlGenerationData.ResultModifiers = new List<ResultModificationBase> (resultModifications);
      base.VisitResultModifications (selectClause, resultModifications);
    }

    public override void VisitGroupClause (GroupClause groupClause)
    {
      throw new NotSupportedException ("Group clauses are not supported by this SQL generator.");
    }
  }
}
