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
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  public class TestableSqlStatementTextGenerator : SqlStatementTextGenerator
  {
    public TestableSqlStatementTextGenerator (ISqlGenerationStage stage)
        : base (stage)
    {
    }

    public new void BuildSelectPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      base.BuildSelectPart (sqlStatement, commandBuilder);
    }

    public new void BuildFromPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      base.BuildFromPart (sqlStatement, commandBuilder);
    }

    public new void BuildWherePart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      base.BuildWherePart (sqlStatement, commandBuilder);
    }

    public new void BuildOrderByPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      base.BuildOrderByPart (sqlStatement, commandBuilder);
    }

    public new void BuildDistinctPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      base.BuildDistinctPart (sqlStatement, commandBuilder);
    }

    public new void BuildTopPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      base.BuildTopPart (sqlStatement, commandBuilder);
    }

    public new void BuildAggregationPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      base.BuildAggregationPart(sqlStatement, commandBuilder);
    }
  }
}