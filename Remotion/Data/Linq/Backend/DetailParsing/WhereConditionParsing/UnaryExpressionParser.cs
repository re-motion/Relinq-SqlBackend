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
using System.Linq.Expressions;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.DetailParsing.WhereConditionParsing
{
  public class UnaryExpressionParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;

    public UnaryExpressionParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (UnaryExpression unaryExpression, ParseContext parseContext)
    {
      switch (unaryExpression.NodeType)
      {
        case ExpressionType.Not:
          return new NotCriterion (_parserRegistry.GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, parseContext));
        case ExpressionType.Convert: // Convert is simply ignored ATM, change to more sophisticated logic when needed
          return (_parserRegistry.GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, parseContext));
        default:
          throw new ParserException ("not or convert expression", unaryExpression.NodeType, "unary expression in where condition");
      }
    }

    public bool CanParse (Expression expression)
    {
      return expression is UnaryExpression;
    }

    public ICriterion Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((UnaryExpression) expression, parseContext);
    }
  }
}
