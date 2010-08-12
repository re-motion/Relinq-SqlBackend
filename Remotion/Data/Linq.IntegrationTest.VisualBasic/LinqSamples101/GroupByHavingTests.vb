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
  Public Class GroupByHavingTests
    Inherits TestBase

    'This sample uses Group By to partition Products by CategoryID.
    <Test()>
    <Ignore("Ignored in C# - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy01()
      Dim categorizedProducts = From prod In DB.Products _
            Group prod By prod.CategoryID Into prodGroup = Group _
            Select prodGroup

      TestExecutor.Execute(categorizedProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Max to find the maximum unit price for each CategoryID.")> _
    <Test()>
    <Ignore("Working in C# but not in VB - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy02()
      Dim maxPrices = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, MaxPrice = Max(prod.UnitPrice) _
            Select prodGroup, MaxPrice

      TestExecutor.Execute(maxPrices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Min to find the minimum unit price for each CategoryID.")> _
    <Test()>
    <Ignore("Working in C# but not in VB - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy03()
      Dim minPrices = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, MinPrice = Min(prod.UnitPrice)

      TestExecutor.Execute(minPrices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Average to find the average UnitPrice for each CategoryID.")> _
    <Test()>
    <Ignore("Working in C# but not in VB - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy04()
      Dim avgPrices = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, AveragePrice = Average(prod.UnitPrice)

      TestExecutor.Execute(avgPrices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Sum to find the total UnitPrice for each CategoryID.")> _
    <Test()>
    <Ignore("Working in C# but not in VB - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy05()
      Dim totalPrices = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, TotalPrice = Sum(prod.UnitPrice)

      TestExecutor.Execute(totalPrices, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Count to find the number of Products in each CategoryID.")> _
    <Test()>
    <Ignore("Working in C# but not in VB - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy06()
      Dim prodQuery = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group _
            Select prodGroup, NumProducts = prodGroup.Count()

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By and Count to find the number of Products in each CategoryID that are discontinued.")> _
    <Test()>
    <Ignore("Working in C# but not in VB - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy07()

      Dim prodQuery = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, NumProducts = Count(prod.Discontinued)

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a Where clause after a Group By clause to find all categories that have at least 10 products.")> _
    <Test()>
    <Ignore("Working in C# but not in VB - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy08()
      Dim bigCategories = From prod In DB.Products _
            Group prod By prod.CategoryID _
            Into ProdGroup = Group, ProdCount = Count() _
            Where ProdCount >= 10 _
            Select ProdGroup, ProdCount

      TestExecutor.Execute(bigCategories, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Group By to group products by CategoryID and SupplierID.")> _
    <Test()>
    <Ignore("Ignored in C# - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
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
    <Ignore("Ignored in C# - ArgumentException : Argument type IGrouping does not match the corresponding member type IEnumerable")>
    Public Sub LinqToSqlGroupBy10()
      Dim categories = From prod In DB.Products _
            Group prod By Key = New With {.Criterion = prod.UnitPrice > 10} _
            Into ProductGroup = Group

      TestExecutor.Execute(categories, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
