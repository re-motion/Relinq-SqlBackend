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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  internal class ObjectIdentityTests:TestBase
  {
    /// <summary>
    /// This sample demonstrates how, upon executing the same query twice, you will 
    /// receive a reference to the same object in memory each time.
    /// </summary>
    [Test]
    public void LinqToSqlObjectIdentity01 ()
    {
      Customer cust1 = DB.Customers.First (c => c.CustomerID == "BONAP");
      Customer cust2 = DB.Customers.First (c => c.CustomerID == "BONAP");

      TestExecutor.Execute(Object.ReferenceEquals (cust1, cust2), MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample demonstrates how, upon executing different queries that return the same row from the 
    /// database, you will receive a reference to the same object in memory each time.")]
    /// </summary>
    [Test]
    public void LinqToSqlObjectIdentity02 ()
    {
      Customer cust1 = DB.Customers.First (c => c.CustomerID == "BONAP");
      Customer cust2 = (
          from o in DB.Orders
          where o.Customer.CustomerID == "BONAP"
          select o)
          .First ()
          .Customer;

      TestExecutor.Execute (Object.ReferenceEquals (cust1, cust2), MethodBase.GetCurrentMethod ());
    }
  }
}