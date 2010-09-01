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
  Public Class NullTests
    Inherits TestBase

    'This sample uses the Nothing value to find Employees
    'that do not report to another Employee.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - byte type not supported")>
    Public Sub LinqToSqlNull01()
      Dim empQuery = From emp In DB.Employees _
            Where emp.ReportsTo Is Nothing

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Nullable(Of T).HasValue to find Employees " & _
    'that do not report to another Employee.")> _
    <Test()>
    <Ignore("Bug or missing feature in Relinq - nullable not supported")>
    Public Sub LinqToSqlNull02()
      Dim empQuery = From emp In DB.Employees _
            Where Not emp.ReportsTo.HasValue _
            Select emp

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Nullable(Of T).Value for Employees 
    'that report to another Employee to return the
    'EmployeeID number of that employee.  Note that
    'the .Value is optional.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - nullable not supported")>
    Public Sub LinqToSqlNull03()
      Dim empQuery = From emp In DB.Employees _
            Where emp.ReportsTo.HasValue _
            Select emp.FirstName, emp.LastName, ReportsTo = emp.ReportsTo.Value

      TestExecutor.Execute(empQuery, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
