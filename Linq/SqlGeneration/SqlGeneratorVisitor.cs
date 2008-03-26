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
    private readonly QueryExpression _queryExpression;

    public SqlGeneratorVisitor (QueryExpression queryExpression, IDatabaseInfo databaseInfo, JoinedTableContext context)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      ArgumentUtility.CheckNotNull ("context", context);

      _databaseInfo = databaseInfo;
      _context = context;
      _queryExpression = queryExpression;

      Tables = new List<Table>();
      Columns = new List<Column>();
      OrderingFields = new List<OrderingField>();
      Joins = new JoinCollection (); 
    }

    public List<Table> Tables { get; private set; }
    public List<Column> Columns { get; private set; }
    public ICriterion Criterion{ get; private set; }
    public List<OrderingField> OrderingFields { get; private set; }
    public JoinCollection Joins { get; private set; }

    public void VisitQueryExpression (QueryExpression queryExpression)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      queryExpression.MainFromClause.Accept (this);
      foreach (IBodyClause bodyClause in queryExpression.BodyClauses)
        bodyClause.Accept (this);

      queryExpression.SelectOrGroupClause.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      Table tableEntry = fromClause.GetTable (_databaseInfo);
      Tables.Add (tableEntry);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      Table tableEntry = fromClause.GetTable (_databaseInfo);
      Tables.Add (tableEntry);
    }

    public void VisitSubQueryFromClause (SubQueryFromClause clause)
    {
      throw new NotImplementedException();
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
      var conditionParser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, _context, true);
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
      var fieldParser = new OrderingFieldParser (_queryExpression, orderingClause, _databaseInfo, _context);
      OrderingField orderingField = fieldParser.GetField();
      OrderingFields.Add (orderingField);
      Joins.AddPath (orderingField.FieldDescriptor.SourcePath);
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      var projectionParser = new SelectProjectionParser (_queryExpression, selectClause, _databaseInfo, _context);
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