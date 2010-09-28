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
  Public Class GroupByHavingTests
    Inherits TestBase

    'This sample uses Group By to partition Products by CategoryID.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy01()
      Dim categorizedProducts = From prod In DB.Products _
            Group prod By prod.CategoryID Into prodGroup = Group _
            Select prodGroup

      TestExecutor.Execute(categorizedProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Max to find the maximum unit price for each CategoryID.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy02()
      Dim maxPrices = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, MaxPrice = Max(prod.UnitPrice) _
            Select prodGroup, MaxPrice

      TestExecutor.Execute(maxPrices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Min to find the minimum unit price for each CategoryID.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy03()
      Dim minPrices = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, MinPrice = Min(prod.UnitPrice)

      TestExecutor.Execute(minPrices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Average to find the average UnitPrice for each CategoryID.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy04()
      Dim avgPrices = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, AveragePrice = Average(prod.UnitPrice)

      TestExecutor.Execute(avgPrices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Sum to find the total UnitPrice for each CategoryID.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy05()
      Dim totalPrices = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, TotalPrice = Sum(prod.UnitPrice)

      TestExecutor.Execute(totalPrices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Count to find the number of Products in each CategoryID.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy06()
      Dim prodQuery = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group _
            Select prodGroup, NumProducts = prodGroup.Count()

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Count to find the number of Products in each CategoryID that are discontinued.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy07()

      Dim prodQuery = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, NumProducts = Count(prod.Discontinued)

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a Where clause after a Group By clause to find all categories that have at least 10 products.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy08()
      Dim bigCategories = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into ProdGroup = Group, ProdCount = Count() _
            Where ProdCount >= 10 _
            Select ProdGroup, ProdCount

      TestExecutor.Execute(bigCategories, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By to group products by CategoryID and SupplierID.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy09()
      Dim categories = From prod In DB.Products _
            Group By Key = New With {prod.CategoryID, prod.SupplierID} _
            Into prodGroup = Group _
            Select Key, prodGroup

      TestExecutor.Execute(categories, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By to return two sequences of products. The first sequence contains products with unit price
    'greater than 10. The second sequence contains products with unit price less than or equal to 10.
    <Test()>
    <Ignore("RM-3265: Support collections to be selected at the top level of a query")>
    Public Sub LinqToSqlGroupBy10()
      Dim categories = From prod In DB.Products _
            Group prod By Key = New With {.Criterion = prod.UnitPrice > 10} _
            Into ProductGroup = Group

      TestExecutor.Execute(categories, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
