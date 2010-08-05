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
using NUnit.Framework;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  internal class CompiledQueryTests : TestBase
  {
    //TODO: Won't compile since INorthwindDataProvider is no subtype of DataContext, which is necessary for using CompiledQuery.Compile(..) - Check if tests are necessary
    ////This sample create a compiled query and then use it to retrieve customers of the input city")]
    //public void LinqToSqlCompileQuery01_1 ()
    //{
    //  //Create compiled query
    //  var fn = CompiledQuery.Compile (
    //      (INorthwindDataProvider db2, string city) =>
    //      from c in db2.Customers
    //      where c.City == city
    //      select c);

    //  var LonCusts = fn (DB, "London");
    //  TestExecutor.Execute (LonCusts, MethodBase.GetCurrentMethod());
    //}

    ////This sample create a compiled query and then use it to retrieve customers of the input city")]
    //public void LinqToSqlCompileQuery01_2 ()
    //{
    //  //Create compiled query
    //  var fn = CompiledQuery.Compile (
    //      (Northwind db2, string city) =>
    //      from c in db2.Customers
    //      where c.City == city
    //      select c);

    //  var SeaCusts = fn (DB, "Seattle");
    //  TestExecutor.Execute (SeaCusts, MethodBase.GetCurrentMethod ());
    //}

  }
}