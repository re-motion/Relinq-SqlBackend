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
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class OrderByBuilderTest
  {
    private CommandBuilder _commandBuilder;
    private OrderByBuilder _orderByBuilder;

    private FieldDescriptor _fieldDescriptor1;
    private FieldDescriptor _fieldDescriptor2;

    private OrderingField _orderingFieldAsc;
    private OrderingField _orderingFieldDesc;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new CommandBuilder (
          new SqlServerGenerator (StubDatabaseInfo.Instance),
          new StringBuilder (),
          new List<CommandParameter> (),
          StubDatabaseInfo.Instance,
          new MethodCallSqlGeneratorRegistry ());
      _orderByBuilder = new OrderByBuilder (_commandBuilder);

      _fieldDescriptor1 = CreateFieldDescriptor ("Table1", "table1", "c1");
      _fieldDescriptor2 = CreateFieldDescriptor ("Table2", "table2", "c2");

      _orderingFieldAsc = new OrderingField (_fieldDescriptor1, OrderingDirection.Asc);
      _orderingFieldDesc = new OrderingField (_fieldDescriptor2, OrderingDirection.Desc);
    }

    [Test]
    public void BuildOrderByPart_NoOrderingFields ()
    {
      _orderByBuilder.BuildOrderByPart (new SqlGenerationData ());

      Assert.That (_commandBuilder.GetCommandText (), Is.Empty);
    }

    [Test]
    public void BuildOrderByPart_WithOrderingFields ()
    {
      var sqlGenerationData = new SqlGenerationData ();
      sqlGenerationData.OrderingFields.Add (_orderingFieldAsc);
      sqlGenerationData.OrderingFields.Add (_orderingFieldDesc);

      _orderByBuilder.BuildOrderByPart (sqlGenerationData);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" ORDER BY [table1].[c1] ASC, [table2].[c2] DESC"));
    }

    [Test]
    public void AppendOrderingFields ()
    {
      _orderByBuilder.AppendOrderingFields (new[] { _orderingFieldAsc, _orderingFieldDesc });

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[table1].[c1] ASC, [table2].[c2] DESC"));
    }

    [Test]
    public void AppendOrderingField_Asc ()
    {
      _orderByBuilder.AppendOrderingField (_orderingFieldAsc);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[table1].[c1] ASC"));
    }

    [Test]
    public void AppendOrderingField_Desc ()
    {
      _orderByBuilder.AppendOrderingField (_orderingFieldDesc);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("[table2].[c2] DESC"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "OrderingDirection -1 is not supported.")]
    public void AppendOrderingField_Invalid ()
    {
      _orderByBuilder.AppendOrderingField (new OrderingField(_fieldDescriptor1, (OrderingDirection)(-1)));
    }

    private FieldDescriptor CreateFieldDescriptor (string tableName, string tableAlias, string columnName)
    {
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable (tableName, tableAlias);
      MemberInfo member = typeof (Cook).GetProperty ("FirstName");
      var column = new Column (path.FirstSource, columnName);
      return new FieldDescriptor (member, path, column);
    }

  }
}
