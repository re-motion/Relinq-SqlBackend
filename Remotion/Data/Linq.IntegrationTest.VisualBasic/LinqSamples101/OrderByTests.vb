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
  Public Class OrderByTests
    Inherits TestBase

    'This sample uses Order By to sort Employees by hire date.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - bytes not supported in relinq")>
    Public Sub LinqToSqlOrderBy01()
      Dim empQuery = From emp In DB.Employees _
            Order By emp.HireDate

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Where and Order By to sort Orders shipped to London by freight.
    <Test()>
    <Ignore("TODO RM-3197: Predicate LambdaExpressions are not correctly resolved if the lambda's parameter is used in a VB string comparison")>
    Public Sub LinqToSqlOrderBy02()
      Dim londonOrders = From ord In DB.Orders _
            Where ord.ShipCity = "London" _
            Order By ord.Freight

      TestExecutor.Execute(londonOrders, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Order By to sort Products
    'by unit price from highest to lowest.
    <Test()>
    Public Sub LinqToSqlOrderBy03()
      Dim sortedProducts = From prod In DB.Products _
            Order By prod.UnitPrice Descending

      TestExecutor.Execute(sortedProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a compound Order By to sort Customers
    'by city and then contact name.
    <Test()>
    Public Sub LinqToSqlOrderBy04()
      Dim custQuery = From cust In DB.Customers _
            Select cust _
            Order By cust.City, cust.ContactName

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Order By to sort Orders from EmployeeID 1
    'by ship-to country, and then by freight from highest to lowest.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - doesn't support '=' in where ? ")>
    Public Sub LinqToSqlOrderBy05()
      Dim ordQuery = From ord In DB.Orders _
            Where ord.EmployeeID = 1 _
            Order By ord.ShipCountry, ord.Freight Descending

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Order By, Max and Group By to find the Products that have
    'the highest unit price in each category, and sorts the group by category id.
    <Test()>
    <Ignore("Bug or missing feature in re-linq: Argument type 'System.Linq.IGrouping`2[System.Nullable`1[System.Int32],Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind.Product]' does not match the corresponding member type 'System.Collections.Generic.IEnumerable`1[Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind.Product]'")>
    Public Sub LinqToSqlOrderBy06()
      Dim categories = From prod In DB.Products _
            Group prod By prod.CategoryID Into Group _
            Order By CategoryID _
            Select Group, _
            MostExpensiveProducts = _
            From prod2 In Group _
            Where prod2.UnitPrice = _
                  Group.Max(Function(prod3) prod3.UnitPrice)

      TestExecutor.Execute(categories, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
