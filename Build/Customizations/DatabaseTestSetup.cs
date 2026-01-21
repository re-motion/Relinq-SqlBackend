// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.IO;
using Nuke.Common;
using Remotion.BuildScript;
using Remotion.BuildScript.Test;
using Remotion.BuildScript.Test.Dimensions;
using Serilog;

namespace Customizations;

public class DatabaseTestSetup : ITestExecutionWrapper, IRequiresTestParameters
{
  private const string c_databaseConnectionStringParameterNameTemplate = "DatabaseConnectionString{0}";
  private const string c_databaseNamePrefixParameterNameTemplate = "DatabaseNamePrefixMsSql{0}";

  private const string c_defaultConnectionString = "Data Source=localhost;TrustServerCertificate=true;Integrated Security=true";

  public void ConfigureTestParameters (TestParameterBuilder builder)
  {
    foreach (var sqlServer in builder.EnabledTestDimensions.OfType<Databases>())
    {
      if (!sqlServer.HasSpecificVersion)
        continue;

      builder.AddOptionalParameter(string.Format(c_databaseConnectionStringParameterNameTemplate, sqlServer.Version), "");
      builder.AddOptionalParameter(string.Format(c_databaseNamePrefixParameterNameTemplate, sqlServer.Version), "");
    }
  }

  public void ExecuteTests (TestExecutionContext context, Action<TestExecutionContext> next)
  {
    var sqlServer = context.TestMatrixRow.GetDimension<Databases>();
    var connectionString = context.GetTestParameter(string.Format(c_databaseConnectionStringParameterNameTemplate, sqlServer.Version));
    var databaseNamePrefix = context.GetTestParameter(string.Format(c_databaseNamePrefixParameterNameTemplate, sqlServer.Version));
    if (string.IsNullOrEmpty(connectionString))
    {
      if (sqlServer == Databases.SqlServerDefault)
      {
        connectionString = c_defaultConnectionString;
      }
      else
      {
        Assert.True(sqlServer.HasSpecificVersion);
        
        throw new InvalidOperationException($"Connectionstring required for {sqlServer}");
      }
    }

    var configuration = context.TestMatrixRow.GetDimension<Configurations>().Value;
    var targetFramework = context.TestMatrixRow.GetDimension<TargetFrameworks>().Identifier;

    var assemblyName = context.Project.GetMetadata(RemotionBuildMetadataProperties.AssemblyName);

    var configFile = context.Project.FolderPath / "bin" / configuration / targetFramework / $"{assemblyName}.dll.config";
    Assert.FileExists(configFile);

    var appConfig = AppConfig.Read(configFile);

    Log.Information("Updating Database Test configuration file:");
    Log.Information($" - Connection string: '{connectionString}'");
    Log.Information($" - Database name prefix: '{databaseNamePrefix}'");

    appConfig.SetAppSetting("ConnectionString", connectionString);
    appConfig.SetAppSetting("DatabaseNamePrefix", databaseNamePrefix);

    appConfig.WriteToFile(configFile);
    File.Copy(configFile, configFile.Parent / "testhost.dll.config", true);
    File.Copy(configFile, configFile.Parent / "testhost.x86.dll.config", true);

    next(context);
  }
}
