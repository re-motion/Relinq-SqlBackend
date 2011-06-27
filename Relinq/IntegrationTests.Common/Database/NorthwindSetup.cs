// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System;
using System.Configuration;
using System.IO;
using Remotion.Linq.IntegrationTests.Common.Utilities;

namespace Remotion.Linq.IntegrationTests.Common.Database
{
  /// <summary>
  /// Sets up the northwind database.
  /// </summary>
  public static class NorthwindSetup
  {
    public static void SetupDatabase ()
    {
      ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["Master"];
      if (connectionSettings == null)
        throw new InvalidOperationException ("Master connection configuration not found. Cannot set up database.");

      var connectionString = connectionSettings.ConnectionString;
      var databaseAgent = new DatabaseAgent (connectionString);

      var commandBatch = File.ReadAllText ("Database/Northwnd.sql");

      databaseAgent.ExecuteBatchString (commandBatch, false);
    }
  }
}