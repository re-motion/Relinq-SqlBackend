// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Generates SQL text for <see cref="BinaryExpression"/> instances.
  /// </summary>
  public class BinaryExpressionTextGenerator
  {
    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly ExpressionTreeVisitor _expressionVisitor;
    private readonly Dictionary<ExpressionType, string> _simpleOperatorRegistry;

    public BinaryExpressionTextGenerator (ISqlCommandBuilder commandBuilder, ExpressionTreeVisitor expressionVisitor)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expressionVisitor", expressionVisitor);

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
                                    { ExpressionType.Coalesce, "COALESCE" },            
                                    { ExpressionType.Power, "POWER" },
                                    { ExpressionType.Equal, "=" },
                                    { ExpressionType.NotEqual, "<>" }            
                                };
    }

    public void GenerateSqlForBinaryExpression (BinaryExpression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Equal:
          GenerateSqlForInfixOperator (expression.Left, expression.Right, expression.NodeType);
          break;
        case ExpressionType.NotEqual:
          GenerateSqlForInfixOperator (expression.Left, expression.Right, expression.NodeType);
          break;
        case ExpressionType.Coalesce:
        case ExpressionType.Power:
          GenerateSqlForPrefixOperator (expression.Left, expression.Right, expression.NodeType);
          break;
        default:
          GenerateSqlForInfixOperator (expression.Left, expression.Right, expression.NodeType);
          break;
      }
    }

    private void GenerateSqlForPrefixOperator (Expression left, Expression right, ExpressionType nodeType)
    {
      string operatorString = GetRegisteredOperatorString (nodeType);
      _commandBuilder.Append (operatorString);
      _commandBuilder.Append (" (");
      _expressionVisitor.VisitExpression (left);
      _commandBuilder.Append (", ");
      _expressionVisitor.VisitExpression (right);
      _commandBuilder.Append (")");
    }

    private void GenerateSqlForInfixOperator (Expression left, Expression right, ExpressionType nodeType)
    {
      string operatorString = GetRegisteredOperatorString(nodeType);

      _expressionVisitor.VisitExpression (left);
      _commandBuilder.Append (" ");
      _commandBuilder.Append (operatorString);
      _commandBuilder.Append (" ");
      _expressionVisitor.VisitExpression (right);
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