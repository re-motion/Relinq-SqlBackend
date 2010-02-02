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
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class FromBuilderTest
  {
    private CommandBuilder _commandBuilder;
    private FromBuilder _fromBuilder;
    private Table _table1;
    private Table _table2;
    private SubQuery _subQuery;
    private SingleJoin _join1;
    private SingleJoin _join2;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry ());
      _fromBuilder = new FromBuilder (_commandBuilder);

      _table1 = new Table ("Table1", "t1");
      _table2 = new Table ("Table2", "t2");

      var queryModel = ExpressionHelper.CreateQueryModel_Student ();
      _subQuery = new SubQuery (queryModel, ParseMode.SubQueryInFrom, "s1");

      _join1 = new SingleJoin (new Column (_table1, "c1"), new Column (new Table ("JoinedTable", "j1"), "c2"));
      _join2 = new SingleJoin (new Column (_join1.RightSide, "c3"), new Column (new Table ("JoinedTable2", "j2"), "c4"));
    }

    [Test]
    public void BuildFromPart ()
    {
      var _sqlGenerationData = new SqlGenerationData ();
      _sqlGenerationData.FromSources.Add (_table1);
      _sqlGenerationData.FromSources.Add (_table2);

      var shortFieldSourcePath = new FieldSourcePath (_join1.LeftSide, new[] { _join1 });
      _sqlGenerationData.Joins.AddPath (shortFieldSourcePath);

      _fromBuilder.BuildFromPart (_sqlGenerationData);

      Assert.That (
          _commandBuilder.GetCommandText(), 
          Is.EqualTo ("FROM [Table1] [t1] LEFT OUTER JOIN [JoinedTable] [j1] ON [t1].[c1] = [j1].[c2], [Table2] [t2]"));
    }

    [Test]
    public void AppendColumnSources ()
    {
      var joins = new JoinCollection ();
      var shortFieldSourcePath = new FieldSourcePath (_join1.LeftSide, new[] { _join1 });
      joins.AddPath (shortFieldSourcePath);

      _fromBuilder.AppendColumnSources (new[] { _table1, _table2 }, joins);

      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo ("[Table1] [t1] LEFT OUTER JOIN [JoinedTable] [j1] ON [t1].[c1] = [j1].[c2], [Table2] [t2]"));
    }

    [Test]
    public void AppendColumnSources_MultipleJoins ()
    {
      var joins = new JoinCollection ();
      var longFieldSourcePath = new FieldSourcePath (_join1.LeftSide, new[] { _join1, _join2 });
      joins.AddPath (longFieldSourcePath);

      _fromBuilder.AppendColumnSources (new[] { _table1, _table2 }, joins);

      Assert.That (
          _commandBuilder.GetCommandText (),
          Is.EqualTo (
              "[Table1] [t1] "
              + "LEFT OUTER JOIN [JoinedTable] [j1] ON [t1].[c1] = [j1].[c2] "
              + "LEFT OUTER JOIN [JoinedTable2] [j2] ON [j1].[c3] = [j2].[c4], [Table2] [t2]"));
    }

    [Test]
    public void AppendColumnSource_First_Table ()
    {
      _fromBuilder.AppendColumnSource (_table1, new[] { _join1 }, true);

      Assert.That (
          _commandBuilder.GetCommandText(),
          Is.EqualTo ("[Table1] [t1] LEFT OUTER JOIN [JoinedTable] [j1] ON [t1].[c1] = [j1].[c2]"));
    }

    [Test]
    public void AppendColumnSource_NonFirst_Table ()
    {
      _fromBuilder.AppendColumnSource (_table1, new SingleJoin[0], false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (", [Table1] [t1]"));
    }

    [Test]
    public void AppendColumnSource_First_SubQuery ()
    {
      _fromBuilder.AppendColumnSource (_subQuery, new[] { _join1 }, true);

      Assert.That (
          _commandBuilder.GetCommandText(),
          Text.Matches (@"^\(SELECT .*\) \[s1\] LEFT OUTER JOIN \[JoinedTable\] \[j1\] ON \[t1\]\.\[c1\] = \[j1\]\.\[c2\]$"));
    }

    [Test]
    public void AppendColumnSource_NonFirst_SubQuery ()
    {
      _fromBuilder.AppendColumnSource (_subQuery, new SingleJoin[0], false);

      Assert.That (
          _commandBuilder.GetCommandText(),
          Text.Matches (@"^ CROSS APPLY \(SELECT .*\) \[s1\]$"));
    }

    [Test]
    public void AppendTable_First ()
    {
      _fromBuilder.AppendTable (_table1, true);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[Table1] [t1]"));
    }

    [Test]
    public void AppendTable_NonFirst ()
    {
      _fromBuilder.AppendTable (_table1, false);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (", [Table1] [t1]"));
    }

    [Test]
    public void AppendSubQuery_First ()
    {
      _fromBuilder.AppendSubQuery (_subQuery, true);

      Assert.That (_commandBuilder.GetCommandText(), Text.Matches (@"^\(SELECT .*\) \[s1\]"));
    }

    [Test]
    public void AppendSubQuery_NonFirst ()
    {
      _fromBuilder.AppendSubQuery (_subQuery, false);

      Assert.That (_commandBuilder.GetCommandText(), Text.Matches (@"^ CROSS APPLY \(SELECT .*\) \[s1\]"));
    }

    [Test]
    public void AppendSubQuery_UsesCommandBuilder ()
    {
      var commandBuilderMock = MockRepository.GenerateMock<ICommandBuilder> ();
      var fromBuilder = new FromBuilder (commandBuilderMock);

      fromBuilder.AppendSubQuery (_subQuery, true);

      commandBuilderMock.AssertWasCalled (mock => mock.AppendEvaluation (_subQuery));
    }

    [Test]
    public void AppendJoin ()
    {
      _fromBuilder.AppendJoin (_join1);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (" LEFT OUTER JOIN [JoinedTable] [j1] ON [t1].[c1] = [j1].[c2]"));
    }
  }
}
