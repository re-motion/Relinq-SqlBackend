/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public abstract class SqlGeneratorBase<TContext> : ISqlGeneratorBase where TContext : ISqlGenerationContext
  {
    protected SqlGeneratorBase (IDatabaseInfo databaseInfo, ParseMode parseMode)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      DatabaseInfo = databaseInfo;
      ParseMode = parseMode;
      DetailParserRegistries = new DetailParserRegistries (DatabaseInfo, ParseMode);
    }

    public IDatabaseInfo DatabaseInfo { get; private set; }
    public ParseMode ParseMode { get; private set; }
    
    public DetailParserRegistries DetailParserRegistries { get; private set; }

    protected abstract TContext CreateContext ();

    public virtual CommandData BuildCommand (QueryModel queryModel)
    {
      SqlGenerationData sqlGenerationData = ProcessQuery (queryModel);

      TContext context = CreateContext ();
      CreateSelectBuilder (context).BuildSelectPart (sqlGenerationData.SelectEvaluations, sqlGenerationData.Distinct);
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
