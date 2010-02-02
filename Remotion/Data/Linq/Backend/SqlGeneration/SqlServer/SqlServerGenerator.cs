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
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators;
using System.Text;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer
{
  // If a fixed CommandBuilder is specified, the SqlServerGenerator can only be used to create one query from one thread. Otherwise, it is
  // stateless and can be used for multiple queries from multiple threads.
  public class SqlServerGenerator : SqlGeneratorBase<SqlServerGenerationContext>
  {
    public SqlServerGenerator (IDatabaseInfo databaseInfo)
        : this (databaseInfo, ParseMode.TopLevelQuery)
    {
    }

    protected SqlServerGenerator (IDatabaseInfo databaseInfo, ParseMode parseMode)
        : base (databaseInfo, parseMode)
    {
      MethodCallRegistry.Register (typeof (string).GetMethod ("ToUpper", new Type[] { }), new MethodCallUpper());
      MethodCallRegistry.Register (typeof (string).GetMethod ("Remove", new[] { typeof (int) }), new MethodCallRemove());
      // TODO: register handler for string.Remove with two arguments
      MethodCallRegistry.Register (typeof (string).GetMethod ("ToLower", new Type[] { }), new MethodCallLower());

      var methodCallConvertTo = new MethodCallConvertTo();
      foreach (var method in methodCallConvertTo.GetSupportedConvertMethods())
        MethodCallRegistry.Register (method, methodCallConvertTo);

      MethodCallRegistry.Register (typeof (string).GetMethod ("Substring", new[] { typeof (int), typeof (int) }), new MethodCallSubstring());
    }

    public virtual SqlServerGenerator CreateNestedSqlGenerator (ParseMode parseMode)
    {
      return new SqlServerGenerator (DatabaseInfo, parseMode);
    }

    protected override SqlServerGenerationContext CreateContext ()
    {
      return new SqlServerGenerationContext (this, DatabaseInfo, MethodCallRegistry);
    }

    public virtual SqlServerGenerationContext CreateDerivedContext (CommandBuilder commandBuilder)
    {
      var derivedCommandBuilder = new CommandBuilder (
          this, 
          commandBuilder.CommandText, 
          commandBuilder.CommandParameters, 
          DatabaseInfo, MethodCallRegistry);
      return new SqlServerGenerationContext (derivedCommandBuilder);
    }

    protected override IOrderByBuilder CreateOrderByBuilder (SqlServerGenerationContext context)
    {
      return new OrderByBuilder (context.CommandBuilder);
    }

    protected override IWhereBuilder CreateWhereBuilder (SqlServerGenerationContext context)
    {
      return new WhereBuilder (context.CommandBuilder);
    }

    protected override IFromBuilder CreateFromBuilder (SqlServerGenerationContext context)
    {
      return new FromBuilder (context.CommandBuilder);
    }

    protected override ISelectBuilder CreateSelectBuilder (SqlServerGenerationContext context)
    {
      return new SelectBuilder (context.CommandBuilder);
    }
  }
}
