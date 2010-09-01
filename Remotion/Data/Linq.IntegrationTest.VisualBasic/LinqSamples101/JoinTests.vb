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


Namespace LinqSamples101
  <TestFixture()>
  Public Class JoinTests
    Inherits TestBase

    'This sample uses foreign key navigation in the 
    'From clause to select all orders for customers in London.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - works in c# but not in vb")>
    Public Sub LinqToSqlJoin01()
      Dim ordersInLondon = From cust In DB.Customers, ord In cust.Orders _
            Where cust.City = "London"

      TestExecutor.Execute(ordersInLondon, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses foreign key navigation in the
    'Where clause to filter for Products whose Supplier is in the USA
    'that are out of stock.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - works in c# but not in vb")>
    Public Sub LinqToSqlJoin02()
      Dim outOfStock = From prod In DB.Products _
            Where prod.Supplier.Country = "USA" AndAlso prod.UnitsInStock = 0

      TestExecutor.Execute(outOfStock, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses foreign key navigation in the 
    'from clause to filter for employees in Seattle,
    'and also list their territories.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - works in c# but not in vb")>
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
    <Test()>
    <Ignore("TODO RM-3197: Predicate LambdaExpressions are not correctly resolved if the lambda's parameter is used in a VB string comparison")>
    Public Sub LinqToSqlJoin04()
      Dim empQuery = From emp1 In DB.Employees, emp2 In emp1.Employees _
            Where emp1.City = emp2.City _
            Select FirstName1 = emp1.FirstName, LastName1 = emp1.LastName, _
            FirstName2 = emp2.FirstName, LastName2 = emp2.LastName, emp1.City

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample explictly joins two tables and projects results from both tables.")> _
    <Test()>
    Public Sub LinqToSqlJoin05()
      Dim ordCount = From cust In db.Customers _
            Group Join ord In db.Orders On cust.CustomerID Equals ord.CustomerID _
            Into orders = Group _
            Select cust.ContactName, OrderCount = orders.Count()

      TestExecutor.Execute(ordCount, MethodBase.GetCurrentMethod())
    End Sub

    'This sample explictly joins three tables and projects results from each of them.")> _
    <Test()>
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
    <Ignore("TODO RM-3198: InvalidOperationException is thrown when a comparison or join condition involves a nullable and a non-nullable expression")>
    <Test()>
    Public Sub LinqToSqlJoin07()
      Dim empQuery = From emp In DB.Employees _
            Group Join ord In DB.Orders On emp Equals ord.Employee _
            Into ords = Group _
            From ord2 In ords.DefaultIfEmpty _
            Select emp.FirstName, emp.LastName, Order = ord2

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample projects a 'Let' expression resulting from a join.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - String.Concat not supported")>
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
    <Test()>
    <Ignore("TODO RM-3110: Support complex columns for entities")>
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
    <Test()>
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
