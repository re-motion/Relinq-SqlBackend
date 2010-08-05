' Copyright (c) Microsoft Corporation.  All rights reserved.
Option Infer On
Option Strict On

Imports System.IO
Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
Imports Remotion.Data.Linq.IntegrationTests.Utilities

Namespace LinqSamples101
  Public Class Executor
    Protected Shared ReadOnly connString As String = "Data Source=localhost;Initial Catalog=Northwind; Integrated Security=SSPI;"

    Protected Shared db As Northwind
    Protected Shared serializer As TestResultSerializer

    Public Shared Sub Main()
      InitSample()



      'ExecuteAllSamples()

      CallAllMethods(GetType(GroupExternalMapping))
      Debug.Print("All Methods executed")
      'Console.Read()
    End Sub

    Private Shared Sub InitSample()
      ' Creates a new Northwind object to start fresh with an empty object cache
      ' Active ADO.NET connection will be reused by new Northwind object

      Dim oldLog As TextWriter
      If db Is Nothing Then
        oldLog = Nothing
      Else
        oldLog = db.Log
      End If

      db = New Northwind(connString) With { _
       .Log = oldLog _
      }
      serializer = New TestResultSerializer(New StreamWriter("C:\\vbTestOut.txt"), Function(memberInfo) Not memberInfo.IsDefined(GetType(System.Data.Linq.Mapping.AssociationAttribute), False))

    End Sub

    Private Shared Sub CallAllMethods(ByVal testClass As Type)
      Dim methods As MethodInfo() = testClass.GetMethods()
      For Each methodInfo As MethodInfo In methods
        Dim instance As Object = Activator.CreateInstance(testClass)

        If methodInfo.Name.Contains("LinqToSql") Then
          Debug.Print("Call: " + methodInfo.Name)
          methodInfo.Invoke(instance, Nothing)
        End If
      Next
      Debug.Print("All Methods for testclass " _
                        + testClass.Name _
                        + " executed " _
                        + System.Environment.NewLine)
    End Sub

    Private Shared Sub ExecuteAllSamples()
      Dim mscorlib As Assembly = Assembly.Load("Remotion.Data.Linq.IntegrationTest.VisualBasic")
      For Each type As Type In mscorlib.GetTypes()

        If type.BaseType <> Nothing Then
          If type.BaseType.Name.Equals("Executor") Then
            Debug.Print("Call Methods Class: " + type.Name)
            CallAllMethods(type)
          End If
        End If

      Next
    End Sub

  End Class
End Namespace
