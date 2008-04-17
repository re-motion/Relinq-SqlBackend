using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class SqlServerEvaluationVisitor : IEvaluationVisitor
  {

    public SqlServerEvaluationVisitor (CommandBuilder commandBuilder, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      
      CommandBuilder = commandBuilder;
      DatabaseInfo = databaseInfo;
    }

    public CommandBuilder CommandBuilder { get; private set; }
    public IDatabaseInfo DatabaseInfo { get; private set; }


    public void VisitBinaryEvaluation (BinaryEvaluation binaryEvaluation)
    {
      CommandBuilder.Append ("(");
      binaryEvaluation.Left.Accept (this);
      switch (binaryEvaluation.Kind)
      {
        case BinaryEvaluation.EvaluationKind.Add:
          CommandBuilder.Append (" + ");
          break;
        case BinaryEvaluation.EvaluationKind.Divide:
          CommandBuilder.Append (" / ");
          break;
        case BinaryEvaluation.EvaluationKind.Modulo:
          CommandBuilder.Append (" % ");
          break;
        case BinaryEvaluation.EvaluationKind.Multiply:
          CommandBuilder.Append (" * ");
          break;
        case BinaryEvaluation.EvaluationKind.Subtract:
          CommandBuilder.Append (" - ");
          break;
      }
      binaryEvaluation.Right.Accept (this);
      CommandBuilder.Append (")");

    }

    public void VisitComplexCriterion (ComplexCriterion complexCriterion)
    {
      throw new System.NotImplementedException();
    }

    public void VisitNotCriterion (NotCriterion notCriterion)
    {
      throw new System.NotImplementedException();
    }

    public void VisitOrderingField (OrderingField orderingField)
    {
      throw new System.NotImplementedException();
    }

    public void VisitConstant (Constant constant)
    {
      ArgumentUtility.CheckNotNull ("constant", constant);
      if (constant.Value == null)
        CommandBuilder.CommandText.Append ("NULL");
      else if (constant.Value.Equals (true))
        CommandBuilder.Append ("(1=1)");
      else if (constant.Value.Equals (false))
        CommandBuilder.Append ("(1<>1)");
      else
      {
        CommandBuilder commandBuilder = new CommandBuilder (CommandBuilder.CommandText,CommandBuilder.CommandParameters);
        CommandParameter parameter = commandBuilder.AddParameter (constant.Value);
        CommandBuilder.CommandText.Append (parameter.Name);
      }
    }

    public void VisitColumn (Column column)
    {
      ArgumentUtility.CheckNotNull ("column", column);
      CommandBuilder.CommandText.Append (SqlServerUtility.GetColumnString (column));
    }

    public void VisitBinaryCondition (BinaryCondition binaryCondition)
    {
      ArgumentUtility.CheckNotNull ("binaryCondition", binaryCondition);
      new BinaryConditionBuilder (CommandBuilder, DatabaseInfo).BuildBinaryConditionPart (binaryCondition);
    }

    public void VisitSubQuery (SubQuery subQuery)
    {
      throw new System.NotImplementedException();
    }

    public void VisitMethodCallEvaluation (MethodCallEvaluation methodCallEvaluation)
    {
      throw new System.NotImplementedException();
    }

    private CommandParameter AddParameter (object value)
    {
      CommandParameter parameter = new CommandParameter ("@" + (CommandBuilder.CommandParameters.Count + 1), value);
      CommandBuilder.CommandParameters.Add (parameter);
      return parameter;
    }
  }
}