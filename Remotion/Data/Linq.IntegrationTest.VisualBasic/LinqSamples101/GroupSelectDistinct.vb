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
  Public Class GroupSelectDistinct
    Inherits Executor

    Public Class Name
      Public FirstName As String
      Public LastName As String
    End Class

    '    <Category("SELECT/DISTINCT")> _
    '<Title("Select - Simple")> _
    '<Description("This sample uses Select to return a sequence of just the " & _
    '             "Customers' contact names.")> _
    Public Sub LinqToSqlSelect01()
      Dim contactList = From cust In db.Customers _
            Select cust.ContactName

      serializer.Serialize(contactList)
    End Sub

    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Anonymous Type 1")> _
    '<Description("This sample uses Select and anonymous types to return " & _
    '             "a sequence of just the Customers' contact names and phone numbers.")> _
    Public Sub LinqToSqlSelect02()
      Dim nameAndNumber = From cust In db.Customers _
            Select cust.ContactName, cust.Phone

      serializer.Serialize(nameAndNumber)
    End Sub


    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Anonymous Type 2")> _
    '<Description("This sample uses Select and anonymous types to return " & _
    '             "a sequence of just the Employees' names and phone numbers, " & _
    '             "with the FirstName and LastName fields combined into a single field, 'Name', " & _
    '             "and the HomePhone field renamed to Phone in the resulting sequence.")> _
    Public Sub LinqToSqlSelect03()
      Dim nameAndNumber = From emp In db.Employees _
            Select Name = emp.FirstName & " " & emp.LastName, _
            Phone = emp.HomePhone

      serializer.Serialize(nameAndNumber)
    End Sub

    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Anonymous Type 3")> _
    '<Description("This sample uses Select and anonymous types to return " & _
    '             "a sequence of all Products' IDs and a calculated value " & _
    '             "called HalfPrice which is set to the Product's UnitPrice " & _
    '             "divided by 2.")> _
    Public Sub LinqToSqlSelect04()
      Dim prices = From prod In db.Products _
            Select prod.ProductID, HalfPrice = prod.UnitPrice / 2

      serializer.Serialize(prices)
    End Sub

    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Conditional ")> _
    '<Description("This sample uses Select and a conditional statment to return a sequence of product " & _
    '             " name and product availability.")> _
    Public Sub LinqToSqlSelect05()
      Dim inStock = From prod In db.Products _
            Select prod.ProductName, _
            Availability = If((prod.UnitsInStock - prod.UnitsOnOrder) < 0, _
                              "Out Of Stock", _
                              "In Stock")
      serializer.Serialize(inStock)
    End Sub

    ' <Category("SELECT/DISTINCT")> _
    '<Title("Select - Named Type")> _
    '<Description("This sample uses Select and a known type to return a sequence of employee names.")> _
    Public Sub LinqToSqlSelect06()
      Dim names = From emp In db.Employees _
            Select New Name With {.FirstName = emp.FirstName, _
            .LastName = emp.LastName}

      serializer.Serialize(names)
    End Sub

    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Filtered")> _
    '<Description("This sample uses Select and Where clauses to return a sequence of " & _
    '             "just the London Customers' contact names.")> _
    Public Sub LinqToSqlSelect07()
      Dim londonNames = From cust In db.Customers _
            Where cust.City = "London" _
            Select cust.ContactName

      serializer.Serialize(londonNames)
    End Sub

    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Shaped")> _
    '<Description("This sample uses Select and anonymous types to return " & _
    '             "a shaped subset of the data about Customers.")> _
    Public Sub LinqToSqlSelect08()
      Dim customers = From cust In db.Customers _
            Select cust.CustomerID, CompanyInfo = New With {cust.CompanyName, _
            cust.City, _
            cust.Country}, _
            ContactInfo = New With {cust.ContactName, _
            cust.ContactTitle}

      serializer.Serialize(customers)
    End Sub

    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Nested")> _
    '<Description("This sample uses nested queries to return a sequence of " & _
    '             "all orders containing their OrderID, a subsequence of the " & _
    '             "items in the order where there is a discount, and the money " & _
    '             "saved if shipping is not included.")> _
    'WORKAROUND: Northwind doesn't offer OrderDetails - changed to OrderDetails
    Public Sub LinqToSqlSelect09()
      Dim orders = From ord In db.Orders _
            Select ord.OrderID, DiscountedProducts = (From od In ord.OrderDetails _
            Where od.Discount > 0.0), _
            FreeShippingDiscount = ord.Freight

      serializer.Serialize(orders)
    End Sub

    '' Phone converter that converts a phone number to 
    '' an international format based on its country.
    '' This sample only supports USA and UK formats, for 
    '' phone numbers from the Northwind database.
    Public Function PhoneNumberConverter(ByVal Country As String, ByVal Phone As String) As String
      Phone = Phone.Replace(" ", "").Replace(")", ")-")
      Select Case Country
        Case "USA"
          Return "1-" & Phone
        Case "UK"
          Return "44-" & Phone
        Case Else
          Return Phone
      End Select
    End Function

    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Local Method Call 1")> _
    '<Description("This sample uses a Local Method Call to " & _
    '             "'PhoneNumberConverter' to convert Phone number " & _
    '             "to an international format.")> _
    Public Sub LinqToSqlLocalMethodCall01()

      Dim q = From c In db.Customers _
            Where c.Country = "UK" Or c.Country = "USA" _
            Select _
            c.CustomerID, c.CompanyName, Phone = c.Phone, InternationalPhone = PhoneNumberConverter(c.Country, c.Phone)

      serializer.Serialize(q)
    End Sub

    '<Category("SELECT/DISTINCT")> _
    '<Title("Select - Local Method Call 2")> _
    '<Description("This sample uses a Local Method Call to " & _
    '             "convert phone numbers to an international format " & _
    '             "and create XDocument.")> _
    Public Sub LinqToSqlLocalMethodCall02()

      Dim doc = <Customers>
                  <%= From c In db.Customers _
                    Where c.Country = "UK" Or c.Country = "USA" _
                    Select <Customer CustomerID=<%= c.CustomerID %>
                             CompanyName=<%= c.CompanyName %>
                             InternationalPhone=<%= PhoneNumberConverter(c.Country, c.Phone) %>/> %>
                </Customers>

      serializer.Serialize(doc.ToString())
    End Sub


    '<Category("SELECT/DISTINCT")> _
    '<Title("Distinct")> _
    '<Description("This sample uses Distinct to select a sequence of the unique cities " & _
    '             "that have Customers.")> _
    Public Sub LinqToSqlSelect10()
      Dim cities = From cust In db.Customers _
            Select cust.City _
            Distinct

      serializer.Serialize(cities)
    End Sub
  End Class
End Namespace
