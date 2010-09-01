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
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class OrderByTests:TestBase
  {
    /// <summary>
    /// This sample uses orderby to sort Employees by hire date.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - InvalidCastException - Unable to cast System.Byte[] to System.Data.Linq.Binary")]
    public void LinqToSqlOrderBy01 ()
    {
      var q =
          from e in DB.Employees
          orderby e.HireDate
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses where and orderby to sort Orders shipped to London by freight.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy02 ()
    {
      var q =
          from o in DB.Orders
          where o.ShipCity == "London"
          orderby o.Freight
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses orderby to sort Products by unit price from highest to lowest.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy03 ()
    {
      var q =
          from p in DB.Products
          orderby p.UnitPrice descending
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses a compound orderby to sort Customers by city and then contact name.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy04 ()
    {
      var q =
          from c in DB.Customers
          orderby c.City, c.ContactName
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses orderby to sort Orders from EmployeeID 1 by ship-to country, and then by freight from highest to lowest.
    /// </summary>
    [Test]
    public void LinqToSqlOrderBy05 ()
    {
      var q =
          from o in DB.Orders
          where o.EmployeeID == 1
          orderby o.ShipCountry, o.Freight descending
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }


    /// <summary>
    /// This sample uses orderby, Max and Group By to find the Products that have the highest 
    /// unit price in each category, and sorts the group by category id.
    /// </summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - ArgumentException - Argument type IQueryable does not match the corresponding member type IEnumerable")]
    public void LinqToSqlOrderBy06 ()
    {
      var categories =
          from p in DB.Products
          group p by p.CategoryID into g
          orderby g.Key
          select new
          {
            g.Key,
            MostExpensiveProducts =
                from p2 in g
                where p2.UnitPrice == g.Max (p3 => p3.UnitPrice)
                select p2
          };

      TestExecutor.Execute (categories, MethodBase.GetCurrentMethod());
    }
  }
}
