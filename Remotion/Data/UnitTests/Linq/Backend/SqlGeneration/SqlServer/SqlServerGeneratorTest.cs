// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;

namespace Remotion.Data.UnitTests.Linq.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class SqlServerGeneratorTest
  {
    [Test]
    public void DefaultMethodCallRegistration ()
    {
      MethodCallSqlGeneratorRegistry methodCallSqlGeneratorRegistry = new SqlServerGenerator (StubDatabaseInfo.Instance).MethodCallRegistry;

      IMethodCallSqlGenerator removeGenerator =
          methodCallSqlGeneratorRegistry.GetGenerator (typeof (string).GetMethod ("Remove", new Type[] { typeof (int) }));

      IMethodCallSqlGenerator upperGenerator =
          methodCallSqlGeneratorRegistry.GetGenerator (typeof (string).GetMethod ("ToUpper", new Type[] { }));

      Assert.IsNotNull (removeGenerator);
      Assert.IsNotNull (upperGenerator);
    }
  }
}