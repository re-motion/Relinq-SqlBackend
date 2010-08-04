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
using System.Data.Linq;
using System.Linq;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupCompiledQuery : Executor
  {
    //This sample create a compiled query and then use it to retrieve customers of the input city")]
    public void LinqToSqlCompileQuery01 ()
    {
      //Create compiled query
      var fn = CompiledQuery.Compile (
          (Northwind db2, string city) =>
          from c in db2.Customers
          where c.City == city
          select c);

      serializer.Serialize ("****** Call compiled query to retrieve customers from London ******");
      var LonCusts = fn (db, "London");
      serializer.Serialize (LonCusts);

      serializer.Serialize (string.Empty);

      serializer.Serialize ("****** Call compiled query to retrieve customers from Seattle ******");
      var SeaCusts = fn (db, "Seattle");
      serializer.Serialize (SeaCusts);
    }
  }
}