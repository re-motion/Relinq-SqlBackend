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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Utilities;

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

      var subQueryModel = subQueryExpression.QueryModel;
      var containsResultOperator = subQueryModel.ResultOperators.LastOrDefault() as ContainsResultOperator;
      if (containsResultOperator != null)
        return ParseSubQueryWithContainsOperator(subQueryModel, containsResultOperator, parseContext);
      else
        return new SubQuery (subQueryModel, ParseMode.SubQueryInWhere, null);
    }

    private ICriterion ParseSubQueryWithContainsOperator (QueryModel subQueryModel, ContainsResultOperator containsResultOperator, ParseContext parseContext)
    {
      var item = _parserRegistry.GetParser (containsResultOperator.Item).Parse (containsResultOperator.Item, parseContext);

      var queryModelClone = subQueryModel.Clone ();
      queryModelClone.ResultOperators.RemoveAt (queryModelClone.ResultOperators.Count - 1);

      var constantFromExpression = GetConstantFromExpression (queryModelClone);
      if (constantFromExpression != null)
      {
        var constantValue = _parserRegistry.GetParser (constantFromExpression).Parse (constantFromExpression, parseContext);
        return new BinaryCondition (constantValue, item, BinaryCondition.ConditionKind.Contains);
      }
      else
      {
        return new BinaryCondition (new SubQuery (queryModelClone, ParseMode.SubQueryInWhere, null), item, BinaryCondition.ConditionKind.Contains);
      }
    }

    private ConstantExpression GetConstantFromExpression (QueryModel subQueryModel)
    {
      if (subQueryModel.MainFromClause.FromExpression is ConstantExpression
          && !typeof (IQueryable).IsAssignableFrom (subQueryModel.MainFromClause.FromExpression.Type)
          && subQueryModel.BodyClauses.Count == 0
          && subQueryModel.ResultOperators.Count == 0
          && subQueryModel.SelectClause.Selector is QuerySourceReferenceExpression
          && ((QuerySourceReferenceExpression) subQueryModel.SelectClause.Selector).ReferencedQuerySource == subQueryModel.MainFromClause)
      {
        return (ConstantExpression) subQueryModel.MainFromClause.FromExpression;
      }
      else
      {
        return null;
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
