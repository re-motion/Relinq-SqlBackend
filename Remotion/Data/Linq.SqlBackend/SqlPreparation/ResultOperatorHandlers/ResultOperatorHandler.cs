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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  /// <summary>
  /// Default implementation of <see cref="IResultOperatorHandler"/> providing commonly needed functionality.
  /// </summary>
  /// <typeparam name="T">The result operator type handled by the concrete subclass of <see cref="ResultOperatorHandler{T}"/>.</typeparam>
  public abstract class ResultOperatorHandler<T> : IResultOperatorHandler where T: ResultOperatorBase
  {
    protected abstract void HandleResultOperator (T resultOperator, ref SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage);

    // TODO Review 2620: Make two methods of this: EnsureNoTopExpression, UpdateDataInfo
    // TODO Review 2620: Add unit tests for these two methods (add a ResultOperatorHandlerTest and a TestResultOperatorHandler)
    protected void EnsureNoTopExpressionAndSetDataInfo (ResultOperatorBase resultOperator, ref SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage)
    {
      // TODO Review 2620: argument checks

      if (sqlStatementBuilder.TopExpression != null)
      {
        var sqlStatement = GetStatementAndResetBuilder (ref sqlStatementBuilder);
        sqlStatementBuilder = new SqlStatementBuilder (); // TODO Review 2620: this is already done by GetStatementAndResetBuilder 

        var subStatementTableInfo = new ResolvedSubStatementTableInfo (
            generator.GetUniqueIdentifier ("q"),
            sqlStatement);
        var sqlTable = new SqlTable (subStatementTableInfo);

        sqlStatementBuilder.SqlTables.Add (sqlTable);
        sqlStatementBuilder.SelectProjection = new SqlTableReferenceExpression (sqlTable);
        // the new statement is an identity query that selects the result of its subquery, so it starts with the same data type
        sqlStatementBuilder.DataInfo = sqlStatement.DataInfo;
      }

      sqlStatementBuilder.DataInfo = resultOperator.GetOutputDataInfo (sqlStatementBuilder.DataInfo);
    }

    // TODO Review 2620: Make this an explicit interface implementation; make the abstract method public
    public void HandleResultOperator (ResultOperatorBase resultOperator, ref SqlStatementBuilder sqlStatementBuilder, UniqueIdentifierGenerator generator, ISqlPreparationStage stage)
    {
      // TODO Review 2620: Argument checks. Also check type of resultOperator: var castOperator = ArgumentUtility.CheckNotNullAndType<T> (resultOperator)
      HandleResultOperator ((T) resultOperator, ref sqlStatementBuilder, generator, stage);
    }

    // TODO Review 2620: Probably not necessary to make this virtual
    protected virtual SqlStatement GetStatementAndResetBuilder (ref SqlStatementBuilder sqlStatementBuilder)
    {
      var sqlSubStatement = sqlStatementBuilder.GetSqlStatement ();
      sqlStatementBuilder = new SqlStatementBuilder ();
      return sqlSubStatement;
    }
  }
}