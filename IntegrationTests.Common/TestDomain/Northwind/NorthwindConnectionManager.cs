// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Remotion.Linq.IntegrationTests.Common.Database;
using Remotion.Linq.LinqToSqlAdapter;

namespace Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind
{
  /// <summary>
  /// Provides functionality to open a connection to the Northwind Database
  /// </summary>
  public class NorthwindConnectionManager : IConnectionManager
  {
    public DbConnection Open()
    {
      string connectionString = GetConnectionString();
      var conn = new SqlConnection (connectionString);
      conn.Open();
      return conn;
    }

    public string GetConnectionString ()
    {
      ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["Northwind"];

      if (connectionSettings == null)
        throw new ArgumentNullException ("Connection configuration not found");

      return DatabaseConfiguration.ReplaceDataSource(connectionSettings.ConnectionString);
    }
  }
}