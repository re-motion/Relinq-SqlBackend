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
using System.Configuration;
using System.Reflection;
using Remotion.Development.UnitTesting.Resources;
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

      var resourceID = typeof (NorthwindSetup).Namespace + ".Northwnd.sql";
      var commandBatch = ResourceUtility.GetResourceString (Assembly.GetExecutingAssembly(), resourceID);

      databaseAgent.ExecuteBatchString (commandBatch, false);
    }
  }
}