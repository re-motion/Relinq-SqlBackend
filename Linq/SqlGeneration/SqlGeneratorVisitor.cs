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
      WhereConditionParser conditionParser = new WhereConditionParser (_queryExpression, whereClause, _databaseInfo, _context, true);
      Tuple<List<FieldDescriptor>, ICriterion> criterions = conditionParser.GetParseResult();
      Criterion = criterions.B;

      foreach (var fieldDescriptor in criterions.A)
        AddJoinsForFieldAccess (fieldDescriptor.SourcePath);
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      foreach (OrderingClause clause in orderByClause.OrderingList)
        clause.Accept (this);
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      OrderingFieldParser fieldParser = new OrderingFieldParser (_queryExpression, orderingClause, _databaseInfo, _context);
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
      SelectProjectionParser projectionParser = new SelectProjectionParser (_queryExpression, selectClause, _databaseInfo, _context);
      IEnumerable<FieldDescriptor> selectedFields = projectionParser.GetSelectedFields();
      
      foreach (var selectedField in selectedFields)
      {
        Columns.Add (selectedField.GetMandatoryColumn());
        AddJoinsForFieldAccess (selectedField.SourcePath);
      }
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