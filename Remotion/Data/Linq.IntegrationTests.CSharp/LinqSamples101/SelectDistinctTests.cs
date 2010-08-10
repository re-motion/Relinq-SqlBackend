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
using System.Xml.Linq;
using NUnit.Framework;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class SelectDistinctTests:TestBase
  {
    /// <summary>
    /// This sample uses SELECT to return a sequence of just the Customers' contact names.
    /// </summary>
    [Test]
    public void LinqToSqlSelect01 ()
    {
      var q =
          from c in DB.Customers
          select c.ContactName;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses SELECT and anonymous types to return a sequence of just the Customers' contact names and phone numbers.
    /// </summary>
    [Test]
    public void LinqToSqlSelect02 ()
    {
      var q =
          from c in DB.Customers
          select new { c.ContactName, c.Phone };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses SELECT and anonymous types to return a sequence of just the Employees' 
    /// names and phone numbers, with the FirstName and LastName fields combined into a single field, 
    /// 'Name', and the HomePhone field renamed to Phone in the resulting sequence.
    /// </summary>
    [Test]
    public void LinqToSqlSelect03 ()
    {
      var q =
          from e in DB.Employees
          select new { Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses SELECT and anonymous types to return a sequence of all Products' IDs 
    /// and a calculated value called HalfPrice which is set to the Product's UnitPrice divided by 2.
    /// </summary>
    [Test]
    public void LinqToSqlSelect04 ()
    {
      var q =
          from p in DB.Products
          select new { p.ProductID, HalfPrice = p.UnitPrice / 2 };
      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses SELECT and a conditional statement to return a sequence of product name and product availability.
    /// </summary>
    [Test]
    public void LinqToSqlSelect05 ()
    {
      var q =
          from p in DB.Products
          select new { p.ProductName, Availability = p.UnitsInStock - p.UnitsOnOrder < 0 ? "Out Of Stock" : "In Stock" };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses SELECT and a known type to return a sequence of employees' names.
    /// </summary>
    [Test]
    public void LinqToSqlSelect06 ()
    {
      var q =
          from e in DB.Employees
          select new Name { FirstName = e.FirstName, LastName = e.LastName };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses SELECT and WHERE to return a sequence of just the London Customers' contact names.
    /// </summary>
    [Test]
    public void LinqToSqlSelect07 ()
    {
      var q =
          from c in DB.Customers
          where c.City == "London"
          select c.ContactName;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses SELECT and anonymous types to return a shaped subset of the data about Customers.
    /// </summary>
    [Test]
    public void LinqToSqlSelect08 ()
    {
      var q =
          from c in DB.Customers
          select new
          {
            c.CustomerID,
            CompanyInfo = new { c.CompanyName, c.City, c.Country },
            ContactInfo = new { c.ContactName, c.ContactTitle }
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses nested queries to return a sequence of all orders containing their OrderID, 
    /// a subsequence of the items in the order where there is a discount, and the money saved if shipping is not included.
    /// </summary>
    [Test]
    public void LinqToSqlSelect09 ()
    {
      var q =
          from o in DB.Orders
          select new
          {
            o.OrderID,
            DiscountedProducts =
                from od in o.OrderDetails
                where od.Discount > 0.0
                select od,
            FreeShippingDiscount = o.Freight
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    ///  Phone converter that converts a phone number to an international format based on its country. 
    ///  This sample only supports USA and UK formats, for phone numbers from the Northwind database.
    /// </summary>
    [Test]
    public string PhoneNumberConverter (string Country, string Phone)
    {
      Phone = Phone.Replace (" ", "").Replace (")", ")-");
      switch (Country)
      {
        case "USA":
          return "1-" + Phone;
        case "UK":
          return "44-" + Phone;
        default:
          return Phone;
      }
    }

    /// <summary>
    /// This sample uses a Local Method Call to 'PhoneNumberConverter' to convert Phone number to an international format.
    /// </summary>
    [Test]
    public void LinqToSqlLocalMethodCall01 ()
    {
      var q = from c in DB.Customers
              where c.Country == "UK" || c.Country == "USA"
              select new { c.CustomerID, c.CompanyName, Phone = c.Phone, InternationalPhone = PhoneNumberConverter (c.Country, c.Phone) };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses a Local Method Call to convert phone numbers to an international format and create XDocument.
    /// </summary>
    [Test,
    Ignore]
    public void LinqToSqlLocalMethodCall02 ()
    {
      XDocument doc = new XDocument (
          new XElement ("Customers", from c in DB.Customers
                                     where c.Country == "UK" || c.Country == "USA"
                                     select (new XElement ("Customer",
                                         new XAttribute ("CustomerID", c.CustomerID),
                                         new XAttribute ("CompanyName", c.CompanyName),
                                         new XAttribute ("InternationalPhone", PhoneNumberConverter (c.Country, c.Phone))
                                         ))));

      TestExecutor.Execute (doc, MethodBase.GetCurrentMethod());
    }


    /// <summary>
    /// This sample uses Distinct to select a sequence of the unique cities that have Customers.
    /// </summary>
    [Test]
    public void LinqToSqlSelect10 ()
    {
      var q = (
          from c in DB.Customers
          select c.City)
          .Distinct ();

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}
