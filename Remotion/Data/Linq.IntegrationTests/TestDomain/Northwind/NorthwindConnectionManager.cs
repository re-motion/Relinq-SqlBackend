// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Remotion.Data.Linq.LinqToSqlAdapter;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  /// <summary>
  /// Provides functionality to open a connection to the Northwind Database
  /// </summary>
  public class NorthwindConnectionManager : IConnectionManager
  {
    public IDbConnection Open ()
    {
      ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["Northwind"];
      if(connectionSettings == null)
        throw new ArgumentNullException("Connection configuration not found");

      var conn = new SqlConnection (connectionSettings.ConnectionString);
      conn.Open();
      return conn;
    }
  }
}