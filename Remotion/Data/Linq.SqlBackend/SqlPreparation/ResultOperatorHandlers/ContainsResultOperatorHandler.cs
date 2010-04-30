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
using System.Collections;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Handles the <see cref="ContainsResultOperator"/> by generating a SQL IN expression.
  /// </summary>
  public class ContainsResultOperatorHandler : ResultOperatorHandler<ContainsResultOperator>
  {
    public override void HandleResultOperator (
        ContainsResultOperator resultOperator,
        QueryModel queryModel,
        SqlStatementBuilder sqlStatementBuilder,
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);

      Expression selectProjection;
      var dataInfo = sqlStatementBuilder.DataInfo;
      var preparedItemExpression = stage.PrepareItemExpression (resultOperator.Item);

      var constantCollection = GetConstantCollectionValue (queryModel);
      if (constantCollection != null) // Scenario: where new[] { 1, 2, 3 }.Contains (...)
      {
        if (queryModel.ResultOperators.Count > 1)
        {
          throw new NotSupportedException (
              "Queries with more than one results operators are not allowed when using Contains in conjunction with a constant collection.");
        }

        selectProjection = constantCollection.Count > 0
                               ? (Expression) new SqlBinaryOperatorExpression ("IN", preparedItemExpression, Expression.Constant (constantCollection))
                               : Expression.Constant (false);

        sqlStatementBuilder.SqlTables.Clear ();
      }
      else // Scenario: where kitchen.Cooks.Contains (...)
      {
        var sqlSubStatement = sqlStatementBuilder.GetStatementAndResetBuilder ();
        var subStatementExpression = new SqlSubStatementExpression (sqlSubStatement);

        selectProjection = new SqlBinaryOperatorExpression ("IN", preparedItemExpression, subStatementExpression);
      }

      sqlStatementBuilder.SelectProjection = selectProjection;
      UpdateDataInfo (resultOperator, sqlStatementBuilder, dataInfo);
    }

    private ICollection GetConstantCollectionValue (QueryModel queryModel)
    {
      var fromExpressionAsConstant = (queryModel.MainFromClause.FromExpression) as ConstantExpression;
      if (queryModel.IsIdentityQuery () && fromExpressionAsConstant != null && typeof (ICollection).IsAssignableFrom (fromExpressionAsConstant.Type))
        return (ICollection) fromExpressionAsConstant.Value;
      else
        return null;
    }
  }
}