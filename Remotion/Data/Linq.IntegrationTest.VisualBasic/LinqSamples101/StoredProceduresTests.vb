' This file is part of the re-motion Core Framework (www.re-motion.org)
' Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
' 
' The re-motion Core Framework is free software; you can redistribute it 
' and/or modify it under the terms of the GNU Lesser General Public License 
' as published by the Free Software Foundation; either version 2.1 of the 
' License, or (at your option) any later version.
' 
' re-motion is distributed in the hope that it will be useful, 
' but WITHOUT ANY WARRANTY; without even the implied warranty of 
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
' GNU Lesser General Public License for more details.
' 
' You should have received a copy of the GNU Lesser General Public License
' along with re-motion; if not, see http://www.gnu.org/licenses.
' 
Option Infer On
Option Strict On

Imports NUnit.Framework

Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind


Namespace LinqSamples101
  Public Class StoredProceduresTests
    Inherits TestBase

    'This sample uses a stored procedure to return the number of Customers in the 'WA' Region.
    <Test()>
    <Ignore()>
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
    <Ignore()>
    Public Sub LinqToSqlStoredProc02()
      'WORKAROUND: Customers_By_City not available => changed to  CustomersByCity
      Dim custQuery = DB.CustomersByCity("London")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a stored procedure to return the Customer 'SEVES' and all it's Orders.
    <Test()>
    <Ignore()>
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

    'This sample uses a stored procedure that returns an out parameter.")> _
    <Test()>
    <Ignore()>
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

    'WORKAROUND: in c# but not in vb

    '[Category("Stored Procedures")]
    '   [Title("Single Result-Set - Multiple Possible Shapes")]
    '   [Description("This sample uses a stored procedure to return a set of " +
    'Customers in the 'WA' Region.  The result set-shape returned depends on the parameter passed in. " +
    'If the parameter equals 1, all Customer properties are returned. " +
    'If the parameter equals 2, the CustomerID, ContactName and CompanyName properties are returned.")]
    '   public void LinqToSqlStoredProc03() {
    '       TestExecutor.Execute("********** Whole Customer Result-set ***********");
    '       IMultipleResults result = db.WholeOrPartialCustomersSet(1);
    '       IEnumerable<WholeCustomersSetResult> shape1 = result.GetResult<WholeCustomersSetResult>();

    '      TestExecutor.Execute(shape1);

    '       TestExecutor.Execute();
    '       TestExecutor.Execute("********** Partial Customer Result-set ***********");
    '       result = db.WholeOrPartialCustomersSet(2);
    '       IEnumerable<PartialCustomersSetResult> shape2 = result.GetResult<PartialCustomersSetResult>();

    '      TestExecutor.Execute(shape2);
    '   }
  End Class
End Namespace
