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
    <Ignore("RM-3337: Support for additional string manipulation routine: Concat")>
    Public Sub LinqToSqlSelect03()
      Dim nameAndNumber = From emp In DB.Employees _
            Select Name = emp.FirstName & " " & emp.LastName, _
            Phone = emp.HomePhone

      TestExecutor.Execute(nameAndNumber, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and anonymous types to return
    'a sequence of all Products' IDs and a calculated value 
    'called HalfPrice which is set to the Product's UnitPrice 
    'divided by 2.
    <Test()>
    Public Sub LinqToSqlSelect04()
      Dim prices = From prod In DB.Products _
            Select prod.ProductID, HalfPrice = prod.UnitPrice / 2

      TestExecutor.Execute(prices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and a conditional statment to return a sequence of product
    'name and product availability.
    <Test()>
    <Ignore("RM-3269: Invalid in-memory projection generated when a binary (or other) expression contains a conversion")>
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
    <Ignore("RM-3306: Support for MemberInitExpressions")>
    Public Sub LinqToSqlSelect06()
      Dim names = From emp In DB.Employees _
            Select New Name With {.FirstName = emp.FirstName, _
            .LastName = emp.LastName}

      TestExecutor.Execute(names, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select and Where clauses to return a sequence of
    'just the London Customers' contact names.
    <Test()>
    <Ignore("RM-3197: Predicate LambdaExpressions are not correctly resolved if the lambda's parameter is used in a VB string comparison")>
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
    'saved if shipping is not included.
    'WORKAROUND: Northwind doesn't offer OrderDetails - changed to OrderDetails
    <Test()>
    <Ignore("RM-3207: When a NewExpression contains a subquery whose original type is IEnumerable<T>, an ArgumentException (wrapped into a " _
            & "TargetInvocationException) is thrown/RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlSelect09()
      Dim orders = From ord In DB.Orders _
            Select ord.OrderID, DiscountedProducts = (From od In ord.OrderDetails _
            Where od.Discount > 0.0), _
            FreeShippingDiscount = ord.Freight

      TestExecutor.Execute(orders, MethodBase.GetCurrentMethod())
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

    'Phone converter that converts a phone number to 
    'an international format based on its country.
    'This sample only supports USA and UK formats, for 
    'phone numbers from the Northwind database.
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
    'PhoneNumberConverter' to convert Phone number
    'to an international format.
    <Test()>
    <Ignore("RM-3307: Support for local method calls")>
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
    <Ignore("RM-3307: Support for local method calls")>
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
  End Class
End Namespace
