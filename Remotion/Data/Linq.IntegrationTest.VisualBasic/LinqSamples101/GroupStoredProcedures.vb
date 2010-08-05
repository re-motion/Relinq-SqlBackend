' Copyright (c) Microsoft Corporation.  All rights reserved.
Option Infer On
Option Strict On

Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
Imports Remotion.Data.Linq.IntegrationTests.Utilities
Imports System.Data.Linq
Imports System.Text

Namespace LinqSamples101
  Public Class GroupStoredProcedures
    Inherits Executor
    '<Category("Stored Procedures")> _
    '<Title("Scalar Return")> _
    '<Description("This sample uses a stored procedure to return the number of Customers in the 'WA' Region.")> _
    Public Sub LinqToSqlStoredProc01()
      'WORKAROUND: Customers_Count_By_Region not available => changed to  CustomersCountByRegion
      Dim count = db.CustomersCountByRegion("WA")

      serializer.Serialize(count)
    End Sub

    ' <Category("Stored Procedures")> _
    '<Title("Single Resultset")> _
    '<Description("This sample uses a method mapped to the 'Customers By City' stored procedure " & _
    '             "in Northwind database to return customers from 'London'.  " & _
    '             "Methods can be created by dragging stored procedures from the Server " & _
    '             "Explorer onto the O/R Designer which can be accessed by double-clicking " & _
    '             "on .DBML file in the Solution Explorer.")> _
    Public Sub LinqToSqlStoredProc02()
      'WORKAROUND: Customers_By_City not available => changed to  CustomersByCity
      Dim custQuery = db.CustomersByCity("London")

      serializer.Serialize(custQuery)
    End Sub

    '<Category("Stored Procedures")> _
    '<Title("Multiple Result-Sets")> _
    '<Description("This sample uses a stored procedure to return the Customer 'SEVES' and all it's Orders.")> _
    Public Sub LinqToSqlStoredProc04()

      'WORKAROUND: Get_Customer_And_Orders not available => changed to  GetCustomerAndOrders
      Dim result = db.GetCustomerAndOrders("SEVES")

      serializer.Serialize("********** Customer Result-set ***********")
      'WORKAROUND: original code: "Dim customer As IEnumerable(Of Get_Customer_And_OrdersResult) = result" changed to
      Dim customer As IEnumerable(Of CustomerResultSet) = result.GetResult(Of CustomerResultSet)()

      serializer.Serialize(customer)
      serializer.Serialize(Environment.NewLine)

      'TODO in c# but not here
      'serializer.Serialize("********** Orders Result-set ***********");
      '      IEnumerable<OrdersResultSet> orders = result.GetResult<OrdersResultSet>();
      '      serializer.Serialize(orders);
    End Sub

    '<Category("Stored Procedures")> _
    '<Title("Out parameters")> _
    '<Description("This sample uses a stored procedure that returns an out parameter.")> _
    Public Sub LinqToSqlStoredProc05()
      Dim totalSales? = 0@

      Dim customerID = "ALFKI"

      ' Out parameters are passed by ByRef, to support scenarios where
      ' the parameter is In or Out.  In this case, the parameter is only
      ' out.
      'WORKARAOUND: Changed CustOrderTotal to CustomerTotalSales
      db.CustomerTotalSales(customerID, totalSales)

      serializer.Serialize(String.Format("Total Sales for Customer '{0}' = {1:C}", customerID, totalSales))
    End Sub

    'TODO: in vb but not in c#

    '<Category("Stored Procedures")> _
    '<Title("Function")> _
    '<Description("This sample uses a method mapped to the 'ProductsUnderThisUnitPrice' function " & _
    '             "in Northwind database to return products with unit price less than $10.00. " & _
    '             "Methods can be created by dragging database functions from the Server " & _
    '             "Explorer onto the O/R Designer which can be accessed by double-clicking " & _
    '             "on the .DBML file in the Solution Explorer.")> _
    'Public Sub LinqToSqlStoredProc06()
    '  Dim cheapProducts = db.ProductsUnderThisUnitPrice(10D)

    ' serializer.Serialize(cheapProducts, 0)
    'End Sub

    ''<Category("Stored Procedures")> _
    ''<Title("Query over methods")> _
    ''<Description("This sample queries against a collection of products returned by " & _
    ''             "'ProductsUnderThisUnitPrice' method. The method was created from the database  " & _
    ''             "function 'ProductsUnderThisUnitPrice' in Northwind database. ")> _
    'Public Sub LinqToSqlStoredProc07()
    '  Dim cheapProducts = From prod In db.ProductsUnderThisUnitPrice(10D) _
    '                      Where prod.Discontinued = True

    ' serializer.Serialize(cheapProducts, 0)
    'End Sub


    'TODO: in c# but not in vb

    '[Category("Stored Procedures")]
    '   [Title("Single Result-Set - Multiple Possible Shapes")]
    '   [Description("This sample uses a stored procedure to return a set of " +
    '   "Customers in the 'WA' Region.  The result set-shape returned depends on the parameter passed in. " +
    '   "If the parameter equals 1, all Customer properties are returned. " +
    '   "If the parameter equals 2, the CustomerID, ContactName and CompanyName properties are returned.")]
    '   public void LinqToSqlStoredProc03() {
    '       serializer.Serialize("********** Whole Customer Result-set ***********");
    '       IMultipleResults result = db.WholeOrPartialCustomersSet(1);
    '       IEnumerable<WholeCustomersSetResult> shape1 = result.GetResult<WholeCustomersSetResult>();

    '      serializer.Serialize(shape1);

    '       serializer.Serialize();
    '       serializer.Serialize("********** Partial Customer Result-set ***********");
    '       result = db.WholeOrPartialCustomersSet(2);
    '       IEnumerable<PartialCustomersSetResult> shape2 = result.GetResult<PartialCustomersSetResult>();

    '      serializer.Serialize(shape2);
    '   }

  End Class
End Namespace
