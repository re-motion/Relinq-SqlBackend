using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class WhereBuilder : IWhereBuilder
  {
    private readonly SqlCommand _command;
    private readonly BinaryConditionBuilder _builder;

    public WhereBuilder (StringBuilder commandText, List<CommandParameter> commandParameters)
    {
      ArgumentUtility.CheckNotNull ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("commandParameters", commandParameters);

      _command = new SqlCommand (commandText, commandParameters);
      _builder = new BinaryConditionBuilder (_command);
    }

    public void BuildWherePart (ICriterion criterion)
    {
      if (criterion != null)
      {
        _command.Append (" WHERE ");
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
      else if (criterion is Constant || criterion is Column) // cannot use "as" operator here because Constant/Column are value types
        AppendTopLevelValue (criterion);
      else
        throw new NotSupportedException ("The criterion kind " + criterion.GetType ().Name + " is not supported.");
    }

    private void AppendCondition (ICondition condition)
    {
      if (condition is BinaryCondition)
      {
        BinaryCondition binaryCondition = (BinaryCondition) condition;
        _builder.BuildBinaryConditionPart (binaryCondition);
      }
      else
        throw new NotSupportedException ("The condition kind " + condition.GetType ().Name + " is not supported.");
    }

    private void AppendTopLevelValue (IValue value)
    {
      if (value is Constant)
      {
        Constant constant = (Constant) value;
        if (constant.Value == null)
          throw new NotSupportedException ("NULL constants are not supported as WHERE conditions.");
        else
          _command.AppendConstant (constant);
      }
      else
      {
        _command.AppendColumn ((Column) value);
        _command.Append ("=1");
      }
    }

    private void AppendComplexCriterion (ComplexCriterion criterion)
    {
      _command.Append ("(");
      AppendCriterion (criterion.Left);

      switch (criterion.Kind)
      {
        case ComplexCriterion.JunctionKind.And:
          _command.Append (" AND ");
          break;
        case ComplexCriterion.JunctionKind.Or:
          _command.Append (" OR ");
          break;
      }

      AppendCriterion (criterion.Right);
      _command.Append (")");
    }

    private void AppendNotCriterion (NotCriterion criterion)
    {
      _command.Append ("NOT ");
      AppendCriterion (criterion.NegatedCriterion);
    }
  }
}