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
using Remotion.Linq.SqlBackend.MappingResolution;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="IResolvedTableInfo"/> provides nested members and methods for all kinds of tables. 
  /// </summary>
  public interface IResolvedTableInfo : ITableInfo
  {
    string TableAlias { get; }

    Expression ResolveReference (
        SqlTable sqlTable,
        IMappingResolver mappingResolver,
        IMappingResolutionContext context,
        UniqueIdentifierGenerator generator);
  }
}