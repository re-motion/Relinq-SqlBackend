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


Imports Remotion.Data.Linq.IntegrationTests
Imports System.Reflection


Namespace LinqSamples101
  <TestFixture()>
  Public Class AggregatesTests
    Inherits TestBase

    'This sample uses Count to find the number of Customers in the database.
    <Test()>
    Public Sub LinqToSqlCount01()
      Dim custCount = db.Customers.Count()

      TestExecutor.Execute(custCount, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Count to find the number of Products in the database
    'that are not discontinued.
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

    'This sample uses Sum to find the total freight over all Orders.
    <Test()>
    Public Sub LinqToSqlCount03()

      Dim totalFreight = Aggregate ord In db.Orders _
            Into Sum(ord.Freight)

      'Alternative Syntax
      'Dim totalFreight = (From ord In db.Orders _
      '                    Select ord.Freight).Sum()

      TestExecutor.Execute(totalFreight, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Sum to find the total number of units on order over all Products.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - System.NotImplementedException - Implement if needed by integration tests - works in c# but not in vb")>
    Public Sub LinqToSqlCount04()
      Dim totalUnits = (From prod In DB.Products _
              Select CInt(prod.UnitsOnOrder.Value)).Sum()

      TestExecutor.Execute(totalUnits, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Min to find the lowest unit price of any Product.
    <Test()>
    Public Sub LinqToSqlCount05()
      Dim lowestPrice = Aggregate prod In db.Products _
            Into Min(prod.UnitPrice)

      TestExecutor.Execute(lowestPrice, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Min to find the lowest freight of any Order.
    <Test()>
    Public Sub LinqToSqlCount06()
      Dim lowestFreight = Aggregate ord In db.Orders _
            Into Min(ord.Freight)

      TestExecutor.Execute(lowestFreight, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Min to find the Products that have the lowest unit price
    'in each category.
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


    'This sample uses Max to find the latest hire date of any Employee.
    <Test()>
    Public Sub LinqToSqlCount08()
      Dim latestHire = Aggregate emp In db.Employees _
            Into Max(emp.HireDate)

      TestExecutor.Execute(latestHire, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Max to find the most units in stock of any Product.
    <Test()>
    Public Sub LinqToSqlCount09()
      Dim mostInStock = Aggregate prod In db.Products _
            Into Max(prod.UnitsInStock)

      TestExecutor.Execute(mostInStock, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Max to find the Products that have the highest unit price
    'in each category.
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


    'This sample uses Average to find the average freight of all Orders.
    <Test()>
    Public Sub LinqToSqlCount11()
      Dim avgFreight = Aggregate ord In db.Orders _
            Into Average(ord.Freight)

      TestExecutor.Execute(avgFreight, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Average to find the average unit price of all Products.
    <Test()>
    Public Sub LinqToSqlCount12()
      Dim avgPrice = Aggregate prod In db.Products _
            Into Average(prod.UnitPrice)

      TestExecutor.Execute(avgPrice, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Average to find the Products that have unit price higher than
    'the average unit price of the category for each category.
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


