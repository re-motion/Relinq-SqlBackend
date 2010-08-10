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
  Public Class UserDefinedFunctionsTests
    Inherits TestBase

    'This sample demonstrates using a scalar user-defined function in a projection.
    <Test()>
    Public Sub LinqToSqlUserDefined01()
      Dim catQuery = From category In db.Categories _
            Select category.CategoryID, _
            TotalUnitPrice = db.TotalProductUnitPriceByCategory(category.CategoryID)

      TestExecutor.Execute(catQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample demonstrates using a scalar user-defined function in a Where clause.
        <Test(), Ignore()>
        Public Sub LinqToSqlUserDefined02()

            Dim prodQuery = From prod In DB.Products _
                  Where prod.UnitPrice = DB.MinUnitPriceByCategory(prod.CategoryID)

            TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
        End Sub

    'This sample demonstrates selecting from a table-valued user-defined function.
    <Test()>
    Public Sub LinqToSqlUserDefined03()

      Dim prodQuery = From p In db.ProductsUnderThisUnitPrice(10.25D) _
            Where Not p.Discontinued

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample demonstrates joining to the results of a table-valued user-defined function.
    <Test()>
    Public Sub LinqToSqlUserDefined04()

      Dim q = From category In db.Categories _
            Group Join prod In db.ProductsUnderThisUnitPrice(8.5D) _
            On category.CategoryID Equals prod.CategoryID _
            Into prods = Group _
            From prod2 In prods _
            Select category.CategoryID, category.CategoryName, _
            prod2.ProductName, prod2.UnitPrice

      TestExecutor.Execute(q, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace

