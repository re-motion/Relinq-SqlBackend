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
  Public Class UserDefinedFunctionsTests
    Inherits TestBase

    'This sample demonstrates using a scalar user-defined function in a projection.
    <Test()>
    Public Sub LinqToSqlUserDefined01()
      Dim catQuery = From category In DB.Categories _
            Select category.CategoryID, _
            TotalUnitPrice = DB.Functions.TotalProductUnitPriceByCategory(category.CategoryID)

      TestExecutor.Execute(catQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample demonstrates using a scalar user-defined function in a Where clause.
    <Test()>
    <Ignore("RM-3335: Support nullable booleans")>
    Public Sub LinqToSqlUserDefined02()

      Dim prodQuery = From prod In DB.Products _
            Where prod.UnitPrice = DB.Functions.MinUnitPriceByCategory(prod.CategoryID)

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample demonstrates selecting from a table-valued user-defined function.
    <Test()>
    <Explicit("Not tested: User-defined function in first from clause. This cannot be tested because it will always cause Linq-to-Sql to execute the query.")>
    Public Sub LinqToSqlUserDefined03()

      Dim prodQuery = From p In DB.Functions.ProductsUnderThisUnitPrice(10.25D) _
            Where Not p.Discontinued

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample demonstrates joining to the results of a table-valued user-defined function.
    <Test()>
    <Ignore("RM-3313: Add a TableInfo type allowing user-defined functions to be used as tables")>
    Public Sub LinqToSqlUserDefined04()

      Dim q = From category In DB.Categories _
            Group Join prod In DB.Functions.ProductsUnderThisUnitPrice(8.5D) _
            On category.CategoryID Equals prod.CategoryID _
            Into prods = Group _
            From prod2 In prods _
            Select category.CategoryID, category.CategoryName, _
            prod2.ProductName, prod2.UnitPrice

      TestExecutor.Execute(q, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace

