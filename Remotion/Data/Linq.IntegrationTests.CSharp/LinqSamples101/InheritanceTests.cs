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
  public class InheritanceTests:TestBase
  {
    /// <summary>
    /// This sample returns all contacts where the city is London.
    /// </summary>
    [Test]
    public void LinqToSqlInheritance01 ()
    {
      var cons = from c in DB.Contacts
                 select c;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());

    }

    /// <summary>
    /// This sample uses OfType to return all customer contacts.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - System.NotImplementedException : Type check currently not supported")]
    public void LinqToSqlInheritance02 ()
    {
      var cons = from c in DB.Contacts.OfType<CustomerContact> ()
                 select c;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses IS to return all shipper contacts.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - System.NotImplementedException : Type check currently not supported")]
    public void LinqToSqlInheritance03 ()
    {
      var cons = from c in DB.Contacts
                 where c is ShipperContact
                 select c;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses AS to return FullContact or null.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - System.NotImplementedException : Type check currently not supported")]
    public void LinqToSqlInheritance04 ()
    {
      var cons = from c in DB.Contacts
                 select c as FullContact;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses a cast to retrieve customer contacts who live in London.
    /// </summary>
    [Test]
    public void LinqToSqlInheritance05 ()
    {
      var cons = from c in DB.Contacts
                 where c.ContactType == "Customer" && ((CustomerContact) c).City == "London"
                 select c;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());
    }
  }
}