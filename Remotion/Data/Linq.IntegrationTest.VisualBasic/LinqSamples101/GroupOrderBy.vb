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
  Public Class GroupOrderBy
    Inherits Executor
    '<Category("ORDER BY")> _
    ' <Title("OrderBy - Simple")> _
    ' <Description("This sample uses Order By to sort Employees " & _
    '              "by hire date.")> _
    Public Sub LinqToSqlOrderBy01()
      Dim empQuery = From emp In db.Employees _
            Order By emp.HireDate

      serializer.Serialize(empQuery)
    End Sub

    '<Category("ORDER BY")> _
    '<Title("OrderBy - With Where")> _
    '<Description("This sample uses Where and Order By to sort Orders " & _
    '             "shipped to London by freight.")> _
    Public Sub LinqToSqlOrderBy02()
      Dim londonOrders = From ord In db.Orders _
            Where ord.ShipCity = "London" _
            Order By ord.Freight

      serializer.Serialize(londonOrders)
    End Sub

    '<Category("ORDER BY")> _
    '<Title("OrderByDescending")> _
    '<Description("This sample uses Order By to sort Products " & _
    '             "by unit price from highest to lowest.")> _
    Public Sub LinqToSqlOrderBy03()
      Dim sortedProducts = From prod In db.Products _
            Order By prod.UnitPrice Descending

      serializer.Serialize(sortedProducts)
    End Sub

    '<Category("ORDER BY")> _
    '<Title("ThenBy")> _
    '<Description("This sample uses a compound Order By to sort Customers " & _
    '             "by city and then contact name.")> _
    Public Sub LinqToSqlOrderBy04()
      Dim custQuery = From cust In db.Customers _
            Select cust _
            Order By cust.City, cust.ContactName

      serializer.Serialize(custQuery)
    End Sub

    '<Category("ORDER BY")> _
    '<Title("ThenByDescending")> _
    '<Description("This sample uses Order By to sort Orders from EmployeeID 1 " & _
    '             "by ship-to country, and then by freight from highest to lowest.")> _
    Public Sub LinqToSqlOrderBy05()
      Dim ordQuery = From ord In db.Orders _
            Where ord.EmployeeID = 1 _
            Order By ord.ShipCountry, ord.Freight Descending

      serializer.Serialize(ordQuery)
    End Sub

    '  <Category("ORDER BY")> _
    '<Title("OrderBy - Group By")> _
    '<Description("This sample uses Order By, Max and Group By to find the Products that have " & _
    '             "the highest unit price in each category, and sorts the group by category id.")> _
    Public Sub LinqToSqlOrderBy06()
      Dim categories = From prod In db.Products _
            Group prod By prod.CategoryID Into Group _
            Order By CategoryID _
            Select Group, _
            MostExpensiveProducts = _
            From prod2 In Group _
            Where prod2.UnitPrice = _
                  Group.Max(Function(prod3) prod3.UnitPrice)

      serializer.Serialize(categories)
    End Sub
  End Class
End Namespace
