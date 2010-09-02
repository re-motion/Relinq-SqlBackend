'Microsoft Public License (Ms-PL)

'This license governs use of the accompanying software. If you use the software, you
'accept this license. If you do not accept the license, do not use the software.

'1. Definitions
'The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
'same meaning here as under U.S. copyright law.
'A "contribution" is the original software, or any additions or changes to the software.
'A "contributor" is any person that distributes its contribution under this license.
'"Licensed patents" are a contributor's patent claims that read directly on its contribution.

'2. Grant of Rights
'(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
'each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
'prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
'(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
'each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
'sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

'3. Conditions and Limitations
'(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
'(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
'such contributor to the software ends automatically.
'(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
'in the software.
'(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
'this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
'license that complies with this license.
'(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
'You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
'the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

Option Infer On
Option Strict On

Imports NUnit.Framework

Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind


Namespace LinqSamples101
  <TestFixture()>
  Public Class StoredProceduresTests
    Inherits TestBase

    'This sample uses a stored procedure to return the number of Customers in the 'WA' Region.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - stored procedures not supported")>
    Public Sub LinqToSqlStoredProc01()
      'WORKAROUND: Customers_Count_By_Region not available => changed to  CustomersCountByRegion
      Dim count = DB.CustomersCountByRegion("WA")

      TestExecutor.Execute(count, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a method mapped to the 'Customers By City' stored procedure
    'in Northwind database to return customers from 'London'.
    'Methods can be created by dragging stored procedures from the Server
    'Explorer onto the O/R Designer which can be accessed by double-clicking
    'on .DBML file in the Solution Explorer.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - stored procedures not supported")>
    Public Sub LinqToSqlStoredProc02()
      'WORKAROUND: Customers_By_City not available => changed to  CustomersByCity
      Dim custQuery = DB.CustomersByCity("London")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a stored procedure to return the Customer 'SEVES' and all it's Orders.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - stored procedures not supported")>
    Public Sub LinqToSqlStoredProc04()

      'WORKAROUND: Get_Customer_And_Orders not available => changed to  GetCustomerAndOrders
      Dim result = DB.GetCustomerAndOrders("SEVES")

      'WORKAROUND: original code: "Dim customer As IEnumerable(Of Get_Customer_And_OrdersResult) = result" changed to following line
      Dim customer As IEnumerable(Of CustomerResultSet) = result.GetResult(Of CustomerResultSet)()

      TestExecutor.Execute(customer, MethodBase.GetCurrentMethod())

      'WORKAROUND: in c# but not in vb
      'TestExecutor.Execute("********** Orders Result-set ***********");
      '      IEnumerable<OrdersResultSet> orders = result.GetResult<OrdersResultSet>();
      '      TestExecutor.Execute(orders);
    End Sub

    'This sample uses a stored procedure that returns an out parameter.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - stored procedures not supported")>
    Public Sub LinqToSqlStoredProc05()
      Dim totalSales? = 0@

      Dim customerID = "ALFKI"

      ' Out parameters are passed by ByRef, to support scenarios where
      ' the parameter is In or Out.  In this case, the parameter is only
      ' out.
      'WORKAROUND: Changed CustOrderTotal to CustomerTotalSales
      DB.CustomerTotalSales(customerID, totalSales)

      TestExecutor.Execute(totalSales, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a method mapped to the 'ProductsUnderThisUnitPrice' function
    'in Northwind database to return products with unit price less than $10.00.
    'Methods can be created by dragging database functions from the Server
    'Explorer onto the O/R Designer which can be accessed by double-clicking
    'on the .DBML file in the Solution Explorer.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - stored procedures not supported")>
    Public Sub LinqToSqlStoredProc06()
      Dim cheapProducts = DB.ProductsUnderThisUnitPrice(10D)

      TestExecutor.Execute(cheapProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample queries against a collection of products returned by
    'ProductsUnderThisUnitPrice' method. The method was created from the database
    'function 'ProductsUnderThisUnitPrice' in Northwind database.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - stored procedures not supported")>
    Public Sub LinqToSqlStoredProc07()
      Dim cheapProducts = From prod In DB.ProductsUnderThisUnitPrice(10D) _
                          Where prod.Discontinued = True

      TestExecutor.Execute(cheapProducts, MethodBase.GetCurrentMethod())
    End Sub

  End Class
End Namespace
