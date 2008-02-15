using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class SqlGeneratorVisitor : IQueryVisitor
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly QueryExpression _queryExpression;

    public SqlGeneratorVisitor (IDatabaseInfo databaseInfo, QueryExpression queryExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);

      _databaseInfo = databaseInfo;
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
      queryExpression.MainFromClause.Accept (this);
      queryExpression.QueryBody.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      Table tableEntry = DatabaseInfoUtility.GetTableForFromClause(_databaseInfo, fromClause);
      Tables.Add (tableEntry);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      Table tableEntry = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Tables.Add (tableEntry);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
    }

    public void VisitLetClause (LetClause letClause)
    {
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      WhereConditionParser conditionParser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, true);
      Criterion = conditionParser.GetCriterion();
      // TODO: add joins
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      foreach (OrderingClause clause in orderByClause.OrderingList)
        clause.Accept (this);
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      OrderingFieldParser fieldParser = new OrderingFieldParser (_queryExpression, orderingClause, _databaseInfo);
      OrderingField orderingField = fieldParser.GetField();
      OrderingFields.Add (orderingField);
      // TODO: add joins
      AddJoinsForFieldAccess (orderingField.FieldDescriptor.SourcePath);
    }

    private void AddJoinsForFieldAccess (IFieldSourcePath fieldSourcePath)
    {
      if (fieldSourcePath is Join)
        Joins.Add ((Join) fieldSourcePath);
    }


    public void VisitSelectClause (SelectClause selectClause)
    {
      SelectProjectionParser projectionParser = new SelectProjectionParser (_queryExpression, selectClause, _databaseInfo);
      IEnumerable<FieldDescriptor> selectedFields = projectionParser.SelectedFields;
      
      IEnumerable<Column> columns =
          from field in selectedFields
          select field.GetMandatoryColumn ();

      Columns.AddRange (columns);
      // TODO: add joins
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
    }

    public void VisitQueryBody (QueryBody queryBody)
    {
      foreach (IBodyClause bodyClause in queryBody.BodyClauses)
        bodyClause.Accept (this);
      queryBody.SelectOrGroupClause.Accept (this);

    }
  }
}