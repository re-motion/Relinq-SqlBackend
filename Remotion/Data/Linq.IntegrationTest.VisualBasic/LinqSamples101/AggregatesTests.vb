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


Imports Remotion.Data.Linq.IntegrationTests
Imports System.Reflection


Namespace LinqSamples101
  Public Class AggregatesTests
    Inherits TestBase

    'This sample uses Count to find the number of Customers in the database.")> _
    <Test()>
    Public Sub LinqToSqlCount01()
      Dim custCount = db.Customers.Count()

      TestExecutor.Execute(custCount, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Count to find the number of Products in the database " & _
    'that are not discontinued.")> _
    <Test()>
    Public Sub LinqToSqlCount02()
      Dim activeProducts = Aggregate prod In db.Products _
            Into Count(Not prod.Discontinued)

      'Alternative Syntax
      'Dim activeProducts = (From prod In db.Products _
      '                      Where Not prod.Discontinued _
      '                      Select prod).Count()

      TestExecutor.Execute(activeProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Sum to find the total freight over all Orders.")> _
    <Test()>
    Public Sub LinqToSqlCount03()

      Dim totalFreight = Aggregate ord In db.Orders _
            Into Sum(ord.Freight)

      'Alternative Syntax
      'Dim totalFreight = (From ord In db.Orders _
      '                    Select ord.Freight).Sum()

      TestExecutor.Execute(totalFreight, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Sum to find the total number of units on order over all Products.")> _
        <Test()>
        <Ignore("Bug or missing feature in Relinq - System.NotImplementedException - Implement if needed by integration tests - works in c# but not in vb")>
        Public Sub LinqToSqlCount04()
            Dim totalUnits = (From prod In DB.Products _
                    Select CInt(prod.UnitsOnOrder.Value)).Sum()

            TestExecutor.Execute(totalUnits, MethodBase.GetCurrentMethod())
        End Sub

    'This sample uses Min to find the lowest unit price of any Product.")> _
    <Test()>
    Public Sub LinqToSqlCount05()
      Dim lowestPrice = Aggregate prod In db.Products _
            Into Min(prod.UnitPrice)

      TestExecutor.Execute(lowestPrice, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Min to find the lowest freight of any Order.")> _
    <Test()>
    Public Sub LinqToSqlCount06()
      Dim lowestFreight = Aggregate ord In db.Orders _
            Into Min(ord.Freight)

      TestExecutor.Execute(lowestFreight, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Min to find the Products that have the lowest unit price " & _
    'in each category.")> _
        <Test()>
        <Ignore("Bug or missing feature in Relinq - ArgumentException - Argument type IQueryable does not match the corresponding member type IEnumerable")>
        Public Sub LinqToSqlCount07()
            Dim categories = From prod In DB.Products _
                  Group prod By prod.CategoryID Into g = Group _
                  Select CategoryID, _
                  CheapestProducts = _
                  From p2 In g _
                  Where p2.UnitPrice = g.Min(Function(p3) p3.UnitPrice) _
                  Select p2

            TestExecutor.Execute(categories, MethodBase.GetCurrentMethod())
        End Sub


    'This sample uses Max to find the latest hire date of any Employee.")> _
    <Test()>
    Public Sub LinqToSqlCount08()
      Dim latestHire = Aggregate emp In db.Employees _
            Into Max(emp.HireDate)

      TestExecutor.Execute(latestHire, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Max to find the most units in stock of any Product.")> _
    <Test()>
    Public Sub LinqToSqlCount09()
      Dim mostInStock = Aggregate prod In db.Products _
            Into Max(prod.UnitsInStock)

      TestExecutor.Execute(mostInStock, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Max to find the Products that have the highest unit price " & _
    'in each category.")> _
        <Test()>
        <Ignore("Bug or missing feature in Relinq - ArgumentException - Argument type IQueryable does not match the corresponding member type IEnumerable")>
        Public Sub LinqToSqlCount10()
            Dim categories = From prod In DB.Products _
                  Group prod By prod.CategoryID Into g = Group _
                  Select CategoryGroup = g, _
                  MostExpensiveProducts = _
                  From p2 In g _
                  Where p2.UnitPrice = g.Max(Function(p3) p3.UnitPrice)

            TestExecutor.Execute(categories, MethodBase.GetCurrentMethod())
        End Sub


    'This sample uses Average to find the average freight of all Orders.")> _
    <Test()>
    Public Sub LinqToSqlCount11()
      Dim avgFreight = Aggregate ord In db.Orders _
            Into Average(ord.Freight)

      TestExecutor.Execute(avgFreight, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Average to find the average unit price of all Products.")> _
    <Test()>
    Public Sub LinqToSqlCount12()
      Dim avgPrice = Aggregate prod In db.Products _
            Into Average(prod.UnitPrice)

      TestExecutor.Execute(avgPrice, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Average to find the Products that have unit price higher than " & _
    'the average unit price of the category for each category.")> _
        <Test()>
        <Ignore("Bug or missing feature in Relinq - ArgumentException - Argument type IQueryable does not match the corresponding member type IEnumerable")>
        Public Sub LinqToSqlCount13()
            Dim categories = From prod In DB.Products _
                  Group prod By prod.CategoryID Into g = Group _
                  Select g, _
                  ExpensiveProducts = _
                  From prod2 In g _
                  Where (prod2.UnitPrice > g.Average(Function(p3) p3.UnitPrice))

            TestExecutor.Execute(categories, MethodBase.GetCurrentMethod())
        End Sub
    End Class
End Namespace


