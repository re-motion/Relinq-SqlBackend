' Copyright (c) Microsoft Corporation.  All rights reserved.
Option Infer On
Option Strict On

Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests.Utilities

Namespace LinqSamples101
  Public Class GroupAggregates
    Inherits Executor
    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Count - Simple")> _
    '<Description("This sample uses Count to find the number of Customers in the database.")> _
    Public Sub LinqToSqlCount01()
      Dim custCount = db.Customers.Count()
      serializer.Serialize(custCount)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Count - Conditional")> _
    '<Description("This sample uses Count to find the number of Products in the database " & _
    '             "that are not discontinued.")> _
    Public Sub LinqToSqlCount02()
      Dim activeProducts = Aggregate prod In db.Products _
                           Into Count(Not prod.Discontinued)

      'Alternative Syntax
      'Dim activeProducts = (From prod In db.Products _
      '                      Where Not prod.Discontinued _
      '                      Select prod).Count()

      serializer.Serialize(activeProducts)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Sum - Simple")> _
    '<Description("This sample uses Sum to find the total freight over all Orders.")> _
    Public Sub LinqToSqlCount03()

      Dim totalFreight = Aggregate ord In db.Orders _
                         Into Sum(ord.Freight)

      'Alternative Syntax
      'Dim totalFreight = (From ord In db.Orders _
      '                    Select ord.Freight).Sum()

      serializer.Serialize(totalFreight)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Sum - Mapped")> _
    '<Description("This sample uses Sum to find the total number of units on order over all Products.")> _
    Public Sub LinqToSqlCount04()
      Dim totalUnits = (From prod In db.Products _
                        Select CInt(prod.UnitsOnOrder.Value)).Sum()

      serializer.Serialize(totalUnits)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Min - Simple")> _
    '<Description("This sample uses Min to find the lowest unit price of any Product.")> _
    Public Sub LinqToSqlCount05()
      Dim lowestPrice = Aggregate prod In db.Products _
                        Into Min(prod.UnitPrice)

      serializer.Serialize(lowestPrice)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Min - Mapped")> _
    '<Description("This sample uses Min to find the lowest freight of any Order.")> _
    Public Sub LinqToSqlCount06()
      Dim lowestFreight = Aggregate ord In db.Orders _
                          Into Min(ord.Freight)

      serializer.Serialize(lowestFreight)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Min - Elements")> _
    '<Description("This sample uses Min to find the Products that have the lowest unit price " & _
    '             "in each category.")> _
    Public Sub LinqToSqlCount07()
      Dim categories = From prod In db.Products _
                       Group prod By prod.CategoryID Into g = Group _
                       Select CategoryID = g, _
                              CheapestProducts = _
                                  From p2 In g _
                                  Where p2.UnitPrice = g.Min(Function(p3) p3.UnitPrice) _
                                  Select p2

      serializer.Serialize(categories)
    End Sub


    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Max - Simple")> _
    '<Description("This sample uses Max to find the latest hire date of any Employee.")> _
    Public Sub LinqToSqlCount08()
      Dim latestHire = Aggregate emp In db.Employees _
                       Into Max(emp.HireDate)

      serializer.Serialize(latestHire)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Max - Mapped")> _
    '<Description("This sample uses Max to find the most units in stock of any Product.")> _
    Public Sub LinqToSqlCount09()
      Dim mostInStock = Aggregate prod In db.Products _
                        Into Max(prod.UnitsInStock)

      serializer.Serialize(mostInStock)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Max - Elements")> _
    '<Description("This sample uses Max to find the Products that have the highest unit price " & _
    '             "in each category.")> _
    Public Sub LinqToSqlCount10()
      Dim categories = From prod In db.Products _
                       Group prod By prod.CategoryID Into g = Group _
                       Select CategoryGroup = g, _
                              MostExpensiveProducts = _
                                  From p2 In g _
                                  Where p2.UnitPrice = g.Max(Function(p3) p3.UnitPrice)

      serializer.Serialize(categories)
    End Sub


    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Average - Simple")> _
    '<Description("This sample uses Average to find the average freight of all Orders.")> _
    Public Sub LinqToSqlCount11()
      Dim avgFreight = Aggregate ord In db.Orders _
                       Into Average(ord.Freight)

      serializer.Serialize(avgFreight)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Average - Mapped")> _
    '<Description("This sample uses Average to find the average unit price of all Products.")> _
    Public Sub LinqToSqlCount12()
      Dim avgPrice = Aggregate prod In db.Products _
                     Into Average(prod.UnitPrice)

      serializer.Serialize(avgPrice)
    End Sub

    '<Category("COUNT/SUM/MIN/MAX/AVG")> _
    '<Title("Average - Elements")> _
    '<Description("This sample uses Average to find the Products that have unit price higher than " & _
    '             "the average unit price of the category for each category.")> _
    Public Sub LinqToSqlCount13()
      Dim categories = From prod In db.Products _
                       Group prod By prod.CategoryID Into g = Group _
                       Select g, _
                              ExpensiveProducts = _
                                  From prod2 In g _
                                  Where (prod2.UnitPrice > g.Average(Function(p3) p3.UnitPrice))

      serializer.Serialize(categories)
    End Sub
  End Class
End Namespace

