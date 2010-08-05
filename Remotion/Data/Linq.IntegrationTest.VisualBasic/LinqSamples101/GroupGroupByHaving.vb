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
  Public Class GroupGroupByHaving
    Inherits Executor
    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Simple")> _
    '<Description("This sample uses Group By to partition Products by " & _
    '             "CategoryID.")> _
    Public Sub LinqToSqlGroupBy01()
      Dim categorizedProducts = From prod In db.Products _
            Group prod By prod.CategoryID Into prodGroup = Group _
            Select prodGroup

      serializer.Serialize(categorizedProducts)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Max")> _
    '<Description("This sample uses Group By and Max " & _
    '             "to find the maximum unit price for each CategoryID.")> _
    Public Sub LinqToSqlGroupBy02()
      Dim maxPrices = From prod In db.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, MaxPrice = Max(prod.UnitPrice) _
            Select prodGroup, MaxPrice

      serializer.Serialize(maxPrices)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Min")> _
    '<Description("This sample uses Group By and Min " & _
    '             "to find the minimum unit price for each CategoryID.")> _
    Public Sub LinqToSqlGroupBy03()
      Dim minPrices = From prod In db.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, MinPrice = Min(prod.UnitPrice)

      serializer.Serialize(minPrices)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Average")> _
    '<Description("This sample uses Group By and Average " & _
    '             "to find the average UnitPrice for each CategoryID.")> _
    Public Sub LinqToSqlGroupBy04()
      Dim avgPrices = From prod In db.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, AveragePrice = Average(prod.UnitPrice)

      serializer.Serialize(avgPrices)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Sum")> _
    '<Description("This sample uses Group By and Sum " & _
    '             "to find the total UnitPrice for each CategoryID.")> _
    Public Sub LinqToSqlGroupBy05()
      Dim totalPrices = From prod In db.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, TotalPrice = Sum(prod.UnitPrice)

      serializer.Serialize(totalPrices)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Count")> _
    '<Description("This sample uses Group By and Count " & _
    '             "to find the number of Products in each CategoryID.")> _
    Public Sub LinqToSqlGroupBy06()
      Dim prodQuery = From prod In db.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group _
            Select prodGroup, NumProducts = prodGroup.Count()

      serializer.Serialize(prodQuery)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Count - Conditional")> _
    '<Description("This sample uses Group By and Count " & _
    '             "to find the number of Products in each CategoryID " & _
    '             "that are discontinued.")> _
    Public Sub LinqToSqlGroupBy07()

      Dim prodQuery = From prod In db.Products _
            Group prod By prod.CategoryID _
            Into prodGroup = Group, NumProducts = Count(prod.Discontinued)

      'Alternative Syntax
      'Dim prodQuery = From prod In db.Products _
      '                Group prod By prod.CategoryID Into prodGroup = Group _
      '                Select prodGroup, _
      '                       NumProducts = prodGroup.Count(Function(prod2) prod2.Discontinued)

      serializer.Serialize(prodQuery)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - followed by Where")> _
    '<Description("This sample uses a Where clause after a Group By clause " & _
    '             "to find all categories that have at least 10 products.")> _
    Public Sub LinqToSqlGroupBy08()
      Dim bigCategories = From prod In db.Products _
            Group prod By prod.CategoryID _
            Into ProdGroup = Group, ProdCount = Count() _
            Where ProdCount >= 10 _
            Select ProdGroup, ProdCount

      serializer.Serialize(bigCategories)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Multiple Columns")> _
    '<Description("This sample uses Group By to group products by CategoryID and SupplierID.")> _
    Public Sub LinqToSqlGroupBy09()
      Dim categories = From prod In db.Products _
            Group By Key = New With {prod.CategoryID, prod.SupplierID} _
            Into prodGroup = Group _
            Select Key, prodGroup

      serializer.Serialize(categories)
    End Sub

    '<Category("GROUP BY/HAVING")> _
    '<Title("GroupBy - Expression")> _
    '<Description("This sample uses Group By to return two sequences of products. " & _
    '             "The first sequence contains products with unit price " & _
    '             "greater than 10. The second sequence contains products " & _
    '             "with unit price less than or equal to 10.")> _
    Public Sub LinqToSqlGroupBy10()
      Dim categories = From prod In db.Products _
            Group prod By Key = New With {.Criterion = prod.UnitPrice > 10} _
            Into ProductGroup = Group

      serializer.Serialize(categories)
    End Sub
  End Class
End Namespace
