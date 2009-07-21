// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer
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
      else if (binaryCondition.Kind == BinaryCondition.ConditionKind.ContainsFulltext)
        AppendContainsFulltext (binaryCondition.Left, binaryCondition.Right);
      else
        AppendGeneralCondition (binaryCondition);
    }

    private void AppendContainsFulltext (IValue left, IValue right)
    {
      _commandBuilder.Append ("CONTAINS (");
      _commandBuilder.AppendEvaluation (left);
      _commandBuilder.Append (",");
      _commandBuilder.AppendEvaluation (right);
      _commandBuilder.Append (")");
    }

    private void AppendNullCondition (IValue value, BinaryCondition.ConditionKind kind)
    {
      _commandBuilder.AppendEvaluation (value);
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
      // Is the left side an empty collection? "Contains" on empty collections is always false.
      if (left is Constant && ((Constant) left).Value is ICollection && ((ICollection) ((Constant) left).Value).Count == 0)
        _commandBuilder.AppendEvaluation (new Constant (false));
      else
      {
        _commandBuilder.AppendEvaluation (right);
        _commandBuilder.Append (" IN (");
        _commandBuilder.AppendEvaluation (left);
        _commandBuilder.Append (")");
      }
    }

    private void AppendGeneralCondition (BinaryCondition binaryCondition)
    {
      _commandBuilder.Append ("(");
      AppendNullChecks (binaryCondition.Left, binaryCondition.Right, binaryCondition.Kind);

      _commandBuilder.AppendEvaluation (binaryCondition.Left);
      _commandBuilder.Append (" ");
      AppendConditionKind (binaryCondition.Kind);
      _commandBuilder.Append (" ");
      _commandBuilder.AppendEvaluation (binaryCondition.Right);
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
            AppendNullChecksForEqualKinds (left, right);
            break;
          case BinaryCondition.ConditionKind.NotEqual:
            AppendNullChecksForNotEqualKind (left, right);
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
        case BinaryCondition.ConditionKind.Add:
          commandString = "+";
          break;
        case BinaryCondition.ConditionKind.Divide:
          commandString = "/";
          break;
        case BinaryCondition.ConditionKind.Modulo:
          commandString = "%";
          break;
        case BinaryCondition.ConditionKind.Multiply:
          commandString = "*";
          break;
        case BinaryCondition.ConditionKind.Subtract:
          commandString = "-";
          break;

        default:
          throw new NotSupportedException ("The binary condition kind " + kind + " is not supported.");
      }
      _commandBuilder.Append (commandString);
    }
  }
}