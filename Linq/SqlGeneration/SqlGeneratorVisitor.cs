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
    
    public SqlGeneratorVisitor (IDatabaseInfo databaseInfo)
    {
      _databaseInfo = databaseInfo;
      Tables = new List<Table>();
      Columns = new List<Column>();
    }

    public List<Table> Tables { get; private set; }
    public List<Column> Columns { get; private set; }
    public ICriterion Criterion{ get; private set; }

    public void VisitQueryExpression (QueryExpression queryExpression)
    {
      queryExpression.FromClause.Accept (this);
      queryExpression.QueryBody.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      Table tableEntry = GetTableForFromClause(fromClause);
      Tables.Add (tableEntry);
    }

    private Table GetTableForFromClause (FromClauseBase fromClause)
    {
      return new Table(_databaseInfo.GetTableName (fromClause.GetQuerySourceType()), fromClause.Identifier.Name);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      Table tableEntry = GetTableForFromClause (fromClause);
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
      BinaryExpression binaryExpression = whereClause.BoolExpression.Body as BinaryExpression;
      Assertion.IsNotNull (binaryExpression);
      Assertion.IsTrue (binaryExpression.Method.Name == "op_Equality");

      MemberExpression leftSide = binaryExpression.Left as MemberExpression;
      Assertion.IsNotNull (leftSide);
      ParameterExpression tableParameter = leftSide.Expression as ParameterExpression;
      Assertion.IsNotNull (tableParameter);

      FromClauseBase fromClause = FromClauseFinder.FindFromClauseForExpression (whereClause, tableParameter);
      Table table = GetTableForFromClause (fromClause);
      MemberInfo columnMember = leftSide.Member;
      Column leftColumn = GetColumn (table, columnMember);

      ConstantExpression rightSide = binaryExpression.Right as ConstantExpression;
      Assertion.IsNotNull (rightSide);
      Constant rightConstant = new Constant(rightSide.Value);

      Criterion = new BinaryCondition (leftColumn, rightConstant, BinaryCondition.ConditionKind.Equal);
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      SelectProjectionParser projectionParser = new SelectProjectionParser (selectClause, _databaseInfo);
      IEnumerable<Tuple<FromClauseBase, MemberInfo>> selectedFields = projectionParser.SelectedFields;
      
      IEnumerable<Column> columns =
          from field in selectedFields
          let table = GetTableForFromClause (field.A)
          select GetColumn (table, field.B);

      Columns.AddRange (columns);
    }

    private Column GetColumn (Table table, MemberInfo member)
    {
      return new Column (table, member == null ? "*" : _databaseInfo.GetColumnName (member));
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
    }

    public void VisitQueryBody (QueryBody queryBody)
    {
      foreach (IFromLetWhereClause fromLetWhereClause in queryBody.FromLetWhereClauses)
        fromLetWhereClause.Accept (this);
      if (queryBody.OrderByClause != null)
        queryBody.OrderByClause.Accept (this);
      queryBody.SelectOrGroupClause.Accept (this);
    }
  }
}