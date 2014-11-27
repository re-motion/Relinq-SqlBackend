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

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Defines whether a <see cref="SqlJoin"/> represents a left or inner join.
  /// </summary>
  public enum JoinSemantics
  {
    /// <summary>
    /// SQL-style LEFT OUTER JOIN semantics: All records of the left table are returned. If the right table holds no matching records, the right 
    /// side's columns contain NULL. 
    /// </summary>
    Left,
    /// <summary>
    /// SQL-style INNER JOIN semantics: Only records that produce a match are returned.
    /// </summary>
    Inner
  }
}