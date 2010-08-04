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
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupExternalMapping:Executor
  {
    //This sample demonstrates how to create a data context that uses an external XML mapping source.
    //Not working --> Endless!
    /*
    public void LinqToSqlExternal01 ()
    {
      // load the mapping source
      string path = Path.GetFullPath (Path.Combine (Environment.CurrentDirectory, @"..\..\LinqSamples101\NorthwindMapped.map"));

      XmlMappingSource mappingSource = XmlMappingSource.FromXml (File.ReadAllText (path));

      // create context using mapping source
      Mapped.NorthwindMapped nw = new Mapped.NorthwindMapped (db.Connection, mappingSource);

      // demonstrate use of an externally-mapped entity 
      Console.WriteLine ("****** Externally-mapped entity ******");
      Mapped.Order order = nw.Orders.First ();
      serializer.Serialize (order);

      // demonstrate use of an externally-mapped inheritance hierarchy
      var contacts = from c in nw.Contacts
                     where c is Mapped.EmployeeContact
                     select c;
      Console.WriteLine ();
      Console.WriteLine ("****** Externally-mapped inheritance hierarchy ******");
      foreach (var contact in contacts)
      {
        Console.WriteLine ("Company name: {0}", contact.CompanyName);
        Console.WriteLine ("Phone: {0}", contact.Phone);
        Console.WriteLine ("This is a {0}", contact.GetType ());
        Console.WriteLine ();
      }

      // demonstrate use of an externally-mapped stored procedure
      Console.WriteLine ();
      Console.WriteLine ("****** Externally-mapped stored procedure ******");
      foreach (Mapped.CustOrderHistResult result in nw.CustomerOrderHistory ("ALFKI"))
      {
        serializer.Serialize (result);
      }

      // demonstrate use of an externally-mapped scalar user defined function
      Console.WriteLine ();
      Console.WriteLine ("****** Externally-mapped scalar UDF ******");
      var totals = from c in nw.Categories
                   select new { c.CategoryID, TotalUnitPrice = nw.TotalProductUnitPriceByCategory (c.CategoryID) };
      serializer.Serialize (totals);

      // demonstrate use of an externally-mapped table-valued user-defined function
      Console.WriteLine ();
      Console.WriteLine ("****** Externally-mapped table-valued UDF ******");
      var products = from p in nw.ProductsUnderThisUnitPrice (9.75M)
                     where p.SupplierID == 8
                     select p;
      serializer.Serialize (products);
    }
     * */
  }
}