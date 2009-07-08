// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators;

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
      MethodCallRegistry.Register (typeof (string).GetMethod ("Remove", new Type[] { typeof (int) }), new MethodCallRemove());
      // TODO: register handler for string.Remove with two arguments
      MethodCallRegistry.Register (typeof (string).GetMethod ("ToLower", new Type[] { }), new MethodCallLower());

      var methodCallConvertTo = new MethodCallConvertTo();
      foreach (var method in methodCallConvertTo.GetSupportedConvertMethods())
        MethodCallRegistry.Register (method, methodCallConvertTo);

      MethodCallRegistry.Register (typeof (string).GetMethod ("Substring", new Type[] { typeof (int), typeof (int) }), new MethodCallSubstring());

      var methodInfoCount = (from m in typeof (Queryable).GetMethods()
                             where m.Name == "Count" && m.GetParameters().Length == 1
                             select m).Single();
      MethodCallRegistry.Register (methodInfoCount, new MethodCallCount());

      var methodInfoDistinct = (from m in typeof (Queryable).GetMethods()
                                where m.Name == "Distinct" && (m.GetParameters().Length == 1)
                                select m).Single();
      MethodCallRegistry.Register (methodInfoDistinct, new MethodCallDistinct());

      var methodInfoSingleOneParameter = (from m in typeof (Queryable).GetMethods()
                                          where m.Name == "Single" && m.GetParameters().Length == 1
                                          select m).Single();
      MethodCallRegistry.Register (methodInfoSingleOneParameter, new MethodCallSingle());

      var methodInfoSingleTwoParameters = (from m in typeof (Queryable).GetMethods()
                                           where m.Name == "Single" && m.GetParameters().Length == 2
                                           select m).Single();
      MethodCallRegistry.Register (methodInfoSingleTwoParameters, new MethodCallSingle());

      var methodInfoFirst = (from m in typeof (Queryable).GetMethods()
                             where m.Name == "First" && m.GetParameters().Length == 1
                             select m).Single();
      MethodCallRegistry.Register (methodInfoFirst, new MethodCallFirst());

      var methodInfoTake = (from m in typeof (Queryable).GetMethods()
                            where m.Name == "Take" && m.GetParameters().Length == 2
                            select m).Single();
      MethodCallRegistry.Register (methodInfoTake, new MethodCallTake());
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