using System;
using System.Collections.Generic;
using System.Linq;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class SqlGeneratorVisitor : IQueryVisitor
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly JoinedTableContext _context;
    private readonly QueryModel _queryModel;

    public SqlGeneratorVisitor (QueryModel queryModel, IDatabaseInfo databaseInfo, JoinedTableContext context)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      ArgumentUtility.CheckNotNull ("context", context);

      _databaseInfo = databaseInfo;
      _context = context;
      _queryModel = queryModel;

      FromSources = new List<IFromSource>();
      Columns = new List<Column>();
      OrderingFields = new List<OrderingField>();
      Joins = new JoinCollection (); 
    }

    public List<IFromSource> FromSources { get; private set; }
    public List<Column> Columns { get; private set; }
    public ICriterion Criterion{ get; private set; }
    public List<OrderingField> OrderingFields { get; private set; }
    public JoinCollection Joins { get; private set; }

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
      IFromSource fromSource = fromClause.GetFromSource (_databaseInfo);
      FromSources.Add (fromSource);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
      throw new NotImplementedException();
    }

    public void VisitLetClause (LetClause letClause)
    {
      throw new NotImplementedException ();
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      var conditionParser = new WhereConditionParser (_queryModel, whereClause, _databaseInfo, _context, true);
      Tuple<List<FieldDescriptor>, ICriterion> criterions = conditionParser.GetParseResult();
      if (Criterion == null)
        Criterion = criterions.B;
      else
        Criterion = new ComplexCriterion (Criterion, criterions.B, ComplexCriterion.JunctionKind.And);


      foreach (var fieldDescriptor in criterions.A)
        Joins.AddPath (fieldDescriptor.SourcePath);
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
      OrderingFields.Add (orderingField);
      Joins.AddPath (orderingField.FieldDescriptor.SourcePath);
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      var projectionParser = new SelectProjectionParser (_queryModel, selectClause, _databaseInfo, _context);
      IEnumerable<FieldDescriptor> selectedFields = projectionParser.GetSelectedFields();
      Distinct = selectClause.Distinct;
      foreach (var selectedField in selectedFields)
      {
        Columns.Add (selectedField.GetMandatoryColumn());
        Joins.AddPath (selectedField.SourcePath);
      }
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      throw new NotImplementedException ();
    }

    public bool Distinct { get; private set; }
  }
}