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
    }

    public List<Table> Tables { get; private set; }
    public List<Column> Columns { get; private set; }
    public ICriterion Criterion{ get; private set; }
    public List<OrderingField> OrderingFields { get; private set; }

    public void VisitQueryExpression (QueryExpression queryExpression)
    {
      queryExpression.FromClause.Accept (this);
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
      WhereConditionParser conditionParser = new WhereConditionParser (whereClause, _databaseInfo, true);
      Criterion = conditionParser.GetCriterion();
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      foreach (OrderingClause clause in orderByClause.OrderingList)
      {
        clause.Accept (this);
      }
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      OrderingFieldParser fieldParser = new OrderingFieldParser (_queryExpression, orderingClause, _databaseInfo);
      OrderingField orderingField = fieldParser.GetField();
      OrderingFields.Add (orderingField);
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      SelectProjectionParser projectionParser = new SelectProjectionParser (selectClause, _databaseInfo);
      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = projectionParser.SelectedFields;
      
      IEnumerable<Column> columns =
          from field in selectedFields
          let table = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, field.A)
          select DatabaseInfoUtility.GetColumn (_databaseInfo, table, field.B).Value;

      Columns.AddRange (columns);
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