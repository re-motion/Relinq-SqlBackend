// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Generates SQL text for <see cref="BinaryExpression"/> instances.
  /// </summary>
  public class BinaryExpressionTextGenerator
  {
    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly ExpressionVisitor _expressionVisitor;
    private readonly Dictionary<ExpressionType, string> _simpleOperatorRegistry;

    public BinaryExpressionTextGenerator (ISqlCommandBuilder commandBuilder, ExpressionVisitor expressionVisitor)
    {
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);
      ArgumentUtility.CheckNotNull (nameof(expressionVisitor), expressionVisitor);

      _commandBuilder = commandBuilder;
      _expressionVisitor = expressionVisitor;

      _simpleOperatorRegistry = new Dictionary<ExpressionType, string>
                                {
                                    { ExpressionType.Add, "+" },           
                                    { ExpressionType.AddChecked, "+" },    
                                    { ExpressionType.And, "&" },           
                                    { ExpressionType.AndAlso, "AND" },     
                                    { ExpressionType.Divide, "/" },        
                                    { ExpressionType.ExclusiveOr, "^" },   
                                    { ExpressionType.GreaterThan, ">" },   
                                    { ExpressionType.GreaterThanOrEqual, ">=" }, 
                                    { ExpressionType.LessThan, "<" },            
                                    { ExpressionType.LessThanOrEqual, "<=" },    
                                    { ExpressionType.Modulo, "%" },              
                                    { ExpressionType.Multiply, "*" },            
                                    { ExpressionType.MultiplyChecked, "*" },     
                                    { ExpressionType.Or, "|" },                  
                                    { ExpressionType.OrElse, "OR" },             
                                    { ExpressionType.Subtract, "-" },            
                                    { ExpressionType.SubtractChecked, "-" },     
                                    { ExpressionType.Equal, "=" },
                                    { ExpressionType.NotEqual, "<>" }            
                                };
    }

    public void GenerateSqlForBinaryExpression (BinaryExpression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Coalesce:
          GenerateSqlForPrefixOperator ("COALESCE", expression.Left, expression.Right);
          break;
        case ExpressionType.Power:
          GenerateSqlForPrefixOperator ("POWER", expression.Left, expression.Right);
          break;
        default:
          GenerateSqlForInfixOperator (expression.Left, expression.Right, expression.NodeType, expression.Type);
          break;
      }
    }

    private void GenerateSqlForPrefixOperator (string sqlOperatorString, Expression left, Expression right)
    {
      _commandBuilder.Append (sqlOperatorString);
      _commandBuilder.Append (" (");
      _expressionVisitor.Visit (left);
      _commandBuilder.Append (", ");
      _expressionVisitor.Visit (right);
      _commandBuilder.Append (")");
    }

    private void GenerateSqlForInfixOperator (Expression left, Expression right, ExpressionType nodeType, Type expressionType)
    {
      if (nodeType == ExpressionType.And && BooleanUtility.IsBooleanType (expressionType))
        GenerateSqlForInfixOperator (left, right, ExpressionType.AndAlso, expressionType);
      else if (nodeType == ExpressionType.Or && BooleanUtility.IsBooleanType (expressionType))
        GenerateSqlForInfixOperator (left, right, ExpressionType.OrElse, expressionType);
      else if (nodeType == ExpressionType.ExclusiveOr && BooleanUtility.IsBooleanType (expressionType))
      {
        // SQL has no logical XOR operator, so we simulate: a XOR b <=> (a AND NOT b) OR (NOT a AND b)
        var exclusiveOrSimulationExpression = Expression.OrElse (
            Expression.AndAlso (left, Expression.Not (right)), 
            Expression.AndAlso (Expression.Not (left), right));
        _expressionVisitor.Visit (exclusiveOrSimulationExpression);
      }
      else
      {
        string operatorString = GetRegisteredOperatorString (nodeType);

        _expressionVisitor.Visit (left);
        _commandBuilder.Append (" ");
        _commandBuilder.Append (operatorString);
        _commandBuilder.Append (" ");
        _expressionVisitor.Visit (right);
      }
    }

    private string GetRegisteredOperatorString (ExpressionType nodeType)
    {
        string operatorString;
        if (!_simpleOperatorRegistry.TryGetValue (nodeType, out operatorString))
          throw new NotSupportedException ("The binary operator '" + nodeType + "' is not supported.");
        return operatorString;
    }
   
  }
}