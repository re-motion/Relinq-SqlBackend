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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Default implementation of <see cref="ISqlCommandBuilder"/> with SQL Server identifier semantics.
  /// </summary>
  public class SqlCommandBuilder : ISqlCommandBuilder
  {
    private readonly StringBuilder _stringBuilder;
    private readonly List<CommandParameter> _parameters;

    private readonly ParameterExpression _inMemoryProjectionRowParameter;
    
    private Expression _inMemoryProjectionBody;
    
    public SqlCommandBuilder ()
    {
      _stringBuilder = new StringBuilder();
      _parameters = new List<CommandParameter>();
      
      _inMemoryProjectionRowParameter = Expression.Parameter (typeof (IDatabaseResultRow), "row");
      _inMemoryProjectionBody = null;
    }

    public ParameterExpression InMemoryProjectionRowParameter
    {
      get { return _inMemoryProjectionRowParameter; }
    }

    public CommandParameter CreateParameter (object value)
    {
      var parameter = new CommandParameter ("@" + (_parameters.Count + 1), value);
      _parameters.Add (parameter);

      return parameter;
    }

    public void Append (string stringToAppend)
    {
      ArgumentUtility.CheckNotNull ("stringToAppend", stringToAppend);
      _stringBuilder.Append (stringToAppend);
    }

    public void AppendSeparated<T> (string separator, IEnumerable<T> values, Action<ISqlCommandBuilder, T> appender)
    {
      ArgumentUtility.CheckNotNull ("separator", separator);
      ArgumentUtility.CheckNotNull ("values", values);
      ArgumentUtility.CheckNotNull ("appender", appender);

      bool first = true;
      foreach (T value in values)
      {
        if (!first)
          _stringBuilder.Append (separator);
        first = false;
        appender (this, value);
      }
    }

    public void AppendIdentifier (string identifier)
    {
      AppendFormat ("[{0}]", identifier);
    }

    public void AppendStringLiteral (string value)
    {
      Append ("'");
      Append (value.Replace ("'", "''"));
      Append ("'");
    }

    public void AppendFormat (string stringToAppend, params object[] parameters)
    {
      ArgumentUtility.CheckNotNull ("stringToAppend", stringToAppend);

      _stringBuilder.AppendFormat (stringToAppend, parameters);
    }

    public CommandParameter AppendParameter (object value)
    {
      var parameter = CreateParameter (value);
      Append (parameter.Name);
      return parameter;
    }

    public void SetInMemoryProjectionBody (Expression body)
    {
      _inMemoryProjectionBody = body;
    }

    public Expression GetInMemoryProjectionBody ()
    {
      return _inMemoryProjectionBody;
    }

 
    public string GetCommandText ()
    {
      return _stringBuilder.ToString();
    }

    public CommandParameter[] GetCommandParameters ()
    {
      return _parameters.ToArray();
    }

    public SqlCommandData GetCommand ()
    {
      var commandText = GetCommandText();
      var commandParameters = GetCommandParameters();
      var inMemoryProjectionBody = GetInMemoryProjectionBody();

      if (string.IsNullOrEmpty (commandText))
        throw new InvalidOperationException ("Command text must be appended before a command can be created.");

      if (inMemoryProjectionBody == null)
        throw new InvalidOperationException ("An in-memory projection body must be appended before a command can be created.");

      return new SqlCommandData (commandText, commandParameters, InMemoryProjectionRowParameter, inMemoryProjectionBody);
    }
  }
}