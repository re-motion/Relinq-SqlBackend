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
Imports Remotion.Linq.IntegrationTests.Common


Namespace LinqSamples101
  <TestFixture()> _
  Public Class OrderByTests
    Inherits TestBase

    'This sample uses Order By to sort Employees by hire date.
    <Test()> _
    Public Sub LinqToSqlOrderBy01()
      Dim empQuery = From emp In DB.Employees _
            Order By emp.HireDate

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Where and Order By to sort Orders shipped to London by freight.
    <Test()> _
    Public Sub LinqToSqlOrderBy02()
      Dim londonOrders = From ord In DB.Orders _
            Where ord.ShipCity = "London" _
            Order By ord.Freight

      TestExecutor.Execute(londonOrders, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Order By to sort Products
    'by unit price from highest to lowest.
    <Test()> _
    Public Sub LinqToSqlOrderBy03()
      Dim sortedProducts = From prod In DB.Products _
            Order By prod.UnitPrice Descending

      TestExecutor.Execute(sortedProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a compound Order By to sort Customers
    'by city and then contact name.
    <Test()> _
    Public Sub LinqToSqlOrderBy04()
      Dim custQuery = From cust In DB.Customers _
            Select cust _
            Order By cust.City, cust.ContactName

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Order By to sort Orders from EmployeeID 1
    'by ship-to country, and then by freight from highest to lowest.
    <Test()> _
    Public Sub LinqToSqlOrderBy05()
      Dim ordQuery = From ord In DB.Orders _
            Where ord.EmployeeID = 1 _
            Order By ord.ShipCountry, ord.Freight Descending

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Order By, Max and Group By to find the Products that have
    'the highest unit price in each category, and sorts the group by category id.
    <Test()> _
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")> _
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
