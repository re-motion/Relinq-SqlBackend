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
  Public Class GroupTopBottom
    Inherits Executor
    '<Category("TOP/BOTTOM")> _
    '  <Title("Take")> _
    '  <Description("This sample uses Take to select the first 5 Employees hired.")> _
    Public Sub LinqToSqlTop01()
      Dim first5Employees = From emp In db.Employees _
            Order By emp.HireDate _
            Take 5

      serializer.Serialize(first5Employees)
    End Sub

    'This sample uses Skip to select all but the 10 most expensive Products.")> _
    Public Sub LinqToSqlTop02()
      Dim expensiveProducts = From prod In db.Products _
            Order By prod.UnitPrice Descending _
            Skip 10

      serializer.Serialize(expensiveProducts)
    End Sub
  End Class
End Namespace

