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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Text;
using Remotion.Utilities;

namespace Remotion.Linq.IntegrationTests.Common.Utilities
{
  // Copied from Remotion.Development.UnitTesting.Data.SqlClient

  /// <summary>Use the <see cref="DatabaseAgent"/> for setting up the database during unit testing.</summary>
  public class DatabaseAgent
  {
    private readonly string _connectionString;
    private string _fileName = null;

    public DatabaseAgent (string connectionString)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(connectionString), connectionString);

      _connectionString = connectionString;
    }

    public void SetDatabaseReadWrite (string database)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(database), database);
      ExecuteCommand (string.Format ("ALTER DATABASE [{0}] SET READ_WRITE WITH ROLLBACK IMMEDIATE", database));
    }

    public void SetDatabaseReadOnly (string database)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(database), database);
      ExecuteCommand (string.Format ("ALTER DATABASE [{0}] SET READ_ONLY WITH ROLLBACK IMMEDIATE", database));
    }

    public int ExecuteBatchFile (string sqlFileName, bool useTransaction)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(sqlFileName), sqlFileName);

      _fileName = sqlFileName;
      if (!Path.IsPathRooted (sqlFileName))
      {
        string assemblyUrl = typeof (DatabaseAgent).Assembly.Location;
        Uri uri = new Uri (assemblyUrl);
        sqlFileName = Path.Combine (Path.GetDirectoryName (uri.LocalPath), sqlFileName);
      }
      return ExecuteBatchString (File.ReadAllText (sqlFileName, Encoding.Default), useTransaction);
    }

    public int ExecuteBatchString (string commandBatch, bool useTransaction)
    {
      ArgumentUtility.CheckNotNull (nameof(commandBatch), commandBatch);

      var count = 0;
      using (DbConnection connection = CreateConnection ())
      {
        connection.Open ();
        if (useTransaction)
        {
          using (DbTransaction transaction = connection.BeginTransaction ())
          {
            count = ExecuteBatchString (connection, commandBatch, transaction);
            transaction.Commit ();
          }
        }
        else
          count = ExecuteBatchString (connection, commandBatch, null);
      }

      return count;
    }

    [Obsolete ("Use 'ExecuteBatchFile' instead.")]
    public int ExecuteBatch (string sqlFileName, bool useTransaction)
    {
      ArgumentUtility.CheckNotNull (nameof(sqlFileName), sqlFileName);

      return ExecuteBatchFile (sqlFileName, useTransaction);
    }

    protected virtual DbConnection CreateConnection ()
    {
      return new SqlConnection (_connectionString);
    }

    protected virtual DbCommand CreateCommand (DbConnection connection, string commandText, DbTransaction transaction)
    {
      DbCommand command = connection.CreateCommand ();
      command.CommandType = CommandType.Text;
      command.CommandText = commandText;
      command.Transaction = transaction;
      return command;
    }

    public int ExecuteCommand (string commandText)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(commandText), commandText);

      using (DbConnection connection = CreateConnection ())
      {
        connection.Open ();
        return ExecuteCommand (connection, commandText, null);
      }
    }

    public object ExecuteScalarCommand (string commandText)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(commandText), commandText);

      using (DbConnection connection = CreateConnection ())
      {
        connection.Open ();
        return ExecuteScalarCommand (connection, commandText, null);
      }
    }

    protected virtual int ExecuteBatchString (DbConnection connection, string commandBatch, DbTransaction transaction)
    {
      ArgumentUtility.CheckNotNull (nameof(connection), connection);
      ArgumentUtility.CheckNotNullOrEmpty (nameof(commandBatch), commandBatch);

      var count = 0;
      foreach (var command in GetCommandTextBatches (commandBatch))
      {
        if (command.Content != null)
        {
          try
          {
            count += ExecuteCommand (connection, command.Content, transaction);
          }
          catch (Exception ex)
          {
            throw new SqlBatchCommandException (
                string.Format (
                    "Could not execute batch command from row {0} to row {1}{2}. (Error message: {3})",
                    command.StartRowNumber,
                    command.EndRowNumber,
                    !string.IsNullOrEmpty (_fileName) ? " in file '" + _fileName + "'" : string.Empty,
                    ex.Message),
                ex);
          }
        }
      }
      return count;
    }

    protected virtual int ExecuteCommand (DbConnection connection, string commandText, DbTransaction transaction)
    {
      using (DbCommand command = CreateCommand (connection, commandText, transaction))
      {
        return command.ExecuteNonQuery ();
      }
    }

    protected virtual object ExecuteScalarCommand (DbConnection connection, string commandText, DbTransaction transaction)
    {
      using (DbCommand command = CreateCommand (connection, commandText, transaction))
      {
        return command.ExecuteScalar ();
      }
    }

    private IEnumerable<BatchCommand> GetCommandTextBatches (string commandBatch)
    {
      var lineNumber = 1;
      var command = new BatchCommand (lineNumber, commandBatch.Length);
      foreach (var line in commandBatch.Split (new[] { "\n", "\r\n" }, StringSplitOptions.None))
      {
        if (line.Trim ().Equals ("GO", StringComparison.OrdinalIgnoreCase))
        {
          var batch = command;
          command = new BatchCommand (lineNumber + 1, commandBatch.Length);
          yield return batch;
        }
        else
          command.AppendCommandBatchLine (line.Trim ());
        lineNumber++;
      }

      yield return command;
    }
  }
}
