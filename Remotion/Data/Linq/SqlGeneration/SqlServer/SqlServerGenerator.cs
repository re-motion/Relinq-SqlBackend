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
using System.Linq;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  // If a fixedCommandBuilder is specified, the SqlServerGenerator can only be used to create one query from one thread. Otherwise, it is
  // stateless and can be used for multiple queries from multiple threads.
  public class SqlServerGenerator : SqlGeneratorBase<SqlServerGenerationContext>
  {
    public SqlServerGenerator (IDatabaseInfo databaseInfo)
      : this (databaseInfo, Parsing.ParseMode.TopLevelQuery)
    {
    }

    protected SqlServerGenerator (IDatabaseInfo databaseInfo, ParseMode parseMode)
      : base (databaseInfo, parseMode)
    {
      MethodCallRegistry.Register (typeof (string).GetMethod ("ToUpper", new Type[] { }), new MethodCallUpper ());
      MethodCallRegistry.Register (typeof (string).GetMethod ("Remove", new Type[] { typeof (int) }), new MethodCallRemove ());
      // TODO: register handler for string.Remove with two arguments
      MethodCallRegistry.Register (typeof (string).GetMethod ("ToLower", new Type[] { }), new MethodCallLower ());
      
      var methodCallConvertTo = new MethodCallConvertTo();
      foreach (var method in methodCallConvertTo.GetSupportedConvertMethods())
        MethodCallRegistry.Register (method, methodCallConvertTo);
      
      MethodCallRegistry.Register (typeof (string).GetMethod ("Substring", new Type[] { typeof (int), typeof (int) }), new MethodCallSubstring());
      
    }

    protected override SqlServerGenerationContext CreateContext ()
    {
      return new SqlServerGenerationContext (DatabaseInfo, MethodCallRegistry);
    }

    protected override IOrderByBuilder CreateOrderByBuilder (SqlServerGenerationContext context)
    {
      return new OrderByBuilder (context.CommandBuilder);
    }

    protected override IWhereBuilder CreateWhereBuilder (SqlServerGenerationContext context)
    {
      return new WhereBuilder (context.CommandBuilder, DatabaseInfo);
    }

    protected override IFromBuilder CreateFromBuilder (SqlServerGenerationContext context)
    {
      return new FromBuilder (context.CommandBuilder, DatabaseInfo);
    }

    protected override ISelectBuilder CreateSelectBuilder (SqlServerGenerationContext context)
    {
      return new SelectBuilder (context.CommandBuilder);
    }
  }
}
