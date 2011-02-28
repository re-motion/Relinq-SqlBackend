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

using System.Collections.Generic;
using System.Data.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.Common.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  [Explicit ("Not tested: Stored procedures")]
  public class StoredProceduresTests:TestBase
  {
    /// <summary>
    /// This sample uses a stored procedure to return the number of Customers in the 'WA' Region.
    /// </summary>
    [Test]
    public void LinqToSqlStoredProc01 ()
    {
      int count = DB.CustomersCountByRegion ("WA");

      TestExecutor.Execute (count, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses a stored procedure to return the CustomerID, ContactName, CompanyName and City of customers who are in London.
    /// </summary>
    [Test]
    public void LinqToSqlStoredProc02 ()
    {
      ISingleResult<CustomersByCityResult> result = DB.CustomersByCity ("London");

      TestExecutor.Execute (result, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses a stored procedure to return a set of Customers in the 'WA' Region.
    /// The result set-shape returned depends on the parameter passed in. If the parameter equals 1, 
    /// all Customer properties are returned. If the parameter equals 2, the CustomerID, ContactName and
    /// CompanyName properties are returned.
    /// </summary>
    [Test]
    public void LinqToSqlStoredProc03_1 ()
    {
      IMultipleResults result = DB.WholeOrPartialCustomersSet (1);
      IEnumerable<WholeCustomersSetResult> shape1 = result.GetResult<WholeCustomersSetResult> ();

      TestExecutor.Execute (shape1, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses a stored procedure to return a set of Customers in the 'WA' Region.  
    /// The result set-shape returned depends on the parameter passed in. If the parameter equals 1, 
    /// all Customer properties are returned. If the parameter equals 2, the CustomerID, ContactName 
    /// and CompanyName properties are returned.
    /// </summary>
    [Test]
    public void LinqToSqlStoredProc03_2 ()
    {
      IMultipleResults result = DB.WholeOrPartialCustomersSet (2);
      IEnumerable<PartialCustomersSetResult> shape2 = result.GetResult<PartialCustomersSetResult> ();

      TestExecutor.Execute (shape2, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses a stored procedure to return the Customer 'SEVES' and all their Orders.
    /// </summary>
    [Test]
    public void LinqToSqlStoredProc04_1 ()
    {
      IMultipleResults result = DB.GetCustomerAndOrders ("SEVES");

      IEnumerable<CustomerResultSet> customer = result.GetResult<CustomerResultSet> ();
      TestExecutor.Execute (customer, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses a stored procedure to return the Customer 'SEVES' and all their Orders.
    /// </summary>
    [Test]
    public void LinqToSqlStoredProc04_2 ()
    {
      IMultipleResults result = DB.GetCustomerAndOrders ("SEVES");

      IEnumerable<OrdersResultSet> orders = result.GetResult<OrdersResultSet> ();
      TestExecutor.Execute (orders, MethodBase.GetCurrentMethod ());
    }

    /// <summary>
    /// This sample uses a stored procedure that returns an out parameter.
    /// </summary>
    [Test]
    public void LinqToSqlStoredProc05 ()
    {
      decimal? totalSales = 0;
      string customerID = "ALFKI";

      // Out parameters are passed by ref, to support scenarios where
      // the parameter is 'in/out'.  In this case, the parameter is only
      // 'out'.
      DB.CustomerTotalSales (customerID, ref totalSales);

      TestExecutor.Execute (totalSales, MethodBase.GetCurrentMethod ());
    }
  }
}