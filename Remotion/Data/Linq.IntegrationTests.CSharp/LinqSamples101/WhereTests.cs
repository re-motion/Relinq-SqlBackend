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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  public class WhereTests:TestBase
  {
    /// <summary>
    /// This sample uses WHERE to filter for Customers in London.
    /// </summary>
    [Test]
    public void LinqToSqlWhere01 ()
    {
      var q =
          from c in DB.Customers
          where c.City == "London"
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses WHERE to filter for Employees hired during or after 1994.
    /// </summary>
    [Test,
    Ignore("Relinq doesn't know how to handle byte arrays")]
    public void LinqToSqlWhere02 ()
    {
      var q =
          from e in DB.Employees
          where e.HireDate >= new DateTime (1994, 1, 1)
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses WHERE to filter for Products that have stock below their reorder level and are not discontinued.
    /// </summary>
    [Test]
    public void LinqToSqlWhere03 ()
    {
      var q =
          from p in DB.Products
          where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses WHERE to filter out Products that are either UnitPrice is greater than 10 or is discontinued.
    /// </summary>
    [Test]
    public void LinqToSqlWhere04 ()
    {
      var q =
          from p in DB.Products
          where p.UnitPrice > 10m || p.Discontinued
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample calls WHERE twice to filter out Products that UnitPrice is greater than 10 and is discontinued.
    /// </summary>
    [Test]
    public void LinqToSqlWhere05 ()
    {
      var q =
          DB.Products.Where (p => p.UnitPrice > 10m).Where (p => p.Discontinued);

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses First to select the first Shipper in the table.
    /// </summary>
    [Test]
    public void LinqToSqlWhere06 ()
    {
      Shipper shipper = DB.Shippers.First ();
      TestExecutor.Execute (shipper, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses First to select the single Customer with CustomerID 'BONAP'.
    /// </summary>
    [Test]
    public void LinqToSqlWhere07 ()
    {
      Customer cust = DB.Customers.First (c => c.CustomerID == "BONAP");
      TestExecutor.Execute (cust, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    ///  This sample uses First to select an Order with freight greater than 10.00.
    /// </summary>
    [Test]
    public void LinqToSqlWhere08 ()
    {
      Order ord = DB.Orders.First (o => o.Freight > 10.00M);
      TestExecutor.Execute (ord, MethodBase.GetCurrentMethod ());
    }
  }
}
