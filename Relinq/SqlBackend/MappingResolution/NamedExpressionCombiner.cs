// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="NamedExpressionCombiner"/> analyzes the inner expression of a <see cref="NamedExpression"/> and returns a combined expression 
  /// if possible. A combined expression is an expression equivalent to the inner expression but with the name included.
  /// The process of combining named expressions must be performed during the mapping resolution stage because the name of an entity or value in
  /// a sub-statement must be defined before an outer statement's reference to that entity or value is resolved by 
  /// <see cref="SubStatementReferenceResolver"/>.
  /// </summary>
  public class NamedExpressionCombiner
  {
    public static Expression ProcessNames (IMappingResolutionContext mappingResolutionContext, NamedExpression outerExpression)
    {
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);
      ArgumentUtility.CheckNotNull ("outerExpression", outerExpression);

      // We cannot implement this as an expression visitor because expression visitors have no fallback case, i.e., there is no good possibility
      // to catch all cases not explicitly handled by a visitor. We need that catch-all case, however, and don't want to automatically visit the
      // expressions' children.

      if (outerExpression.Expression is NewExpression)
      {
        var newExpression = (NewExpression) outerExpression.Expression;
        var preparedArguments = newExpression.Arguments.Select (expr => ProcessNames (mappingResolutionContext, new NamedExpression (outerExpression.Name, expr)));

        if (newExpression.Members != null && newExpression.Members.Count>0)
          return Expression.New (newExpression.Constructor, preparedArguments, newExpression.Members);
        else
          return Expression.New (newExpression.Constructor, preparedArguments);
      }
      else if (outerExpression.Expression is SqlEntityExpression)
      {
        var entityExpression = (SqlEntityExpression) outerExpression.Expression;
        string newName = CombineNames (outerExpression.Name, entityExpression.Name);

        return mappingResolutionContext.UpdateEntityAndAddMapping (entityExpression, entityExpression.Type, entityExpression.TableAlias, newName);
      }
      else if (outerExpression.Expression is NamedExpression)
      {
        var namedExpression = (NamedExpression) outerExpression.Expression;
        var newName = CombineNames (outerExpression.Name, namedExpression.Name);
        return ProcessNames (mappingResolutionContext, new NamedExpression (newName, namedExpression.Expression));
      }
      else if (outerExpression.Expression is SqlGroupingSelectExpression)
      {
        var groupingSelectExpression = (SqlGroupingSelectExpression) outerExpression.Expression;
        var newKeyExpression = ProcessNames (
            mappingResolutionContext, 
            new NamedExpression (outerExpression.Name, groupingSelectExpression.KeyExpression));
        var newElementExpression = ProcessNames (
            mappingResolutionContext,
            new NamedExpression (outerExpression.Name, groupingSelectExpression.ElementExpression));
        var newAggregationExpressions = groupingSelectExpression.AggregationExpressions.Select (
            e => ProcessNames (mappingResolutionContext, new NamedExpression (outerExpression.Name, e)));
        return mappingResolutionContext.UpdateGroupingSelectAndAddMapping (groupingSelectExpression, newKeyExpression, newElementExpression, newAggregationExpressions);
      }
      else if (outerExpression.Expression is ConvertedBooleanExpression)
      {
        var convertedBooleanExpression = (ConvertedBooleanExpression) outerExpression.Expression;
        var innerNamedExpression = new NamedExpression (outerExpression.Name, convertedBooleanExpression.Expression);
        return new ConvertedBooleanExpression (ProcessNames (mappingResolutionContext, innerNamedExpression));
      }
      else if (outerExpression.Expression.NodeType == ExpressionType.Convert || outerExpression.Expression.NodeType == ExpressionType.ConvertChecked)
      {
        var unaryExpression = (UnaryExpression) outerExpression.Expression;
        var innerNamedExpression = new NamedExpression (outerExpression.Name, unaryExpression.Operand);
        return Expression.MakeUnary (
            unaryExpression.NodeType, 
            ProcessNames (mappingResolutionContext, innerNamedExpression), 
            unaryExpression.Type, 
            unaryExpression.Method);
      }
      else
        return outerExpression;
    }

    private static string CombineNames (string name1, string name2)
    {
      if (name1 == null)
        return name2;
      
      if (name2 == null)
        return name1;
      
      return name1 + "_" + name2;
    }
  }
}