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
using Remotion.Data.Linq.Clauses.StreamedData;
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
      var fromExpression = queryModel.MainFromClause.FromExpression as ConstantExpression;
      IStreamedDataInfo dataInfo = sqlStatementBuilder.DataInfo;

      if (queryModel.IsIdentityQuery () && fromExpression != null && typeof (ICollection).IsAssignableFrom (fromExpression.Type))
      {
        if (queryModel.ResultOperators.Count > 1)
          throw new NotSupportedException ("Expression with more than one results operators are not allowed when using contains.");

        if (((ICollection) fromExpression.Value).Count > 0)
        {
          var preparedItemExpression = stage.PrepareItemExpression (resultOperator.Item);
          selectProjection = new SqlBinaryOperatorExpression ("IN", preparedItemExpression, fromExpression);
        }
        else
        {
          selectProjection = Expression.Constant (false);
        }

        sqlStatementBuilder.SqlTables.Clear ();
      }
      else
      {
        var sqlSubStatement = sqlStatementBuilder.GetStatementAndResetBuilder();
        var subStatementExpression = new SqlSubStatementExpression (sqlSubStatement);
        var preparedItemExpression = stage.PrepareItemExpression (resultOperator.Item);
        selectProjection = new SqlBinaryOperatorExpression ("IN", preparedItemExpression, subStatementExpression);
      }

      sqlStatementBuilder.SelectProjection = selectProjection;
      UpdateDataInfo (resultOperator, sqlStatementBuilder, dataInfo);
    }
  }
}