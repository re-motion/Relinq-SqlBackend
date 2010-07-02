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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  public class SqlGeneratingSelectExpressionVisitor : SqlGeneratingExpressionVisitor
  {
    public static new void GenerateSql (Expression expression, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlGeneratingSelectExpressionVisitor (commandBuilder, stage);
      visitor.VisitExpression (expression);
    }

    protected SqlGeneratingSelectExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
        : base(commandBuilder, stage)
    {
    }

    public override Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      VisitExpression (expression.Expression);
      CommandBuilder.Append (" AS ");
      CommandBuilder.AppendIdentifier (expression.Name ?? "value");
      
      return expression;
    }

    protected override void AppendColumnForEntity (SqlEntityExpression entity, SqlColumnExpression column)
    {
      column.Accept (this);
      if (column.ColumnName != "*")
      {
        if (entity.Name != null)
        {
          CommandBuilder.Append (" AS ");
          CommandBuilder.AppendIdentifier (entity.Name + "_" + column.ColumnName);
        }
        else if ((entity is SqlEntityReferenceExpression) && ((SqlEntityReferenceExpression) entity).ReferencedEntity.Name != null)
        {
          // entity references without a name that point to an entity with a name must assign aliases to their columns;
          // otherwise, their columns would include the referenced entity's name
          CommandBuilder.Append (" AS ");
          CommandBuilder.AppendIdentifier (column.ColumnName);
        }
      }
    }
  }
}