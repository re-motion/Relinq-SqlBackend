/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class SqlGeneratorVisitor : IQueryVisitor
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly DetailParserRegistries _detailParserRegistries;
    private readonly ParseContext _parseContext;

    private bool _secondOrderByClause;

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

      _secondOrderByClause = false;

      SqlGenerationData = new SqlGenerationData { ParseMode = parseMode };
    }

    public SqlGenerationData SqlGenerationData { get; private set; }

    public void VisitQueryModel (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      queryModel.MainFromClause.Accept (this);
      foreach (IBodyClause bodyClause in queryModel.BodyClauses)
        bodyClause.Accept (this);

      queryModel.SelectOrGroupClause.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      VisitFromClause (fromClause);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      VisitFromClause (fromClause);
    }

    public void VisitSubQueryFromClause (SubQueryFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      VisitFromClause (fromClause);
    }

    private void VisitFromClause (FromClauseBase fromClause)
    {
      IColumnSource columnSource = fromClause.GetFromSource (_databaseInfo);

      SqlGenerationData.AddFromClause (columnSource);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
      throw new NotImplementedException();
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);

      LambdaExpression boolExpression = whereClause.GetSimplifiedBoolExpression();
      ICriterion criterion = _detailParserRegistries.WhereConditionParser.GetParser (boolExpression.Body).Parse (boolExpression.Body, _parseContext);

      SqlGenerationData.AddWhereClause (criterion, _parseContext.FieldDescriptors);
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);

      for (int i = 0; i < orderByClause.OrderingList.Count; i++)
      {
        OrderingClause clause = orderByClause.OrderingList[i];
        clause.Accept (this);
        if (i == (orderByClause.OrderingList.Count - 1))
          _secondOrderByClause = true;
      }
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      ArgumentUtility.CheckNotNull ("orderingClause", orderingClause);
      var fieldParser = new OrderingFieldParser (_databaseInfo);
      OrderingField orderingField = fieldParser.Parse (orderingClause.Expression.Body, _parseContext, orderingClause.OrderDirection);

      if (!_secondOrderByClause)
        SqlGenerationData.AddOrderingFields (orderingField);
      else
        SqlGenerationData.AddFirstOrderingFields (orderingField);
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      Expression projectionBody =
          selectClause.ProjectionExpression != null ? selectClause.ProjectionExpression.Body : _parseContext.QueryModel.MainFromClause.Identifier;

      List<MethodCall> methodCalls = GetMethodCalls(selectClause);

      IEvaluation evaluation =
          _detailParserRegistries.SelectProjectionParser.GetParser (projectionBody).Parse (projectionBody, _parseContext);

      SetSelectClause (methodCalls, evaluation);
    }

    private List<MethodCall> GetMethodCalls (SelectClause selectClause)
    {
      List<MethodCall> methodCalls = new List<MethodCall>();
      ResultModifierParser parser = new ResultModifierParser (_detailParserRegistries.SelectProjectionParser);
      if (selectClause.ResultModifiers != null)
      {
        foreach (var method in selectClause.ResultModifiers)
          methodCalls.Add (parser.Parse (method, _parseContext));
      }
      return methodCalls;
    }

    private void SetSelectClause (List<MethodCall> methodCalls, IEvaluation evaluation)
    {
      if (methodCalls.Count != 0)
        SqlGenerationData.SetSelectClause (methodCalls, _parseContext.FieldDescriptors, evaluation);
      else
        SqlGenerationData.SetSelectClause (null, _parseContext.FieldDescriptors, evaluation);
    }

    public void VisitLetClause (LetClause letClause)
    {
      ArgumentUtility.CheckNotNull ("letClause", letClause);
      Expression projectionBody = letClause.Expression;

      IEvaluation evaluation =
          _detailParserRegistries.SelectProjectionParser.GetParser (projectionBody).Parse (projectionBody, _parseContext);


      LetData letData = new LetData (evaluation, letClause.Identifier.Name, letClause.GetColumnSource (_databaseInfo));
      SqlGenerationData.AddLetClause (letData, _parseContext.FieldDescriptors);
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      throw new NotImplementedException();
    }
  }
}