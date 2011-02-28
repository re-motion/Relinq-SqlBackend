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
Imports Remotion.Data.Linq.IntegrationTests.Common


Namespace LinqSamples101
  <TestFixture()> _
  Public Class StringDateFunctionsTests
    Inherits TestBase

    'This sample uses the & operator to concatenate string fields
    'and string literals in forming the Customers' calculated
    'Location value.
    <Test()> _
    Public Sub LinqToSqlString01()
      Dim custQuery = From cust In DB.Customers _
            Select cust.CustomerID, _
            Location = cust.City & ", " & cust.Country

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Length property to find all Products whose
    'name is shorter than 10 characters.
    <Test()> _
    Public Sub LinqToSqlString02()
      Dim shortProducts = From prod In db.Products _
            Where prod.ProductName.Length < 10

      TestExecutor.Execute(shortProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Contains method to find all Customers whose
    'contact name contains 'Anders'.
    <Test()> _
    Public Sub LinqToSqlString03()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.Contains("Anders")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the IndexOf method to find the first instance of
    'a space in each Customer's contact name.
    <Test()> _
    <Ignore("RM-3308: The SQL generated for some string manipulation functions doesn't deal with spaces correctly")> _
    Public Sub LinqToSqlString04()
      Dim custQuery = From cust In DB.Customers _
            Select cust.ContactName, SpacePos = cust.ContactName.IndexOf(" ")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the StartsWith method to find Customers whose contact name starts with 'Maria'.
    <Test()> _
    Public Sub LinqToSqlString05()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.StartsWith("Maria")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the StartsWith method to find Customers whose
    'contact name ends with 'Anders'.
    <Test()> _
    Public Sub LinqToSqlString06()
      Dim custQuery = From cust In db.Customers _
            Where cust.ContactName.EndsWith("Anders")

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Substring method to return Product names starting from the fourth letter.
    <Test()> _
    Public Sub LinqToSqlString07()
      Dim prodQuery = From prod In db.Products _
            Select prod.ProductName.Substring(3)

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Substring method to find Employees whose
    'home phone numbers have '555' as the seventh through ninth digits.
    <Test()> _
    Public Sub LinqToSqlString08()
      Dim empQuery = From emp In DB.Employees _
            Where emp.HomePhone.Substring(6, 3) = "555"

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the ToUpper method to return Employee names
    'where the last name has been converted to uppercase.
    <Test()> _
    Public Sub LinqToSqlString09()
      Dim empQuery = From emp In db.Employees _
            Select LastName = emp.LastName.ToUpper(), emp.FirstName

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the ToLower method to return Category names
    'that have been converted to lowercase.
    <Test()> _
    Public Sub LinqToSqlString10()
      Dim categoryQuery = From category In db.Categories _
            Select category.CategoryName.ToLower()

      TestExecutor.Execute(categoryQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Trim method to return the first five
    'digits of Employee home phone numbers, with leading and
    'trailing spaces removed.
    <Test()> _
    Public Sub LinqToSqlString11()
      Dim empQuery = From emp In DB.Employees _
            Select emp.HomePhone.Substring(0, 5).Trim()

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Insert method to return a sequence of
    'employee phone numbers that have a ) in the fifth position, 
    'inserting a : after the ).
    <Test()> _
    Public Sub LinqToSqlString12()
      Dim empQuery = From emp In DB.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Insert(5, ":")

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Insert method to return a sequence of
    'employee phone numbers that have a ) in the fifth position,
    'removing all characters starting from the tenth character.
    <Test()> _
    Public Sub LinqToSqlString13()
      Dim empQuery = From emp In DB.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Remove(9)

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Insert method to return a sequence of
    'employee phone numbers that have a ) in the fifth position,
    'removing the first six characters.
    <Test()> _
    Public Sub LinqToSqlString14()
      Dim empQuery = From emp In DB.Employees _
            Where emp.HomePhone.Substring(4, 1) = ")" _
            Select emp.HomePhone.Remove(0, 6)

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the Replace method to return a sequence of
    'Supplier information where the Country field has had
    'UK replaced with United Kingdom and USA replaced with
    'United States of America.
    <Test()> _
    Public Sub LinqToSqlString15()
      Dim supplierQuery = From supplier In db.Suppliers _
            Select supplier.CompanyName, _
            Country = supplier.Country.Replace("UK", "United Kingdom") _
            .Replace("USA", "United States of America")

      TestExecutor.Execute(supplierQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the DateTime's Year property to
    'find Orders placed in 1997.
    <Test()> _
    <Ignore("RM-3702: Support DateTime properties")> _
    Public Sub LinqToSqlString16()
      Dim ordersIn97 = From ord In DB.Orders _
            Where ord.OrderDate.Value.Year = 1997

      TestExecutor.Execute(ordersIn97, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the DateTime's Month property to
    'find Orders placed in December.
    <Test()> _
    <Ignore("RM-3702: Support DateTime properties")> _
    Public Sub LinqToSqlString17()
      Dim decemberOrders = From ord In DB.Orders _
            Where ord.OrderDate.Value.Month = 12

      TestExecutor.Execute(decemberOrders, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses the DateTime's Day property to
    'find Orders placed on the 31st day of the month.
    <Test()> _
    <Ignore("RM-3702: Support DateTime properties")> _
    Public Sub LinqToSqlString18()
      Dim ordQuery = From ord In DB.Orders _
            Where ord.OrderDate.Value.Day = 31

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
