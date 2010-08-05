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
  Public Class StringDateFunctionsTests
    Inherits TestBase

    'This sample uses the & operator to concatenate string fields
    'and string literals in forming the Customers' calculated
    'Location value.
    <Test()>
    Public Sub LinqToSqlString01()
      Dim custQuery = From cust In db.Customers _
            Select cust.CustomerID, _
            Location = cust.City & ", " & cust.Country

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Length property to find all Products whose
    'name is shorter than 10 characters.
    <Test()>
    Public Sub LinqToSqlString02()
      Dim shortProducts = From prod In db.Products _
            Where prod.ProductName.Length < 10

      TestExecutor.Execute(shortProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Contains method to find all Customers whose
    'contact name contains 'Anders'.
    <Test()>
    Public Sub LinqToSqlString03()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.Contains("Anders")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the IndexOf method to find the first instance of
    'a space in each Customer's contact name.
    <Test()>
    Public Sub LinqToSqlString04()
      Dim custQuery = From cust In db.Customers _
            Select cust.ContactName, SpacePos = cust.ContactName.IndexOf(" ")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the StartsWith method to find Customers whose contact name starts with 'Maria'.
    <Test()>
    Public Sub LinqToSqlString05()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.StartsWith("Maria")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the StartsWith method to find Customers whose " & _
    'contact name ends with 'Anders'.")> _
    <Test()>
    Public Sub LinqToSqlString06()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.EndsWith("Anders")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Substring method to return Product names starting from the fourth letter.
    <Test()>
    Public Sub LinqToSqlString07()
      Dim prodQuery = From prod In db.Products _
            Select prod.ProductName.Substring(3)

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Substring method to find Employees whose
    'home phone numbers have '555' as the seventh through ninth digits.
    <Test()>
    Public Sub LinqToSqlString08()
      Dim empQuery = From emp In db.Employees _
            Where emp.HomePhone.Substring(6, 3) = "555"

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the ToUpper method to return Employee names
    'where the last name has been converted to uppercase.
    <Test()>
    Public Sub LinqToSqlString09()
      Dim empQuery = From emp In db.Employees _
            Select LastName = emp.LastName.ToUpper(), emp.FirstName

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the ToLower method to return Category names
    'that have been converted to lowercase.
    <Test()>
    Public Sub LinqToSqlString10()
      Dim categoryQuery = From category In db.Categories _
            Select category.CategoryName.ToLower()

      TestExecutor.Execute(categoryQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Trim method to return the first five
    'digits of Employee home phone numbers, with leading and
    'trailing spaces removed.
    <Test()>
    Public Sub LinqToSqlString11()
      Dim empQuery = From emp In db.Employees _
            Select emp.HomePhone.Substring(0, 5).Trim()

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Insert method to return a sequence of
    'employee phone numbers that have a ) in the fifth position, 
    'inserting a : after the ).
    <Test()>
    Public Sub LinqToSqlString12()
      Dim empQuery = From emp In db.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Insert(5, ":")

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Insert method to return a sequence of
    'employee phone numbers that have a ) in the fifth position,
    'removing all characters starting from the tenth character.
    <Test()>
    Public Sub LinqToSqlString13()
      Dim empQuery = From emp In db.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Remove(9)

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Insert method to return a sequence of
    'employee phone numbers that have a ) in the fifth position,
    'removing the first six characters.
    <Test()>
    Public Sub LinqToSqlString14()
      Dim empQuery = From emp In db.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Remove(0, 6)

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Replace method to return a sequence of
    'Supplier information where the Country field has had
    'UK replaced with United Kingdom and USA replaced with
    'United States of America.")> _
    <Test()>
    Public Sub LinqToSqlString15()
      Dim supplierQuery = From supplier In db.Suppliers _
            Select supplier.CompanyName, _
            Country = supplier.Country.Replace("UK", "United Kingdom") _
            .Replace("USA", "United States of America")

      TestExecutor.Execute(supplierQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the DateTime's Year property to " & _
    'find Orders placed in 1997.")> _
    <Test()>
    Public Sub LinqToSqlString16()
      Dim ordersIn97 = From ord In db.Orders _
            Where ord.OrderDate.Value.Year = 1997

      TestExecutor.Execute(ordersIn97, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the DateTime's Month property to
    'find Orders placed in December.
    <Test()>
    Public Sub LinqToSqlString17()
      Dim decemberOrders = From ord In db.Orders _
            Where ord.OrderDate.Value.Month = 12

      TestExecutor.Execute(decemberOrders, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the DateTime's Day property to
    'find Orders placed on the 31st day of the month.
    <Test()>
    Public Sub LinqToSqlString18()
      Dim ordQuery = From ord In db.Orders _
            Where ord.OrderDate.Value.Day = 31

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
