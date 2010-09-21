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
  Public Class WhereTests
    Inherits TestBase
    'VB 101LinqSamples starts with different samples.
    'Samples were renamed according to the C# samples

    'This sample uses a Where clause to filter for Customers in London.
    <Test()>
    <Ignore("RM-3197: Predicate LambdaExpressions are not correctly resolved if the lambda's parameter is used in a VB string comparison")>
    Public Sub LinqToSqlWhere01()

      'Only return customers from London
      Dim londonCustomers = From cust In DB.Customers _
            Where cust.City = "London" _
            Select cust.CompanyName, cust.City, cust.Country


      TestExecutor.Execute(londonCustomers, MethodBase.GetCurrentMethod())
    End Sub


    'This sample uses a Where clause to filter for Employees hired
    'during or after 1994.
    <Test()>
    <Ignore("RM-3335: Support nullable booleans")>
    Public Sub LinqToSqlWhere02()
      Dim hiredAfter1994 = From emp In DB.Employees _
            Where emp.HireDate >= #1/1/1994# _
            Select emp

      TestExecutor.Execute(hiredAfter1994, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a Where clause to filter for Products that have stock below their
    'reorder level and are not discontinued.
    <Test()>
    <Ignore("RM-3335: Support nullable booleans")>
    Public Sub LinqToSqlWhere03()
      Dim needToOrder = From prod In DB.Products _
            Where prod.UnitsInStock <= prod.ReorderLevel _
                  AndAlso Not prod.Discontinued _
            Select prod

      TestExecutor.Execute(needToOrder, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a Where clause to filter out Products that are either
    'discontinued or that have a UnitPrice greater than 10.
    <Test()>
    <Ignore("RM-3335: Support nullable booleans")>
    Public Sub LinqToSqlWhere04()
      Dim prodQuery = From prod In DB.Products _
            Where prod.UnitPrice > 10.0# OrElse prod.Discontinued

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses two Where clauses to filter out Products that are discontinued 
    'and with UnitPrice greater than 10
    <Test()>
    <Ignore("RM-3335: Support nullable booleans")>
    Public Sub LinqToSqlWhere05()

      Dim prodQuery = From prod In DB.Products _
            Where prod.UnitPrice > 10D _
            Where prod.Discontinued

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub


    'This sample uses First to select the first Shipper in the table.
    <Test()>
    Public Sub LinqToSqlWhere06()
      Dim shipper = db.Shippers.First()

      TestExecutor.Execute(shipper, MethodBase.GetCurrentMethod())
    End Sub


    'This sample uses Take to select the first Customer with CustomerID 'BONAP'.
    <Test()>
    <Ignore("RM-3197: Predicate LambdaExpressions are not correctly resolved if the lambda's parameter is used in a VB string comparison")>
    Public Sub LinqToSqlWhere07()
      Dim customer = From cust In DB.Customers _
            Where cust.CustomerID = "BONAP" _
            Take 1

      TestExecutor.Execute(customer, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses First to select an Order with freight greater than 10.00.
    <Test()>
    <Ignore("RM-3335: Support nullable booleans")>
    Public Sub LinqToSqlWhere08()
      Dim firstOrd = (From ord In DB.Orders _
            Where ord.Freight > 10D _
            Select ord).First()

      TestExecutor.Execute(firstOrd, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
