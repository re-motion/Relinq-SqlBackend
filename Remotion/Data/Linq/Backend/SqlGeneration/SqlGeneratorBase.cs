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
using System.Collections.Generic;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.DetailParsing;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.SqlGeneration
{
  public abstract class SqlGeneratorBase<TContext> : ISqlGenerator
      where TContext: ISqlGenerationContext
  {
    protected SqlGeneratorBase (IDatabaseInfo databaseInfo, ParseMode parseMode)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      DatabaseInfo = databaseInfo;
      ParseMode = parseMode;
      DetailParserRegistries = new DetailParserRegistries (DatabaseInfo, ParseMode);
      MethodCallRegistry = new MethodCallSqlGeneratorRegistry();
    }

    public IDatabaseInfo DatabaseInfo { get; private set; }
    public ParseMode ParseMode { get; private set; }

    public DetailParserRegistries DetailParserRegistries { get; private set; }
    public MethodCallSqlGeneratorRegistry MethodCallRegistry { get; private set; }

    protected abstract TContext CreateContext ();

    public virtual CommandData BuildCommand (QueryModel queryModel)
    {
      TContext context = CreateContext();
      return BuildCommand(queryModel, context);
    }

    public virtual CommandData BuildCommand (QueryModel queryModel, TContext context)
    {
      var sqlGenerationData = ProcessQuery (queryModel);
      if (sqlGenerationData.SelectEvaluation == null)
        throw new InvalidOperationException ("The concrete subclass did not set a select evaluation.");

      CreateSelectBuilder (context).BuildSelectPart (sqlGenerationData);
      CreateFromBuilder (context).BuildFromPart (sqlGenerationData);
      CreateWhereBuilder (context).BuildWherePart (sqlGenerationData);
      CreateOrderByBuilder (context).BuildOrderByPart (sqlGenerationData);

      return new CommandData (context.CommandText, context.CommandParameters, sqlGenerationData);
    }

   protected virtual SqlGenerationData ProcessQuery (QueryModel queryModel)
    {
      ParseContext parseContext = CreateParseContext (queryModel);
      var visitor = new SqlGeneratorVisitor (DatabaseInfo, ParseMode, DetailParserRegistries, parseContext);
      queryModel.Accept (visitor);
      parseContext.JoinedTableContext.CreateAliases (queryModel);
      return visitor.SqlGenerationData;
    }

    protected virtual ParseContext CreateParseContext (QueryModel queryModel)
    {
      var joinedTableContext = new JoinedTableContext (DatabaseInfo);
      return new ParseContext (queryModel, new List<FieldDescriptor>(), joinedTableContext);
    }

    protected abstract IOrderByBuilder CreateOrderByBuilder (TContext context);
    protected abstract IWhereBuilder CreateWhereBuilder (TContext context);
    protected abstract IFromBuilder CreateFromBuilder (TContext context);
    protected abstract ISelectBuilder CreateSelectBuilder (TContext context);
  }
}
