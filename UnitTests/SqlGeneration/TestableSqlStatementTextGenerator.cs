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
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  public class TestableSqlStatementTextGenerator : SqlStatementTextGenerator
  {
    public TestableSqlStatementTextGenerator (ISqlGenerationStage stage)
        : base (stage)
    {
    }

    public new void BuildSelectPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder, bool outerStatement)
    {
      base.BuildSelectPart (sqlStatement, commandBuilder, outerStatement);
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

    public new void BuildSetOperationCombinedStatementsPart (SqlStatement sqlStatement, ISqlCommandBuilder commandBuilder)
    {
      base.BuildSetOperationCombinedStatementsPart (sqlStatement, commandBuilder);
    }
  }
}