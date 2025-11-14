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
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlCommandData"/> contains the SQL command text and parameters generated for a LINQ query. In addition, it provides the possibility
  /// to get an in-memory projection expression that can be compiled to a delegate and executed on every row of the result set in order to construct
  /// a projection result as defined by the LINQ query.
  /// </summary>
  public struct SqlCommandData
  {
    private readonly string _commandText;
    private readonly CommandParameter[] _parameters;
    private readonly ParameterExpression _inMemoryProjectionParameter;
    private readonly Expression _inMemoryProjectionBody;

    public SqlCommandData (
        string commandText, 
        CommandParameter[] parameters, 
        ParameterExpression inMemoryProjectionParameter, 
        Expression inMemoryProjectionBody)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(commandText), commandText);
      ArgumentUtility.CheckNotNull (nameof(parameters), parameters);
      ArgumentUtility.CheckNotNull (nameof(inMemoryProjectionParameter), inMemoryProjectionParameter);
      ArgumentUtility.CheckNotNull (nameof(inMemoryProjectionBody), inMemoryProjectionBody);
      
      _commandText = commandText;
      _parameters = parameters;
      _inMemoryProjectionParameter = inMemoryProjectionParameter;
      _inMemoryProjectionBody = inMemoryProjectionBody;
    }

    /// <summary>
    /// Gets the SQL command text. This is the command to be executed against the database. For each result row, a delegate compiled from the
    /// <see cref=" GetInMemoryProjection{T}">in-memory projection</see>
    /// should be executed in order to retrieve the constructed projection.
    /// </summary>
    /// <value>The SQL command text.</value>
    public string CommandText
    {
      get { return _commandText; }
    }

    /// <summary>
    /// Gets the parameters to be used when executing <see cref="CommandText"/> as a SQL command.
    /// </summary>
    /// <value>The parameters of the query.</value>
    public CommandParameter[] Parameters
    {
      get { return _parameters; }
    }


    /// <summary>
    /// Gets the in-memory projection associated with this <see cref="SqlCommandData"/>. The in-memory projection can be used to construct the
    /// objects selected by a LINQ query by applying it to each of the rows returned for the query defined by <see cref="CommandText"/>. The result
    /// for each row is accessed via an implementation of <see cref="IDatabaseResultRow"/> supplied by the LINQ provider. Compile the returned
    /// expression in order to get an executable delegate that executes the in-memory projection.
    /// </summary>
    /// <typeparam name="T">The type of the values to be returned by the projection. This corresponds to the type parameters passed to the methods of 
    /// <see cref="IQueryExecutor"/>. If the type is not known, pass <see cref="object"/>.</typeparam>
    /// <returns>
    /// The in-memory projection associated with this <see cref="SqlCommandData"/>.
    /// </returns>
    public Expression<Func<IDatabaseResultRow, T>> GetInMemoryProjection<T> ()
    {
      var body = typeof (T) == _inMemoryProjectionBody.Type ? _inMemoryProjectionBody : Expression.Convert (_inMemoryProjectionBody, typeof (T));
      return Expression.Lambda<Func<IDatabaseResultRow, T>> (body, _inMemoryProjectionParameter);
    }
  }
}