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
  Public Class GroupStringDateFunctions
    Inherits Executor

    'This sample uses the & operator to concatenate string fields " & _
    '         "and string literals in forming the Customers' calculated " & _
    '         "Location value.")> _
    Public Sub LinqToSqlString01()
      Dim custQuery = From cust In db.Customers _
            Select cust.CustomerID, _
            Location = cust.City & ", " & cust.Country

      serializer.Serialize(custQuery)
    End Sub

    'This sample uses the Length property to find all Products whose " & _
    '              "name is shorter than 10 characters.")> _
    Public Sub LinqToSqlString02()
      Dim shortProducts = From prod In db.Products _
            Where prod.ProductName.Length < 10

      serializer.Serialize(shortProducts)
    End Sub

    'This sample uses the Contains method to find all Customers whose " & _
    '             "contact name contains 'Anders'.")> _
    Public Sub LinqToSqlString03()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.Contains("Anders")

      serializer.Serialize(custQuery)
    End Sub

    'This sample uses the IndexOf method to find the first instance of " & _
    '             "a space in each Customer's contact name.")> _
    Public Sub LinqToSqlString04()
      Dim custQuery = From cust In db.Customers _
            Select cust.ContactName, SpacePos = cust.ContactName.IndexOf(" ")

      serializer.Serialize(custQuery)
    End Sub

    'This sample uses the StartsWith method to find Customers whose " & _
    '             "contact name starts with 'Maria'.")> _
    Public Sub LinqToSqlString05()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.StartsWith("Maria")

      serializer.Serialize(custQuery)
    End Sub

    'This sample uses the StartsWith method to find Customers whose " & _
    '             "contact name ends with 'Anders'.")> _
    Public Sub LinqToSqlString06()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.EndsWith("Anders")

      serializer.Serialize(custQuery)
    End Sub

    'This sample uses the Substring method to return Product names starting " & _
    '             "from the fourth letter.")> _
    Public Sub LinqToSqlString07()
      Dim prodQuery = From prod In db.Products _
            Select prod.ProductName.Substring(3)

      serializer.Serialize(prodQuery)
    End Sub

    'This sample uses the Substring method to find Employees whose " & _
    '             "home phone numbers have '555' as the seventh through ninth digits.")> _
    Public Sub LinqToSqlString08()
      Dim empQuery = From emp In db.Employees _
            Where emp.HomePhone.Substring(6, 3) = "555"

      serializer.Serialize(empQuery)
    End Sub

    'This sample uses the ToUpper method to return Employee names " & _
    '             "where the last name has been converted to uppercase.")> _
    Public Sub LinqToSqlString09()
      Dim empQuery = From emp In db.Employees _
            Select LastName = emp.LastName.ToUpper(), emp.FirstName

      serializer.Serialize(empQuery)
    End Sub

    'This sample uses the ToLower method to return Category names " & _
    '             "that have been converted to lowercase.")> _
    Public Sub LinqToSqlString10()
      Dim categoryQuery = From category In db.Categories _
            Select category.CategoryName.ToLower()

      serializer.Serialize(categoryQuery)
    End Sub

    'This sample uses the Trim method to return the first five " & _
    '             "digits of Employee home phone numbers, with leading and " & _
    '             "trailing spaces removed.")> _
    Public Sub LinqToSqlString11()
      Dim empQuery = From emp In db.Employees _
            Select emp.HomePhone.Substring(0, 5).Trim()

      serializer.Serialize(empQuery)
    End Sub

    'This sample uses the Insert method to return a sequence of " & _
    '             "employee phone numbers that have a ) in the fifth position, " & _
    '             "inserting a : after the ).")> _
    Public Sub LinqToSqlString12()
      Dim empQuery = From emp In db.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Insert(5, ":")

      serializer.Serialize(empQuery)
    End Sub

    'This sample uses the Insert method to return a sequence of " & _
    '             "employee phone numbers that have a ) in the fifth position, " & _
    '             "removing all characters starting from the tenth character.")> _
    Public Sub LinqToSqlString13()
      Dim empQuery = From emp In db.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Remove(9)

      serializer.Serialize(empQuery)
    End Sub

    'This sample uses the Insert method to return a sequence of " & _
    '             "employee phone numbers that have a ) in the fifth position, " & _
    '             "removing the first six characters.")> _
    Public Sub LinqToSqlString14()
      Dim empQuery = From emp In db.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Remove(0, 6)

      serializer.Serialize(empQuery)
    End Sub

    'This sample uses the Replace method to return a sequence of " & _
    '             "Supplier information where the Country field has had " & _
    '             "UK replaced with United Kingdom and USA replaced with " & _
    '             "United States of America.")> _
    Public Sub LinqToSqlString15()
      Dim supplierQuery = From supplier In db.Suppliers _
            Select supplier.CompanyName, _
            Country = supplier.Country.Replace("UK", "United Kingdom") _
            .Replace("USA", "United States of America")

      serializer.Serialize(supplierQuery)
    End Sub

    'This sample uses the DateTime's Year property to " & _
    '             "find Orders placed in 1997.")> _
    Public Sub LinqToSqlString16()
      Dim ordersIn97 = From ord In db.Orders _
            Where ord.OrderDate.Value.Year = 1997

      serializer.Serialize(ordersIn97)
    End Sub

    'This sample uses the DateTime's Month property to " & _
    '             "find Orders placed in December.")> _
    Public Sub LinqToSqlString17()
      Dim decemberOrders = From ord In db.Orders _
            Where ord.OrderDate.Value.Month = 12

      serializer.Serialize(decemberOrders)
    End Sub

    'This sample uses the DateTime's Day property to " & _
    '             "find Orders placed on the 31st day of the month.")> _
    Public Sub LinqToSqlString18()
      Dim ordQuery = From ord In db.Orders _
            Where ord.OrderDate.Value.Day = 31

      serializer.Serialize(ordQuery)
    End Sub
  End Class
End Namespace
