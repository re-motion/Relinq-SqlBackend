using System;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class BinaryConditionBuilder
  {
    private readonly ICommandBuilder _commandBuilder;
    private readonly IDatabaseInfo _databaseInfo;

    public BinaryConditionBuilder (ICommandBuilder commandBuilder, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("command", commandBuilder);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      _commandBuilder = commandBuilder;
      _databaseInfo = databaseInfo;
    }

    public void BuildBinaryConditionPart (BinaryCondition binaryCondition)
    {
      if (binaryCondition.Left.Equals (new Constant (null)))
        AppendNullCondition (binaryCondition.Right, binaryCondition.Kind);
      else if (binaryCondition.Right.Equals (new Constant (null)))
        AppendNullCondition (binaryCondition.Left, binaryCondition.Kind);
      else if (binaryCondition.Kind == BinaryCondition.ConditionKind.Contains)
        AppendContainsCondition ((SubQuery) binaryCondition.Left, binaryCondition.Right);
      else if (binaryCondition.Kind == BinaryCondition.ConditionKind.ContainsFulltext)
        AppendContainsFulltext (binaryCondition.Left,binaryCondition.Right);
      else
        AppendGeneralCondition (binaryCondition);
    }

    private void AppendContainsFulltext(IValue left, IValue right)
    {
      _commandBuilder.Append ("CONTAINS (");
      AppendValue (left);
      _commandBuilder.Append (",");
      AppendValue (right);
      _commandBuilder.Append(")");
      
    }

    private void AppendNullCondition (IValue value, BinaryCondition.ConditionKind kind)
    {
      AppendValue (value);
      switch (kind)
      {
        case BinaryCondition.ConditionKind.Equal:
          _commandBuilder.Append (" IS NULL");
          break;
        default:
          Assertion.IsTrue (kind == BinaryCondition.ConditionKind.NotEqual, "null can only be compared via == and !=");
          _commandBuilder.Append (" IS NOT NULL");
          break;
      }
    }

    private void AppendContainsCondition (SubQuery left, IValue right)
    {
      AppendValue (right);
      _commandBuilder.Append (" IN (");
      CreateSqlGeneratorForSubQuery (left, _databaseInfo, _commandBuilder).BuildCommandString (left.QueryModel);
      _commandBuilder.Append (")");
    }
    
    protected virtual SqlGeneratorBase CreateSqlGeneratorForSubQuery (SubQuery subQuery, IDatabaseInfo databaseInfo, ICommandBuilder commandBuilder)
    {
      return new SqlServerGenerator (databaseInfo, commandBuilder, ParseContext.SubQueryInWhere);
    }

    private void AppendGeneralCondition (BinaryCondition binaryCondition)
    {
      _commandBuilder.Append ("(");
      AppendNullChecks (binaryCondition.Left, binaryCondition.Right, binaryCondition.Kind);

      AppendValue (binaryCondition.Left);
      _commandBuilder.Append (" ");
      AppendConditionKind (binaryCondition.Kind);
      _commandBuilder.Append (" ");
      AppendValue (binaryCondition.Right);
      _commandBuilder.Append (")");
    }

    private void AppendNullChecks (IValue left, IValue right, BinaryCondition.ConditionKind conditionKind)
    {
      if (left is Column || right is Column)
      {
        switch (conditionKind)
        {
          case BinaryCondition.ConditionKind.Equal:
          case BinaryCondition.ConditionKind.LessThanOrEqual:
          case BinaryCondition.ConditionKind.GreaterThanOrEqual:
            AppendNullChecksForEqualKinds(left, right);
            break;
          case BinaryCondition.ConditionKind.NotEqual:
            AppendNullChecksForNotEqualKind(left, right);
            break;
        }
      }
    }

    private void AppendNullChecksForEqualKinds (IValue left, IValue right)
    {
      if (left is Column && right is Column)
      {
        _commandBuilder.Append ("(");
        AppendNullCondition (left, BinaryCondition.ConditionKind.Equal);
        _commandBuilder.Append (" AND ");
        AppendNullCondition (right, BinaryCondition.ConditionKind.Equal);
        _commandBuilder.Append (") OR ");
      }
    }

    private void AppendNullChecksForNotEqualKind (IValue left, IValue right)
    {
      if (left is Column && right is Column)
      {
        _commandBuilder.Append ("(");
        AppendNullCondition (left, BinaryCondition.ConditionKind.Equal);
        _commandBuilder.Append (" AND ");
        AppendNullCondition (right, BinaryCondition.ConditionKind.NotEqual);
        _commandBuilder.Append (") OR ");
        _commandBuilder.Append ("(");
        AppendNullCondition (left, BinaryCondition.ConditionKind.NotEqual);
        _commandBuilder.Append (" AND ");
        AppendNullCondition (right, BinaryCondition.ConditionKind.Equal);
        _commandBuilder.Append (") OR ");
      }
      else if (left is Column)
      {
        AppendNullCondition (left, BinaryCondition.ConditionKind.Equal);
        _commandBuilder.Append (" OR ");
      }
      else
      {
        AppendNullCondition (right, BinaryCondition.ConditionKind.Equal);
        _commandBuilder.Append (" OR ");
      }
    }
    
    private void AppendValue (IValue value)
    {
      if (value is Constant)
        _commandBuilder.AppendConstant ((Constant) value);
      else if (value is Column)
        _commandBuilder.AppendEvaluation((Column) value);
      else
        throw new NotSupportedException ("Value type " + value.GetType ().Name + " is not supported.");
    }

    private void AppendConditionKind (BinaryCondition.ConditionKind kind)
    {
      string commandString;
      switch (kind)
      {
        case BinaryCondition.ConditionKind.Equal:
          commandString = "=";
          break;
        case BinaryCondition.ConditionKind.NotEqual:
          commandString = "<>";
          break;
        case BinaryCondition.ConditionKind.LessThan:
          commandString = "<";
          break;
        case BinaryCondition.ConditionKind.LessThanOrEqual:
          commandString = "<=";
          break;
        case BinaryCondition.ConditionKind.GreaterThan:
          commandString = ">";
          break;
        case BinaryCondition.ConditionKind.GreaterThanOrEqual:
          commandString = ">=";
          break;
        case BinaryCondition.ConditionKind.Like:
          commandString = "LIKE";
          break;
        default:
          throw new NotSupportedException ("The binary condition kind " + kind + " is not supported.");
      }
      _commandBuilder.Append (commandString);
    }
  }
}