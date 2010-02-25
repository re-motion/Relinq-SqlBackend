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
  public class MethodCallUpperTest : MethodCalTestBase
  {
    [Test]
    public void ToUpper ()
    {
      var methodInfo = typeof (string).GetMethod ("ToUpper", new Type[] { });
      var column = new Column (new Table ("Chef", "s"), "FirstColumn");
      var arguments = new List<IEvaluation> ();
      var methodCall = new MethodCall (methodInfo, column, arguments);
      
      var methodCallUpper = new MethodCallUpper ();
      methodCallUpper.GenerateSql (methodCall, CommandBuilder);
      
      Assert.AreEqual ("xyz UPPER([s].[FirstColumn])", CommandBuilder.GetCommandText ());
    }
  }
}
