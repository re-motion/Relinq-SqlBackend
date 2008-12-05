// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.Generic;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public abstract class SqlGeneratorBase<TContext> : ISqlGenerator where TContext : ISqlGenerationContext
  {
    protected SqlGeneratorBase (IDatabaseInfo databaseInfo, ParseMode parseMode)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      DatabaseInfo = databaseInfo;
      ParseMode = parseMode;
      DetailParserRegistries = new DetailParserRegistries (DatabaseInfo, ParseMode);
      MethodCallRegistry = new MethodCallSqlGeneratorRegistry ();
    }

    public IDatabaseInfo DatabaseInfo { get; private set; }
    public ParseMode ParseMode { get; private set; }
    
    public DetailParserRegistries DetailParserRegistries { get; private set; }
    public MethodCallSqlGeneratorRegistry MethodCallRegistry {get; private set; }
    

    protected abstract TContext CreateContext ();

    public virtual CommandData BuildCommand (QueryModel queryModel)
    {
      SqlGenerationData sqlGenerationData = ProcessQuery (queryModel);

      TContext context = CreateContext ();
      IEvaluation selectEvaluation = sqlGenerationData.SelectEvaluation;
      if (selectEvaluation == null)
        throw new InvalidOperationException ("The concrete subclass did not set a select evaluation.");

      CreateSelectBuilder (context).BuildSelectPart (selectEvaluation, sqlGenerationData.ResultModifiers);
      CreateFromBuilder (context).BuildFromPart (sqlGenerationData.FromSources, sqlGenerationData.Joins);
      CreateFromBuilder (context).BuildLetPart (sqlGenerationData.LetEvaluations);
      CreateWhereBuilder (context).BuildWherePart (sqlGenerationData.Criterion);
      CreateOrderByBuilder (context).BuildOrderByPart (sqlGenerationData.OrderingFields);

      return new CommandData (context.CommandText, context.CommandParameters, sqlGenerationData);
    }

    protected virtual SqlGenerationData ProcessQuery (QueryModel queryModel)
    {
      JoinedTableContext joinedTableContext = new JoinedTableContext();
      ParseContext parseContext = new ParseContext (queryModel, queryModel.GetExpressionTree(), new List<FieldDescriptor>(), joinedTableContext);
      SqlGeneratorVisitor visitor = new SqlGeneratorVisitor (DatabaseInfo, ParseMode, DetailParserRegistries, parseContext);
      queryModel.Accept (visitor);
      joinedTableContext.CreateAliases();
      return visitor.SqlGenerationData;
    }

    protected abstract IOrderByBuilder CreateOrderByBuilder (TContext context);
    protected abstract IWhereBuilder CreateWhereBuilder (TContext context);
    protected abstract IFromBuilder CreateFromBuilder (TContext context);
    protected abstract ISelectBuilder CreateSelectBuilder (TContext context);
  }
}
