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
  '<Title("LINQ to SQL Samples")> <Prefix("LinqToSql")> _
  Public Class GroupWhere
    Inherits Executor
    'Private db As Northwind 'NorthwindDataContext
    'Private newDB As NorthwindInheritance
    '<Category("Your First LINQ Query")> _
    '<Title("Simple Filtering")> _
    '<Description("This sample uses a Where clause to filter for Customers in London.")> _

    'TODO: Methodnames are different to C# LinqSamples Methodnames. Starts with different Method

    Public Sub LinqToSqlFirst02()

      'Only return customers from London
      Dim londonCustomers = From cust In db.Customers _
            Where cust.City = "London" _
            Select cust.CompanyName, cust.City, cust.Country

      'Execute the query and print out the results
      For Each custRow In londonCustomers
        serializer.Serialize ("Company: " & custRow.CompanyName & vbTab & _
                              "City: " & custRow.City & vbTab & _
                              "Country: " & custRow.Country)
      Next
    End Sub


    '<Category("WHERE")> _
    '<Title("Where - 1")> _
    '<Description("This sample uses a Where clause to filter for Employees hired " & _
    '"during or after 1994.")> _
    Public Sub LinqToSqlWhere01()
      Dim hiredAfter1994 = From emp In db.Employees _
            Where emp.HireDate >= #1/1/1994# _
            Select emp

      serializer.Serialize (hiredAfter1994)
    End Sub

    '    <Category("WHERE")> _
    '<Title("Where - 2")> _
    '<Description("This sample uses a Where clause to filter for Products that have stock below their " & _
    '             "reorder level and are not discontinued.")> _
    Public Sub LinqToSqlWhere02()
      Dim needToOrder = From prod In db.Products _
            Where prod.UnitsInStock <= prod.ReorderLevel _
                  AndAlso Not prod.Discontinued _
            Select prod

      serializer.Serialize (needToOrder)
    End Sub

    '<Category("WHERE")> _
    '<Title("Where - 3")> _
    '<Description("This sample uses a Where clause to filter out Products that are either " & _
    '             "discontinued or that have a UnitPrice greater than 10.")> _
    Public Sub LinqToSqlWhere03()
      Dim prodQuery = From prod In db.Products _
            Where prod.UnitPrice > 10.0# OrElse prod.Discontinued

      serializer.Serialize (prodQuery)
    End Sub

    '    <Category("WHERE")> _
    '<Title("Where - 4")> _
    '<Description("This sample uses two Where clauses to filter out Products that are discontinued " & _
    '             "and with UnitPrice greater than 10")> _
    Public Sub LinqToSqlWhere04()

      Dim prodQuery = From prod In db.Products _
            Where prod.UnitPrice > 10D _
            Where prod.Discontinued

      serializer.Serialize (prodQuery)
    End Sub


    '<Category("WHERE")> _
    '<Title("First - Simple")> _
    '<Description("This sample uses First to select the first Shipper in the table.")> _
    Public Sub LinqToSqlWhere05()
      Dim shipper = db.Shippers.First()

      serializer.Serialize (shipper)
    End Sub


    '<Category("WHERE")> _
    '<Title("First - Element")> _
    '<Description("This sample uses Take to select the first Customer with CustomerID 'BONAP'.")> _
    Public Sub LinqToSqlWhere06()
      Dim customer = From cust In db.Customers _
            Where cust.CustomerID = "BONAP" _
            Take 1

      serializer.Serialize (customer)
    End Sub

    '<Category("WHERE")> _
    '<Title("First - Condition")> _
    '<Description("This sample uses First to select an Order with freight greater than 10.00.")> _
    Public Sub LinqToSqlWhere07()
      Dim firstOrd = (From ord In db.Orders _
            Where ord.Freight > 10D _
            Select ord).First()

      serializer.Serialize (firstOrd)
    End Sub


    'TODO: in vb but not in c#
    'Additional methods

    '  <Category("Your First LINQ Query")> _
    '<Title("Select 2 columns")> _
    '<Description("This sample selects 2 columns and returns the data from the database.")> _
    '  Public Sub LinqToSqlFirst01()

    '    'Instead of returning the entire Customers table, just return the
    '    'CompanyName and Country
    '    Dim londonCustomers = From cust In db.Customers _
    '                          Select cust.CompanyName, cust.Country

    '    'Execute the query and print out the results
    '    For Each custRow In londonCustomers
    '      serializer.Serialize("Company: " & custRow.CompanyName & vbTab & _
    '                        "Country: " & custRow.Country)
    '    Next
    '  End Sub


    'Linq specific ?
  End Class
End Namespace
