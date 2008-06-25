using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class SqlGeneratorVisitor : IQueryVisitor
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly DetailParser _detailParser;
    private readonly ParseContext _parseContext;
    
    public SqlGeneratorVisitor (IDatabaseInfo databaseInfo, ParseMode parseMode, DetailParser detailParser, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("parseContext", parseMode);
      ArgumentUtility.CheckNotNull ("detailParser", detailParser);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      
      _databaseInfo = databaseInfo;
      _detailParser = detailParser;
      _parseContext = parseContext;

      SqlGenerationData = new SqlGenerationData {ParseMode = parseMode};
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
      VisitFromClause(fromClause);
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

      LambdaExpression boolExpression = whereClause.GetSimplifiedBoolExpression ();
      ICriterion criterion = _detailParser.WhereConditionParser.GetParser (boolExpression.Body).Parse (boolExpression.Body, _parseContext);

      SqlGenerationData.AddWhereClause (criterion, _parseContext.FieldDescriptors);
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      foreach (OrderingClause clause in orderByClause.OrderingList)
        clause.Accept (this);
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      ArgumentUtility.CheckNotNull ("orderingClause", orderingClause);
      var fieldParser = new OrderingFieldParser (_parseContext.QueryModel, orderingClause, _databaseInfo, _parseContext.JoinedTableContext);
      OrderingField orderingField = fieldParser.GetField();

      SqlGenerationData.AddOrderingFields(orderingField);
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      Expression projectionBody = selectClause.ProjectionExpression != null ? selectClause.ProjectionExpression.Body : _parseContext.QueryModel.MainFromClause.Identifier;
      
      List<IEvaluation> listEvaluations = 
        _detailParser.SelectProjectionParser.GetParser (projectionBody).Parse (projectionBody, _parseContext);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> evaluations =
        new Tuple<List<FieldDescriptor>, List<IEvaluation>> (_parseContext.FieldDescriptors, listEvaluations);
      
      SqlGenerationData.AddSelectClause (selectClause, evaluations);
    }

    public void VisitLetClause (LetClause letClause)
    {
      ArgumentUtility.CheckNotNull ("letClause", letClause);
      Expression projectionBody = letClause.Expression;
      
      List<IEvaluation> listEvaluations =
        _detailParser.SelectProjectionParser.GetParser (projectionBody).Parse (projectionBody, _parseContext);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> evaluations =
        new Tuple<List<FieldDescriptor>, List<IEvaluation>> (_parseContext.FieldDescriptors, listEvaluations);

      LetData letData = new LetData(evaluations.B, letClause.Identifier.Name,letClause.GetColumnSource(_databaseInfo));
      SqlGenerationData.AddLetClauses (letData, evaluations);
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      throw new NotImplementedException ();
    }

    
  }
}