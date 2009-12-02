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
    [Test]
    public void CombineOrderedFields()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      OrderByBuilder orderByBuilder = new OrderByBuilder (commandBuilder);

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ("s1", "s1");
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column (path.FirstSource,"c1");
      FieldDescriptor descriptor = new FieldDescriptor (member, path, column);

      OrderingField field1 = new OrderingField (descriptor,OrderingDirection.Asc);
      OrderingField field2 = new OrderingField (descriptor, OrderingDirection.Desc);
      List<OrderingField> orderingFields = new List<OrderingField> { field1,field2 };

      orderByBuilder.BuildOrderByPart (orderingFields);

      Assert.AreEqual (" ORDER BY [s1].[c1] ASC, [s1].[c1] DESC", commandBuilder.GetCommandText());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "OrderingDirection 2147483647 is not supported.")]
    public void CombineOrderedFields_InvalidDirection ()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      OrderByBuilder orderByBuilder = new OrderByBuilder (commandBuilder);

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ("s1", "s1");
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column (path.FirstSource, "c1");
      FieldDescriptor descriptor = new FieldDescriptor (member, path, column);

      OrderingField field1 = new OrderingField (descriptor, (OrderingDirection) int.MaxValue);
      List<OrderingField> orderingFields = new List<OrderingField> { field1 };

      orderByBuilder.BuildOrderByPart (orderingFields);
    }
  }
}
