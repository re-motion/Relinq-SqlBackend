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
Imports Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind

Namespace LinqSamples101
  <TestFixture()> _
  Public Class ConversionOperatorsTests
    Inherits TestBase

    'This sample uses ToArray so that the client-side IEnumerable(Of T) implementation of Where is used, instead of the default Query(Of T) 
    'implementation which would be converted to SQL and executed
    'on the server.  This is necessary because the where clause
    'references a user-defined client-side method, isValidProduct,
    'which cannot be converted to SQL.
    <Test()> _
    Public Sub LinqToSqlConversions01()
      Dim prodQuery = From prod In DB.Products.AsEnumerable() _
            Where isValidProduct(prod)

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    Private Function isValidProduct(ByVal prod As Product) As Boolean
      Return (prod.ProductName.LastIndexOf("C") = 0)
    End Function

    'This sample uses ToArray to immediately evaluate a query into an array
    'and get the 3rd element.
    <Test()> _
    Public Sub LinqToSqlConversions02()
      Dim londonCustomers = From cust In DB.Customers _
            Where cust.City = "London"

      Dim custArray = londonCustomers.ToArray()
      TestExecutor.Execute(custArray(3), MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses ToList to immediately evaluate a query into a List(Of T).
    <Test()> _
    Public Sub LinqToSqlConversions03()
      Dim hiredAfter1994 = From emp In DB.Employees _
            Where emp.HireDate >= #1/1/1994#

      Dim qList = hiredAfter1994.ToList()
      TestExecutor.Execute(qList, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses ToDictionary to immediately evaluate a query and
    'a key expression into an Dictionary(Of K, T).
    <Test()> _
    Public Sub LinqToSqlConversion04()
      Dim prodQuery = From prod In DB.Products _
            Where prod.UnitsInStock <= prod.ReorderLevel _
                  AndAlso Not prod.Discontinued

      Dim qDictionary = prodQuery.ToDictionary(Function(prod) prod.ProductID)

      TestExecutor.Execute(qDictionary, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace

