//Microsoft Public License (Ms-PL)

//This license governs use of the accompanying software. If you use the software, you
//accept this license. If you do not accept the license, do not use the software.

//1. Definitions
//The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
//same meaning here as under U.S. copyright law.
//A "contribution" is the original software, or any additions or changes to the software.
//A "contributor" is any person that distributes its contribution under this license.
//"Licensed patents" are a contributor's patent claims that read directly on its contribution.

//2. Grant of Rights
//(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
//each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
//prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
//each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
//sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//3. Conditions and Limitations
//(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
//such contributor to the software ends automatically.
//(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
//in the software.
//(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
//this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
//license that complies with this license.
//(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
//You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
//the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

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
    [Ignore ("RM-3269: Invalid in-memory projection generated when a binary (or other) expression contains a conversion")]
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
    [Ignore ("RM-3306: Support for MemberInitExpressions")]
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
    [Ignore ("RM-3207: When a NewExpression contains a subquery whose original type is IEnumerable<T>, an ArgumentException (wrapped into a "
            + "TargetInvocationException) is thrown")]
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
    /// This sample uses Distinct to select a sequence of the unique cities that have Customers.
    /// </summary>
    [Test]
    public void LinqToSqlSelect10 ()
    {
      var q = (
          from c in DB.Customers
          select c.City)
          .Distinct ();

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    ///  Phone converter that converts a phone number to an international format based on its country. 
    ///  This sample only supports USA and UK formats, for phone numbers from the Northwind database.
    /// </summary>
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
    [Ignore ("RM-3307: Support for local method calls")]
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
    [Test]
    [Ignore ("RM-3307: Support for local method calls")]
    public void LinqToSqlLocalMethodCall02 ()
    {
      var doc = new XDocument (
          new XElement ("Customers", from c in DB.Customers
                                     where c.Country == "UK" || c.Country == "USA"
                                     select (new XElement ("Customer",
                                         new XAttribute ("CustomerID", c.CustomerID),
                                         new XAttribute ("CompanyName", c.CompanyName),
                                         new XAttribute ("InternationalPhone", PhoneNumberConverter (c.Country, c.Phone))
                                         ))));

      TestExecutor.Execute (doc, MethodBase.GetCurrentMethod());
    }
  }
}
