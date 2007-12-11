using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class SqlGenerator
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly QueryExpression _query;
    private readonly StringBuilder _commandText = new StringBuilder ();
    private readonly List<QueryParameter> _commandParameters = new List<QueryParameter> ();

    public SqlGenerator (QueryExpression query, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("query", query);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      _query = query;
      _databaseInfo = databaseInfo;

      BuildCommandString();
    }

    public string GetCommandString ()
    {
      return _commandText.ToString();
    }

    public QueryParameter[] GetCommandParameters ()
    {
      return _commandParameters.ToArray ();
    }
 
    private void BuildCommandString ()
    {
      SqlGeneratorVisitor visitor = new SqlGeneratorVisitor (_databaseInfo);
      _query.Accept (visitor);

      BuildSelectPart(visitor);
      BuildFromPart(visitor);
      BuildWherePart(visitor);
    }

    private void BuildSelectPart (SqlGeneratorVisitor visitor)
    {
      _commandText.Append ("SELECT ");

      if (visitor.Columns.Count == 0)
        throw new InvalidOperationException ("The query does not select any fields from the data source.");
      else
      {
        IEnumerable<string> columnEntries = JoinColumnItems (visitor.Columns);
        _commandText.Append (SeparatedStringBuilder.Build (", ", columnEntries)).Append (" ");
      }
    }

    private void BuildFromPart (SqlGeneratorVisitor visitor)
    {
      _commandText.Append ("FROM ");

      IEnumerable<string> tableEntries = JoinTableItems (visitor.Tables);
      _commandText.Append (SeparatedStringBuilder.Build (", ", tableEntries));
    }

    private void BuildWherePart (SqlGeneratorVisitor visitor)
    {
      ICriterion criterion = visitor.Criterion;
      if (criterion != null)
      {
        _commandText.Append (" WHERE ");
        AppendCriterion (criterion);
      }
    }

    private IEnumerable<string> JoinTableItems (IEnumerable<Table> tables)
    {
      foreach (Table table in tables)
        yield return WrapSqlIdentifier (table.Name) + " " + WrapSqlIdentifier (table.Alias);
    }

    private IEnumerable<string> JoinColumnItems (IEnumerable<Column> columns)
    {
      foreach (Column column in columns)
        yield return GetColumnString (column);
    }

    private string GetColumnString (Column column)
    {
      return WrapSqlIdentifier (column.Table.Alias) + "." + WrapSqlIdentifier (column.Name);
    }

    private string WrapSqlIdentifier (string identifier)
    {
      if (identifier != "*")
        return "[" + identifier + "]";
      else
        return "*";
    }

    private void AppendCriterion (ICriterion criterion)
    {
      ICondition condition;
      ComplexCriterion complexCriterion;

      if ((condition = criterion as ICondition) != null)
        AppendCondition (condition);
      else if ((complexCriterion = criterion as ComplexCriterion) != null)
        AppendComplexCriterion (complexCriterion);
    }

    private void AppendCondition (ICondition condition)
    {
      if (condition is BinaryCondition)
      {
        BinaryCondition binaryCondition = (BinaryCondition) condition;
        AppendValue (binaryCondition.Left);
        _commandText.Append (" ");
        AppendBinaryConditionKind (binaryCondition.Kind);
        _commandText.Append (" ");
        AppendValue (binaryCondition.Right);
      }
    }

    private void AppendValue (IValue value)
    {
      if (value is Constant)
      {
        Constant constant = (Constant) value;
        QueryParameter parameter = AddParameter (constant.Value);
        _commandText.Append (parameter.Name);
      }
      else
      {
        Column column = (Column) value;
        _commandText.Append (GetColumnString (column));
      }
    }

    private void AppendBinaryConditionKind (BinaryCondition.ConditionKind kind)
    {
      string commandString = null;
      switch (kind)
      {
        case BinaryCondition.ConditionKind.Equal:
          commandString = "=";
          break;
      }
      Assertion.IsNotNull (commandString);
      _commandText.Append (commandString);
    }

    private void AppendComplexCriterion (ComplexCriterion criterion)
    {
      throw new NotImplementedException ();
    }

    private QueryParameter AddParameter (object value)
    {
      QueryParameter parameter = new QueryParameter ("@" + (_commandParameters.Count + 1), value);
      _commandParameters.Add (parameter);
      return parameter;
    }
 }
}