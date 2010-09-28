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
  Public Class ExistsInAnyAllContainsTests
    Inherits TestBase
    'This sample uses the Any operator to return only Customers that have no Orders.
    <Test()>
    Public Sub LinqToSqlExists01()
      Dim custQuery = From cust In DB.Customers _
            Where Not cust.Orders.Any()

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Any to return only Categories that have
    'at least one Discontinued product.
    <Test()>
    <Ignore("RM-3198: InvalidOperationException is thrown when a comparison or join condition involves a nullable and a non-nullable expression")>
    Public Sub LinqToSqlExists02()
      Dim prodQuery = From cust In DB.Categories _
            Where (From prod In cust.Products Where prod.Discontinued).Any()

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses All to return Customers whom all of their orders
    'have been shipped to their own city or whom have no orders.
    <Test()>
    <Ignore("RM-3197: Predicate LambdaExpressions are not correctly resolved if the lambda's parameter is used in a VB string comparison")>
    Public Sub LinqToSqlExists03()
      Dim ordQuery = From cust In DB.Customers _
              Where cust.Orders.All(Function(ord) ord.ShipCity = cust.City)

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Contain to find which Customer contains an order with OrderID 10248.
    <Test()>
    Public Sub LinqToSqlExists04()

      Dim order = (From o In DB.Orders _
            Where o.OrderID = 10248).First()

      Dim q = DB.Customers.Where(Function(p) p.Orders.Contains(order)).ToList()

      TestExecutor.Execute(New With {order, q}, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Contains to find customers whose city is Seattle, London, Paris or Vancouver.
    <Test()>
    <Ignore("RM-3336: Contains with a constant collection does not work if the expression on which Contains is called has type IEnumerable<T>")>
    Public Sub LinqToSqlExists05()
      Dim cities = New String() {"Seattle", "London", "Vancouver", "Paris"}

      Dim q = DB.Customers.Where(Function(p) cities.Contains(p.City)).ToList()

      TestExecutor.Execute(q, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
