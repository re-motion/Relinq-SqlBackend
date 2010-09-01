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
  Public Class SelectDistinctTests
    Inherits TestBase

    Public Class Name
      Public FirstName As String
      Public LastName As String
    End Class

    'This sample uses Select to return a sequence of just the
    'Customers contact names.
    <Test()>
    Public Sub LinqToSqlSelect01()
      Dim contactList = From cust In DB.Customers _
            Select cust.ContactName

      TestExecutor.Execute(contactList, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and anonymous types to return
    'a sequence of just the Customers contact names and phone numbers.
    <Test()>
    Public Sub LinqToSqlSelect02()
      Dim nameAndNumber = From cust In DB.Customers _
            Select cust.ContactName, cust.Phone

      TestExecutor.Execute(nameAndNumber, MethodBase.GetCurrentMethod())
    End Sub


    'This sample uses Select and anonymous types to return 
    'a sequence of just the Employees' names and phone numbers,
    'with the FirstName and LastName fields combined into a single field, 'Name',
    'and the HomePhone field renamed to Phone in the resulting sequence.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - String.Concat is not supported, test works in c# but not in vb")>
    Public Sub LinqToSqlSelect03()
      Dim nameAndNumber = From emp In DB.Employees _
            Select Name = emp.FirstName & " " & emp.LastName, _
            Phone = emp.HomePhone

      TestExecutor.Execute(nameAndNumber, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and anonymous types to return
    'a sequence of all Products' IDs and a calculated value 
    'called HalfPrice which is set to the Product's UnitPrice 
    'divided by 2.")> _
    <Test()>
    <Ignore("Bug or missing feature in Relinq - rounding differences ?")>
    Public Sub LinqToSqlSelect04()
      Dim prices = From prod In DB.Products _
            Select prod.ProductID, HalfPrice = prod.UnitPrice / 2

      TestExecutor.Execute(prices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and a conditional statment to return a sequence of product
    'name and product availability.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - nullables not supported")>
    Public Sub LinqToSqlSelect05()
      Dim inStock = From prod In DB.Products _
            Select prod.ProductName, _
            Availability = If((prod.UnitsInStock - prod.UnitsOnOrder) < 0, _
                              "Out Of Stock", _
                              "In Stock")
      TestExecutor.Execute(inStock, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and a known type to return a sequence of employee names.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - cannot be translated to SQL tex0t by this SQL generator ")>
    Public Sub LinqToSqlSelect06()
      Dim names = From emp In DB.Employees _
            Select New Name With {.FirstName = emp.FirstName, _
            .LastName = emp.LastName}

      TestExecutor.Execute(names, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and Where clauses to return a sequence of
    'just the London Customers' contact names.
    'Bug or missing feature in Relinq - test works in c# but not in vb
    <Test()>
    <Ignore("TODO RM-3197: Predicate LambdaExpressions are not correctly resolved if the lambda's parameter is used in a VB string comparison")>
    Public Sub LinqToSqlSelect07()
      Dim londonNames = From cust In DB.Customers _
            Where cust.City = "London" _
            Select cust.ContactName

      TestExecutor.Execute(londonNames, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and anonymous types to return
    'a shaped subset of the data about Customers.
    <Test()>
    Public Sub LinqToSqlSelect08()
      Dim customers = From cust In DB.Customers _
            Select cust.CustomerID, CompanyInfo = New With {cust.CompanyName, _
            cust.City, _
            cust.Country}, _
            ContactInfo = New With {cust.ContactName, _
            cust.ContactTitle}

      TestExecutor.Execute(customers, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses nested queries to return a sequence of
    'all orders containing their OrderID, a subsequence of the
    'items in the order where there is a discount, and the money 
    'saved if shipping is not included.")> _
    'WORKAROUND: Northwind doesn't offer OrderDetails - changed to OrderDetails
    <Test()>
    <Ignore("Bug or missing feature in re-linq: Argument type 'System.Linq.IQueryable`1[Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind.OrderDetail]' does not match the corresponding member type 'System.Collections.Generic.IEnumerable`1[Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind.OrderDetail]'")>
    Public Sub LinqToSqlSelect09()
      Dim orders = From ord In DB.Orders _
            Select ord.OrderID, DiscountedProducts = (From od In ord.OrderDetails _
            Where od.Discount > 0.0), _
            FreeShippingDiscount = ord.Freight

      TestExecutor.Execute(orders, MethodBase.GetCurrentMethod())
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

    'This sample uses a Local Method Call to
    ''PhoneNumberConverter' to convert Phone number
    'to an international format.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - local method calls not supported")>
    Public Sub LinqToSqlLocalMethodCall01()

      Dim q = From c In DB.Customers _
            Where c.Country = "UK" Or c.Country = "USA" _
            Select _
            c.CustomerID, c.CompanyName, Phone = c.Phone, InternationalPhone = PhoneNumberConverter(c.Country, c.Phone)

      TestExecutor.Execute(q, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a Local Method Call to
    'convert phone numbers to an international format
    'and create XDocument.>
    'TODO need to fix SavingTestExecutor to properly handle result ?
    <Test()> _
    <Ignore("Bug or missing feature in Relinq - local method calls not supported")>
    Public Sub LinqToSqlLocalMethodCall02()

      Dim doc = <Customers>
                  <%= From c In DB.Customers _
                    Where c.Country = "UK" Or c.Country = "USA" _
                    Select <Customer CustomerID=<%= c.CustomerID %>
                             CompanyName=<%= c.CompanyName %>
                             InternationalPhone=<%= PhoneNumberConverter(c.Country, c.Phone) %>/> %>
                </Customers>

      TestExecutor.Execute(doc, MethodBase.GetCurrentMethod())
    End Sub


    'This sample uses Distinct to select a sequence of the unique cities
    'that have Customers.
    <Test()>
    Public Sub LinqToSqlSelect10()
      Dim cities = From cust In DB.Customers _
            Select cust.City _
            Distinct

      TestExecutor.Execute(cities, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
