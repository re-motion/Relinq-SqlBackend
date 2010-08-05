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


Namespace LinqSamples101
  Public Class GroupJoin
    Inherits Executor
    '  <Category("JOIN")> _
    '<Title("SelectMany - 1 to Many - 1")> _
    '<Description("This sample uses foreign key navigation in the " & _
    '"From clause to select all orders for customers in London.")> _
    Public Sub LinqToSqlJoin01()
      Dim ordersInLondon = From cust In db.Customers, ord In cust.Orders _
            Where cust.City = "London"

      serializer.Serialize(ordersInLondon)
    End Sub

    'This sample uses foreign key navigation in the " & _
    '             "Where clause to filter for Products whose Supplier is in the USA " & _
    '             "that are out of stock.")> _
    Public Sub LinqToSqlJoin02()
      Dim outOfStock = From prod In db.Products _
            Where prod.Supplier.Country = "USA" AndAlso prod.UnitsInStock = 0

      serializer.Serialize(outOfStock)
    End Sub

    'This sample uses foreign key navigation in the " & _
    '             "from clause to filter for employees in Seattle, " & _
    '             "and also list their territories.")> _
    Public Sub LinqToSqlJoin03()
      Dim seattleEmployees = From emp In db.Employees, et In emp.EmployeeTerritories _
            Where emp.City = "Seattle" _
            Select emp.FirstName, emp.LastName, et.Territory.TerritoryDescription

      serializer.Serialize(seattleEmployees)
    End Sub

    'This sample uses foreign key navigation in the " & _
    '             "Select clause to filter for pairs of employees where " & _
    '             "one employee reports to the other and where " & _
    '             "both employees are from the same City.")> _
    Public Sub LinqToSqlJoin04()
      Dim empQuery = From emp1 In db.Employees, emp2 In emp1.Employees _
            Where emp1.City = emp2.City _
            Select FirstName1 = emp1.FirstName, LastName1 = emp1.LastName, _
            FirstName2 = emp2.FirstName, LastName2 = emp2.LastName, emp1.City

      serializer.Serialize(empQuery)
    End Sub

    'This sample explictly joins two tables and projects results from both tables.")> _
    Public Sub LinqToSqlJoin05()
      Dim ordCount = From cust In db.Customers _
            Group Join ord In db.Orders On cust.CustomerID Equals ord.CustomerID _
            Into orders = Group _
            Select cust.ContactName, OrderCount = orders.Count()

      serializer.Serialize(ordCount)
    End Sub

    'This sample explictly joins three tables and projects results from each of them.")> _
    Public Sub LinqToSqlJoin06()
      Dim joinQuery = From cust In db.Customers _
            Group Join ord In db.Orders On cust.CustomerID Equals ord.CustomerID _
            Into ords = Group _
            Group Join emp In db.Employees On cust.City Equals emp.City _
            Into emps = Group _
            Select cust.ContactName, ords = ords.Count(), emps = emps.Count()

      serializer.Serialize(joinQuery)
    End Sub

    'This sample shows how to get LEFT OUTER JOIN by using DefaultIfEmpty(). " & _
    '             "The DefaultIfEmpty() method returns Nothing when there is no Order for the Employee.")> _
    Public Sub LinqToSqlJoin07()
      Dim empQuery = From emp In db.Employees _
            Group Join ord In db.Orders On emp Equals ord.Employee _
            Into ords = Group _
            From ord2 In ords.DefaultIfEmpty _
            Select emp.FirstName, emp.LastName, Order = ord2

      serializer.Serialize(empQuery)
    End Sub

    'This sample projects a 'Let' expression resulting from a join.")> _
    Public Sub LinqToSqlJoin08()
      Dim ordQuery = From cust In db.Customers _
            Group Join ord In db.Orders On cust.CustomerID Equals ord.CustomerID _
            Into ords = Group _
            Let Location = cust.City + cust.Country _
            From ord2 In ords _
            Select cust.ContactName, ord2.OrderID, Location

      serializer.Serialize(ordQuery)
    End Sub

    'This sample shows a join with a composite key.")> _
    Public Sub LinqToSqlJoin09()

      'The Key keyword means that when the anonymous types are tested for equality,
      'only the OrderID field will be compared
      'WORKAROUND: Northwind doesn't offer OrderDetails - changed to OrderDetails
      Dim ordQuery = From ord In db.Orders _
            From prod In db.Products _
            Group Join details In db.OrderDetails _
            On New With {Key ord.OrderID, prod.ProductID} _
            Equals New With {Key details.OrderID, details.ProductID} _
            Into details = Group _
            From d In details _
            Select ord.OrderID, prod.ProductID, d.UnitPrice

      serializer.Serialize(ordQuery)
    End Sub

    'This sample shows how to construct a join where one side is nullable and the other isn't.")> _
    Public Sub LinqToSqlJoin10()
      Dim ordQuery = From ord In db.Orders _
            Group Join emp In db.Employees _
            On ord.EmployeeID Equals CType(emp.EmployeeID, Integer?) _
            Into emps = Group _
            From emp2 In emps _
            Select ord.OrderID, emp2.FirstName

      serializer.Serialize(ordQuery)
    End Sub
  End Class
End Namespace
