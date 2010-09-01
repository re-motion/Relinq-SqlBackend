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
  <TestFixture()>
  Public Class UnionAllIntersectTests
    Inherits TestBase

    'This sample uses Except to return a sequence of all countries that
    'Customers live in but no Employees live in.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - Clauses.ResultOperatorBase' is not supported by this registry and no custom result operator handler has been registered.")>
    Public Sub LinqToSqlUnion05()
      Dim countries = (From cust In DB.Customers _
            Select cust.Country).Except(From emp In DB.Employees _
                                          Select emp.Country)

      TestExecutor.Execute(countries, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace

