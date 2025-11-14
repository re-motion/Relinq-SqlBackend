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
using Microsoft.Data.SqlClient;

namespace Remotion.Linq.IntegrationTests.Common.Database
{
  public static class DatabaseConfiguration
  {
    public const string DefaultDatabaseDirectory = @"C:\Databases\";

    public const string DefaultDatabaseNamePrefix = "DBPrefix_";

    public static string DataSource
    {
      get { return ConfigurationManager.AppSettings["DataSource"]; }
    }

    public static string DatabaseDirectory
    {
      get { return ConfigurationManager.AppSettings["DatabaseDirectory"].TrimEnd('\\') + "\\"; }
    }

    public static string DatabaseNamePrefix
    {
      get { return ConfigurationManager.AppSettings["DatabaseNamePrefix"]; }
    }

    public static bool IntegratedSecurity
    {
      get { return Boolean.Parse (ConfigurationManager.AppSettings["IntegratedSecurity"]); }
    }

    public static string Username
    {
      get { return ConfigurationManager.AppSettings["Username"]; }
    }

    public static string Password
    {
      get { return ConfigurationManager.AppSettings["Password"]; }
    }

    public static string ReplaceDataSource (string connectionString)
    {
      var sqlConnectionStringBuilder = new SqlConnectionStringBuilder (connectionString);
      sqlConnectionStringBuilder.DataSource = DataSource;
      sqlConnectionStringBuilder.InitialCatalog = sqlConnectionStringBuilder.InitialCatalog.Replace (DefaultDatabaseNamePrefix, DatabaseNamePrefix);
      sqlConnectionStringBuilder.IntegratedSecurity = IntegratedSecurity;
      sqlConnectionStringBuilder.UserID = Username;
      sqlConnectionStringBuilder.Password = Password;
      return sqlConnectionStringBuilder.ConnectionString;
    }
  }
}