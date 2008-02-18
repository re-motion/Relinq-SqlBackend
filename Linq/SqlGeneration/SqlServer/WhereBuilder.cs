using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class WhereBuilder : IWhereBuilder
  {
    private readonly StringBuilder _commandText;
    private readonly List<CommandParameter> _commandParameters;

    public WhereBuilder (StringBuilder commandText, List<CommandParameter> commandParameters)
    {
      ArgumentUtility.CheckNotNull ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("commandParameters", commandParameters);
      _commandText = commandText;
      _commandParameters = commandParameters;
    }

    public void BuildWherePart (ICriterion criterion)
    {
      if (criterion != null)
      {
        _commandText.Append (" WHERE ");
        AppendCriterion (criterion);
      }
    }

    private void AppendCriterion (ICriterion criterion)
    {
      ICondition condition;

      if ((condition = criterion as ICondition) != null)
        AppendCondition (condition);
      else if (criterion is ComplexCriterion)
        AppendComplexCriterion ((ComplexCriterion) criterion);
      else if (criterion is NotCriterion)
        AppendNotCriterion ((NotCriterion) criterion);
      else if (criterion is Constant)
        AppendValue (criterion);
      else
        throw new NotSupportedException ("The criterion kind " + criterion.GetType ().Name + " is not supported.");
    }

    private void AppendCondition (ICondition condition)
    {
      if (condition is BinaryCondition)
      {
        BinaryCondition binaryCondition = (BinaryCondition) condition;
        AppendBinaryCondition (binaryCondition);
      }
      else
        throw new NotSupportedException ("The condition kind " + condition.GetType ().Name + " is not supported.");
    }

    private void AppendBinaryCondition (BinaryCondition binaryCondition)
    {
      if (binaryCondition.Left.Equals (new Constant (null)))
        AppendNullCondition (binaryCondition.Right, binaryCondition.Kind);
      else if (binaryCondition.Right.Equals (new Constant (null)))
        AppendNullCondition (binaryCondition.Left, binaryCondition.Kind);
      else
      {
        AppendValue (binaryCondition.Left);
        _commandText.Append (" ");
        AppendBinaryConditionKind (binaryCondition.Kind);
        _commandText.Append (" ");
        AppendValue (binaryCondition.Right);
      }
    }

    private void AppendNullCondition (IValue value, BinaryCondition.ConditionKind kind)
    {
      AppendValue (value);
      switch (kind)
      {
        case BinaryCondition.ConditionKind.Equal:
          _commandText.Append (" IS NULL");
          break;
        default:
          Assertion.IsTrue (kind == BinaryCondition.ConditionKind.NotEqual, "null can only be compared via == and !=");
          _commandText.Append (" IS NOT NULL");
          break;
      }
    }

    private void AppendValue (IValue value)
    {
      if (value is Constant)
      {
        Constant constant = (Constant) value;
        if (constant.Value == null)
          _commandText.Append ("NULL");
        else if (constant.Value.Equals (true))
          _commandText.Append ("1=1");
        else if (constant.Value.Equals (false))
          _commandText.Append ("1!=1");
        else
        {
          CommandParameter parameter = AddParameter (constant.Value);
          _commandText.Append (parameter.Name);
        }
      }
      else
      {
        Column column = (Column) value;
        _commandText.Append (SqlServerUtility.GetColumnString (column));
      }
    }

    private void AppendBinaryConditionKind (BinaryCondition.ConditionKind kind)
    {
      string commandString;
      switch (kind)
      {
        case BinaryCondition.ConditionKind.Equal:
          commandString = "=";
          break;
        case BinaryCondition.ConditionKind.NotEqual:
          commandString = "!=";
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
      _commandText.Append (commandString);
    }

    private void AppendComplexCriterion (ComplexCriterion criterion)
    {
      _commandText.Append ("(");
      AppendCriterion (criterion.Left);
      _commandText.Append (")");

      switch (criterion.Kind)
      {
        case ComplexCriterion.JunctionKind.And:
          _commandText.Append (" AND ");
          break;
        case ComplexCriterion.JunctionKind.Or:
          _commandText.Append (" OR ");
          break;
      }

      _commandText.Append ("(");
      AppendCriterion (criterion.Right);
      _commandText.Append (")");
    }

    private void AppendNotCriterion (NotCriterion criterion)
    {
      _commandText.Append ("NOT (");
      AppendCriterion (criterion.NegatedCriterion);
      _commandText.Append (")");
    }

    private CommandParameter AddParameter (object value)
    {
      CommandParameter parameter = new CommandParameter ("@" + (_commandParameters.Count + 1), value);
      _commandParameters.Add (parameter);
      return parameter;
    }
  }
}