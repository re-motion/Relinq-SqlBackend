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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Handles the <see cref="ContainsResultOperator"/> by generating a SQL IN expression.
  /// </summary>
  public class ContainsResultOperatorHandler : ResultOperatorHandler<ContainsResultOperator>
  {
    public override void HandleResultOperator (ContainsResultOperator resultOperator, SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage, ISqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("sqlStatementBuilder", sqlStatementBuilder);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      var dataInfo = sqlStatementBuilder.DataInfo;
      var preparedItemExpression = stage.PrepareResultOperatorItemExpression (resultOperator.Item, context);
      // No name required for the select projection inside of an IN expression
      // (If the expression is a constant collection, a name would even be fatal.)
      var sqlSubStatement = sqlStatementBuilder.GetStatementAndResetBuilder ();
      var subStatementExpression = sqlSubStatement.CreateExpression();
      
      sqlStatementBuilder.SelectProjection = new SqlInExpression (typeof(bool), preparedItemExpression, subStatementExpression);
      
      UpdateDataInfo (resultOperator, sqlStatementBuilder, dataInfo);
    }
  }
}