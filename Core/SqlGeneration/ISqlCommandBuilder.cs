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
using System.Linq.Expressions;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Used by the classes in the SQL generation stage to build a SQL command. Use <see cref="GetCommand"/> to access the command when the stage
  /// has finished.
  /// </summary>
  public interface ISqlCommandBuilder
  {
    ParameterExpression InMemoryProjectionRowParameter { get; }
    
    CommandParameter CreateParameter (object value);
    CommandParameter GetOrCreateParameter (ConstantExpression constantExpression);

    void Append (string stringToAppend);
    void AppendSeparated<T> (string separator, IEnumerable<T> values, Action<ISqlCommandBuilder, T> appender);
    void AppendIdentifier (string identifier);
    void AppendStringLiteral (string value);
    void AppendBooleanLiteral (bool value);
    void AppendFormat (string stringToAppend, params object[] parameters);
    CommandParameter AppendParameter (object value);

    void SetInMemoryProjectionBody (Expression body);
    Expression GetInMemoryProjectionBody ();

    string GetCommandText ();
    CommandParameter[] GetCommandParameters ();
    SqlCommandData GetCommand ();
  }
}