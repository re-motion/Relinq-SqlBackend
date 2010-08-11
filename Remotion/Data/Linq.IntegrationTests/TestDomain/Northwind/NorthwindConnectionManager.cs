// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Remotion.Data.Linq.LinqToSqlAdapter.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  public class NorthwindConnectionManager : IConnectionManager
  {
    public IDbConnection Open ()
    {
      ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["Northwind"];
      if(connectionSettings == null)
      {
        throw new ArgumentNullException("Connection configuration not found");
      }

      var conn = new SqlConnection (connectionSettings.ConnectionString);
      conn.Open();
      return conn;
    }
  }
}