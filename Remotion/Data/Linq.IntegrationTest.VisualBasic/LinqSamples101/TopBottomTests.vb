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
  Public Class TopBottomTests
    Inherits TestBase

    'This sample uses Take to select the first 5 Employees hired.
    <Ignore("Bug or missing feature in Relinq - relinq doesn't support byte types yet")>
    <Test()>
    Public Sub LinqToSqlTop01()
      Dim first5Employees = From emp In DB.Employees _
            Order By emp.HireDate _
            Take 5

      TestExecutor.Execute(first5Employees, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Skip to select all but the 10 most expensive Products.
    <Test()>
    Public Sub LinqToSqlTop02()
      Dim expensiveProducts = From prod In DB.Products _
            Order By prod.UnitPrice Descending _
            Skip 10

      TestExecutor.Execute(expensiveProducts, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace

