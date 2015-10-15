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
Imports Remotion.Linq.IntegrationTests.Common


Namespace LinqSamples101
  <TestFixture()> _
  Public Class JoinTests
    Inherits TestBase

    'This sample uses foreign key navigation in the 
    'From clause to select all orders for customers in London.
    <Test()> _
    Public Sub LinqToSqlJoin01()
      Dim ordersInLondon = From cust In DB.Customers, ord In cust.Orders _
            Where cust.City = "London"

      TestExecutor.Execute(ordersInLondon, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses foreign key navigation in the
    'Where clause to filter for Products whose Supplier is in the USA
    'that are out of stock.
    <Test()> _
    Public Sub LinqToSqlJoin02()
      Dim outOfStock = From prod In DB.Products _
            Where prod.Supplier.Country = "USA" AndAlso prod.UnitsInStock = 0

      TestExecutor.Execute(outOfStock, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses foreign key navigation in the 
    'from clause to filter for employees in Seattle,
    'and also list their territories.
    <Test()> _
    <Ignore("RM-3110: Support complex columns for entities")> _
    Public Sub LinqToSqlJoin03()
      Dim seattleEmployees = From emp In DB.Employees, et In emp.EmployeeTerritories _
            Where emp.City = "Seattle" _
            Select emp.FirstName, emp.LastName, et.Territory.TerritoryDescription

      TestExecutor.Execute(seattleEmployees, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses foreign key navigation in the 
    'Select clause to filter for pairs of employees where
    'one employee reports to the other and where
    'both employees are from the same City.
    <Test()> _
    Public Sub LinqToSqlJoin04()
      Dim empQuery = From emp1 In DB.Employees, emp2 In emp1.Employees _
            Where emp1.City = emp2.City _
            Select FirstName1 = emp1.FirstName, LastName1 = emp1.LastName, _
            FirstName2 = emp2.FirstName, LastName2 = emp2.LastName, emp1.City

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample explictly joins two tables and projects results from both tables.
    <Test()> _
    Public Sub LinqToSqlJoin05()
      Dim ordCount = From cust In db.Customers _
            Group Join ord In db.Orders On cust.CustomerID Equals ord.CustomerID _
            Into orders = Group _
            Select cust.ContactName, OrderCount = orders.Count()

      TestExecutor.Execute(ordCount, MethodBase.GetCurrentMethod())
    End Sub

    'This sample explictly joins three tables and projects results from each of them.
    <Test()> _
    Public Sub LinqToSqlJoin06()
      Dim joinQuery = From cust In db.Customers _
            Group Join ord In db.Orders On cust.CustomerID Equals ord.CustomerID _
            Into ords = Group _
            Group Join emp In db.Employees On cust.City Equals emp.City _
            Into emps = Group _
            Select cust.ContactName, ords = ords.Count(), emps = emps.Count()

      TestExecutor.Execute(joinQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample shows how to get LEFT OUTER JOIN by using DefaultIfEmpty().
    'The DefaultIfEmpty() method returns Nothing when there is no Order for the Employee.
    <Test()> _
    Public Sub LinqToSqlJoin07()
      Dim empQuery = From emp In DB.Employees _
            Group Join ord In DB.Orders On emp Equals ord.Employee _
            Into ords = Group _
            From ord2 In ords.DefaultIfEmpty _
            Select emp.FirstName, emp.LastName, Order = ord2

      'Added to make query result stable.
      Dim stableResult = empQuery.AsEnumerable().OrderBy((Function(t) t.FirstName)).ThenBy((Function(t) t.LastName)).ThenBy((Function(t) t.Order.OrderID))
      TestExecutor.Execute(stableResult, MethodBase.GetCurrentMethod())
    End Sub

    'This sample projects a 'Let' expression resulting from a join.
    <Test()> _
    Public Sub LinqToSqlJoin08()
      Dim ordQuery = From cust In DB.Customers _
            Group Join ord In DB.Orders On cust.CustomerID Equals ord.CustomerID _
            Into ords = Group _
            Let Location = cust.City + cust.Country _
            From ord2 In ords _
            Select cust.ContactName, ord2.OrderID, Location

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample shows a join with a composite key.
    <Test()> _
    <Ignore("RM-3110: Support complex columns for entities")> _
    Public Sub LinqToSqlJoin09()

      'The Key keyword means that when the anonymous types are tested for equality,
      'only the OrderID field will be compared
      'WORKAROUND: Northwind doesn't offer OrderDetails - changed to OrderDetails
      Dim ordQuery = From ord In DB.Orders _
            From prod In DB.Products _
            Group Join details In DB.OrderDetails _
            On New With {Key ord.OrderID, prod.ProductID} _
            Equals New With {Key details.OrderID, details.ProductID} _
            Into details = Group _
            From d In details _
            Select ord.OrderID, prod.ProductID, d.UnitPrice

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample shows how to construct a join where one side is nullable and the other isn't.
    <Test()> _
    Public Sub LinqToSqlJoin10()
      Dim ordQuery = From ord In db.Orders _
            Group Join emp In db.Employees _
            On ord.EmployeeID Equals CType(emp.EmployeeID, Integer?) _
            Into emps = Group _
            From emp2 In emps _
            Select ord.OrderID, emp2.FirstName

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
