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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer
{
  public class FromBuilder : IFromBuilder
  {
    private readonly ICommandBuilder _commandBuilder;

    public FromBuilder (ICommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      _commandBuilder = commandBuilder;
    }

    public void BuildFromPart (SqlGenerationData sqlGenerationData)
    {
      ArgumentUtility.CheckNotNull ("sqlGenerationData", sqlGenerationData);

      _commandBuilder.Append ("FROM ");
      AppendColumnSources (sqlGenerationData.FromSources, sqlGenerationData.Joins);
    }

    public void AppendColumnSources (IEnumerable<IColumnSource> fromSources, JoinCollection joins)
    {
      ArgumentUtility.CheckNotNull ("fromSources", fromSources);
      ArgumentUtility.CheckNotNull ("joins", joins);

      bool first = true;
      foreach (var fromSource in fromSources)
      {
        AppendColumnSource(fromSource, joins[fromSource], first);
        first = false;
      }
    }

    public void AppendColumnSource (IColumnSource fromSource, IEnumerable<SingleJoin> joins, bool first)
    {
      ArgumentUtility.CheckNotNull ("fromSource", fromSource);
      ArgumentUtility.CheckNotNull ("joins", joins);

      var table = fromSource as Table;
      if (table != null)
        AppendTable(table, first);
      else
        AppendSubQuery ((SubQuery) fromSource, first);

      AppendJoins (joins);
    }

    public void AppendTable (Table table, bool first)
    {
      ArgumentUtility.CheckNotNull ("table", table);
      if (!first)
        _commandBuilder.Append (", ");
      _commandBuilder.Append (SqlServerUtility.GetTableDeclaration (table));
    }

    public void AppendSubQuery (SubQuery subQuery, bool first)
    {
      ArgumentUtility.CheckNotNull ("subQuery", subQuery);
      if (!first)
        _commandBuilder.Append (" CROSS APPLY ");
      _commandBuilder.AppendEvaluation (subQuery);
    }

    public void AppendJoins (IEnumerable<SingleJoin> joins)
    {
      ArgumentUtility.CheckNotNull ("joins", joins);
      foreach (var join in joins)
        AppendJoin (join);
    }

    public void AppendJoin (SingleJoin join)
    {
      _commandBuilder.Append (" LEFT OUTER JOIN ");
      _commandBuilder.Append (SqlServerUtility.GetTableDeclaration ((Table) join.RightSide));
      _commandBuilder.Append (" ON ");
      _commandBuilder.Append (SqlServerUtility.GetColumnString (join.LeftColumn));
      _commandBuilder.Append (" = ");
      _commandBuilder.Append (SqlServerUtility.GetColumnString (join.RightColumn));
    }
  }
}
