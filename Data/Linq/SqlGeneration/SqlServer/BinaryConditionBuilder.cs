/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class BinaryConditionBuilder
  {
    private readonly CommandBuilder _commandBuilder;

    public BinaryConditionBuilder (CommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("command", commandBuilder);
      _commandBuilder = commandBuilder;
    }

    public void BuildBinaryConditionPart (BinaryCondition binaryCondition)
    {
      if (binaryCondition.Left.Equals (new Constant (null)))
        AppendNullCondition (binaryCondition.Right, binaryCondition.Kind);
      else if (binaryCondition.Right.Equals (new Constant (null)))
        AppendNullCondition (binaryCondition.Left, binaryCondition.Kind);
      else if (binaryCondition.Kind == BinaryCondition.ConditionKind.Contains)
        AppendContainsCondition (binaryCondition.Left, binaryCondition.Right);
        //AppendContainsCondition ((SubQuery) binaryCondition.Left, binaryCondition.Right);
      else if (binaryCondition.Kind == BinaryCondition.ConditionKind.ContainsFulltext)
        AppendContainsFulltext (binaryCondition.Left, binaryCondition.Right);
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

    private void AppendContainsCondition (IEvaluation left, IValue right)
    {
      if (left is Constant)
      {
        var constant = (Constant) left;
        if (constant.Value is ICollection)
        {
          var possibleEmptyCollection = (ICollection) constant.Value;
          if (possibleEmptyCollection.Count == 0)
            _commandBuilder.Append ("(0 = 1)");
          else
            AppendContainsForSubQuery (left, right);
        }
      }
      else
        AppendContainsForSubQuery (left, right);
    }

    private void AppendContainsForSubQuery (IEvaluation left, IValue right)
    {
      AppendValue (right);
      _commandBuilder.Append (" IN (");
      _commandBuilder.AppendEvaluation (left);
      _commandBuilder.Append (")");
    }

    protected virtual ISqlGenerator CreateSqlGeneratorForSubQuery (SubQuery subQuery, IDatabaseInfo databaseInfo, CommandBuilder commandBuilder)
    {
      return new InlineSqlServerGenerator (databaseInfo, commandBuilder, ParseMode.SubQueryInWhere);
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
      _commandBuilder.AppendEvaluation (value);
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
