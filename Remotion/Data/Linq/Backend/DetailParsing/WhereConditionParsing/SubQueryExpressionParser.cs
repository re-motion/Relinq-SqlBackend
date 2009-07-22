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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Backend.DetailParsing.WhereConditionParsing
{
  public class SubQueryExpressionParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;

    public SubQueryExpressionParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (SubQueryExpression subQueryExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("subQueryExpression", subQueryExpression);

      var containsResultOperator = subQueryExpression.QueryModel.ResultOperators.LastOrDefault() as ContainsResultOperator;
      if (containsResultOperator != null)
      {
        var queryModelClone = subQueryExpression.QueryModel.Clone();
        queryModelClone.ResultOperators.RemoveAt (queryModelClone.ResultOperators.Count - 1);
        var item = _parserRegistry.GetParser (containsResultOperator.Item).Parse (containsResultOperator.Item, parseContext);

        return new ContainsCriterion (new SubQuery (queryModelClone, ParseMode.SubQueryInWhere, null), item);
      }
      else
      {
        return new SubQuery (subQueryExpression.QueryModel, ParseMode.SubQueryInWhere, null);
      }
    }

    public bool CanParse (Expression expression)
    {
      return expression is SubQueryExpression;
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((SubQueryExpression) expression, parseContext);
    }
  }
}