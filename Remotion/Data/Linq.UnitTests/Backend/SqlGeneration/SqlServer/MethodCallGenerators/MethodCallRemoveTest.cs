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
using NUnit.Framework;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  [TestFixture]
  public class MethodCallRemoveTest : MethodCalTestBase
  {
    [Test]
    public void Remove ()
    {
      var methodInfo = typeof (string).GetMethod ("Remove", new[] { typeof (int) });
      var column = new Column (new Table ("Cook", "s"), "FirstColumn");
      var arguments = new List<IEvaluation> { new Constant (5) };
      var methodCall = new MethodCall (methodInfo, column, arguments);

      var methodCallRemove = new MethodCallRemove();
      methodCallRemove.GenerateSql (methodCall, CommandBuilder);

      Assert.AreEqual ("xyz STUFF([s].[FirstColumn],@2,LEN([s].[FirstColumn]), \"\")", CommandBuilder.GetCommandText ());
      Assert.AreEqual (5, CommandBuilder.GetCommandParameters ()[1].Value);
    }
  }
}
