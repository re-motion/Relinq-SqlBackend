using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class WhereBuilder : IWhereBuilder
  {
    private readonly ICommandBuilder _commandBuilder;
    private readonly BinaryConditionBuilder _builder;

    public WhereBuilder (ICommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      _commandBuilder = commandBuilder;
      _builder = new BinaryConditionBuilder (_commandBuilder);
    }

    public void BuildWherePart (ICriterion criterion)
    {
      if (criterion != null)
      {
        _commandBuilder.Append (" WHERE ");
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
          _commandBuilder.AppendConstant (constant);
      }
      else
      {
        _commandBuilder.AppendColumn ((Column) value);
        _commandBuilder.Append ("=1");
      }
    }

    private void AppendComplexCriterion (ComplexCriterion criterion)
    {
      _commandBuilder.Append ("(");
      AppendCriterion (criterion.Left);

      switch (criterion.Kind)
      {
        case ComplexCriterion.JunctionKind.And:
          _commandBuilder.Append (" AND ");
          break;
        case ComplexCriterion.JunctionKind.Or:
          _commandBuilder.Append (" OR ");
          break;
      }

      AppendCriterion (criterion.Right);
      _commandBuilder.Append (")");
    }

    private void AppendNotCriterion (NotCriterion criterion)
    {
      _commandBuilder.Append ("NOT ");
      AppendCriterion (criterion.NegatedCriterion);
    }
  }
}