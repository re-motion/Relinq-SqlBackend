// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;


namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class SqlServerEvaluationVisitor : IEvaluationVisitor
  {
    public SqlServerEvaluationVisitor (CommandBuilder commandBuilder, IDatabaseInfo databaseInfo, MethodCallSqlGeneratorRegistry methodCallRegistry)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("methodCallRegistry", methodCallRegistry);

      CommandBuilder = commandBuilder;
      DatabaseInfo = databaseInfo;
      MethodCallRegistry = methodCallRegistry;
    }

    public CommandBuilder CommandBuilder { get; private set; }
    public IDatabaseInfo DatabaseInfo { get; private set; }
    public MethodCallSqlGeneratorRegistry MethodCallRegistry { get; private set; }


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
      else if (constant.Value is ICollection)
        AddConstantCollection ((ICollection) constant.Value);
      else if (constant.Value.Equals (true))
        CommandBuilder.Append ("(1=1)");
      else if (constant.Value.Equals (false))
        CommandBuilder.Append ("(1<>1)");
      else
      {
        CommandParameter parameter = CommandBuilder.AddParameter (constant.Value);
        CommandBuilder.CommandText.Append (parameter.Name);
      }
    }

    private void AddConstantCollection (ICollection enumerable)
    {
      int counter = 0;
      foreach (var cons in enumerable)
      {
        VisitConstant (new Constant (cons));
        counter++;
        if (counter != enumerable.Count)
         CommandBuilder.Append (", ");
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
      new BinaryConditionBuilder (CommandBuilder).BuildBinaryConditionPart (binaryCondition);
    }

    public void VisitSubQuery (SubQuery subQuery)
    {
      CommandBuilder.Append ("(");
      //new InlineSqlServerGenerator (DatabaseInfo, CommandBuilder, ParseMode.SubQueryInSelect).BuildCommand (subQuery.QueryModel);
      new InlineSqlServerGenerator (DatabaseInfo, CommandBuilder, subQuery.ParseMode).BuildCommand (subQuery.QueryModel);
      CommandBuilder.Append (")");
      if (subQuery.Alias != null)
      {
        CommandBuilder.Append (" [");
        CommandBuilder.Append (subQuery.Alias);
        CommandBuilder.Append ("]");
      }
    }

    public void VisitMethodCall (MethodCall methodCall)
    {
      ArgumentUtility.CheckNotNull ("methodCall", methodCall);

      MethodCallRegistry.GetGenerator (methodCall.EvaluationMethodInfo).GenerateSql (methodCall, CommandBuilder);
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

    //public void VisitSourceMarkerEvaluation (SourceMarkerEvaluation sourceMarkerEvaluation)
    //{
    //  CommandBuilder.Append ("");
    //}
  }
}
