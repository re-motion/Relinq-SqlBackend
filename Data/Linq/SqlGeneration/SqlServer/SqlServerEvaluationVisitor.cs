/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
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
      ArgumentUtility.CheckNotNull ("binaryEvaluation", binaryEvaluation);
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
      ArgumentUtility.CheckNotNull ("complexCriterion", complexCriterion);
      CommandBuilder.Append ("(");
      complexCriterion.Left.Accept (this);
      switch (complexCriterion.Kind)
      {
        case ComplexCriterion.JunctionKind.And:
          CommandBuilder.Append (" AND ");
          break;
        case ComplexCriterion.JunctionKind.Or:
          CommandBuilder.Append (" OR ");
          break;
      }
      complexCriterion.Right.Accept (this);
      CommandBuilder.Append (")");
    }

    public void VisitNotCriterion (NotCriterion notCriterion)
    {
      ArgumentUtility.CheckNotNull ("notCriterion", notCriterion);
      CommandBuilder.Append (" NOT ");
      notCriterion.NegatedCriterion.Accept (this);
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
        CommandBuilder commandBuilder = new CommandBuilder (CommandBuilder.CommandText, CommandBuilder.CommandParameters, DatabaseInfo);
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
      CommandBuilder.Append ("((");
      new InlineSqlServerGenerator (DatabaseInfo, CommandBuilder, ParseMode.SubQueryInSelect).BuildCommand (subQuery.QueryModel);
      CommandBuilder.Append (") ");
      CommandBuilder.Append (subQuery.Alias);
      CommandBuilder.Append (")");
    }

    public void VisitMethodCallEvaluation (MethodCall methodCall)
    {
      switch (methodCall.EvaluationMethodInfo.Name)
      {
        case "ToUpper":
          CommandBuilder.Append ("UPPER(");
          methodCall.EvaluationParameter.Accept (this);
          CommandBuilder.Append (")");
          break;

        case "Remove":
          CommandBuilder.Append ("STUFF(");
          methodCall.EvaluationParameter.Accept (this);
          CommandBuilder.Append (",");

          foreach (var argument in methodCall.EvaluationArguments)
            argument.Accept (this);

          CommandBuilder.Append (",CONVERT(Int,DATALENGTH(");
          methodCall.EvaluationParameter.Accept (this);
          CommandBuilder.Append (") / 2), \"");
          CommandBuilder.Append (")");
          break;

        default:
          string message = string.Format (
              "The method {0}.{1} is not supported by the SQL Server code generator.",
              methodCall.EvaluationMethodInfo.DeclaringType.FullName,
              methodCall.EvaluationMethodInfo.Name);
          throw new SqlGenerationException (message);
      }
    }

    public void VisitNewObjectEvaluation (NewObject newObject)
    {
      bool first = true;
      foreach (var argument in newObject.ConstructorArguments)
      {
        if (!first)
          CommandBuilder.Append (", ");
        argument.Accept (this);
        first = false;
      }
    }
  }
}
