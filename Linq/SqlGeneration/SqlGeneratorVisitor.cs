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
    public ParseContext ParseContext { get; private set; }

    private readonly IDatabaseInfo _databaseInfo;
    private readonly JoinedTableContext _context;
    private readonly QueryModel _queryModel;
    
    public SqlGeneratorVisitor (QueryModel queryModel, IDatabaseInfo databaseInfo, JoinedTableContext context, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      ArgumentUtility.CheckNotNull ("context", context);

      _databaseInfo = databaseInfo;
      _context = context;
      _queryModel = queryModel;

      ParseContext = parseContext;

      SqlGenerationData = new SqlGenerationData();  
    }

    public SqlGenerationData SqlGenerationData { get; private set; }

    public void VisitQueryExpression (QueryModel queryModel)
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
      SqlGenerationData.FromSources.Add(columnSource);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
      throw new NotImplementedException();
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      var conditionParser = new WhereConditionParser (_queryModel, whereClause, _databaseInfo, _context, true);
      Tuple<List<FieldDescriptor>, ICriterion> criterions = conditionParser.GetParseResult();
      if (SqlGenerationData.Criterion == null)
        SqlGenerationData.SetCriterion(criterions.B);
      else
        SqlGenerationData.SetCriterion (new ComplexCriterion (SqlGenerationData.Criterion, criterions.B, ComplexCriterion.JunctionKind.And));

      foreach (var fieldDescriptor in criterions.A)
        SqlGenerationData.Joins.AddPath (fieldDescriptor.SourcePath);
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
      var fieldParser = new OrderingFieldParser (_queryModel, orderingClause, _databaseInfo, _context);
      OrderingField orderingField = fieldParser.GetField();
      SqlGenerationData.OrderingFields.Add(orderingField);
      SqlGenerationData.Joins.AddPath (orderingField.FieldDescriptor.SourcePath);
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      Expression projectionBody = selectClause.ProjectionExpression != null ? selectClause.ProjectionExpression.Body : _queryModel.MainFromClause.Identifier;
      var projectionParser = new SelectProjectionParser (_queryModel, projectionBody, _databaseInfo, _context, ParseContext);

      Tuple<List<FieldDescriptor>, List<IEvaluation>> evaluations = projectionParser.GetParseResult ();

      Distinct = selectClause.Distinct;
      
      SqlGenerationData.SelectEvaluations.AddRange(evaluations.B);
      foreach (var selectedField in evaluations.A)
        SqlGenerationData.Joins.AddPath(selectedField.SourcePath);
    }

    public void VisitLetClause (LetClause letClause)
    {
      ArgumentUtility.CheckNotNull ("letClause", letClause);
      Expression projectionBody = letClause.Expression;
      var projectionParser = new SelectProjectionParser (_queryModel, projectionBody, _databaseInfo, _context, ParseContext);
      Tuple<List<FieldDescriptor>, List<IEvaluation>> evaluations = projectionParser.GetParseResult ();

      LetData letData = new LetData(evaluations.B, letClause.Identifier.Name,letClause.GetColumnSource(_databaseInfo));
      SqlGenerationData.LetEvaluations.Add(letData);
      
      foreach (var selectedField in evaluations.A)
      {
        SqlGenerationData.Joins.AddPath(selectedField.SourcePath);
      }   
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      throw new NotImplementedException ();
    }

    public bool Distinct { get; private set; }
  }
}