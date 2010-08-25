Imports Remotion.Data.Linq.IntegrationTests
Imports System.Reflection
Imports NUnit.Framework

' TODO Review: Move this attribute down to each of the concrete test classes
<TestFixture()>
Public Class TestBase
  Inherits AbstractTestBase

  ' VB will not add the folder structure to the resource file name when embedding a resource
  ' The desired resource name is: Remotion.Data.Linq.IntegrationTests.VisualBasic.LinqSamples101.Resources.TestClass.TestMethod.result
  ' This is achieved by putting a file called "LinqSamples101.Resources.TestClass.TestMethod.result" into the LinqSamples101\Resources folder
  Protected Overrides ReadOnly Property SavedResourceFileNameGenerator As System.Func(Of System.Reflection.MethodBase, String)
    Get
      Return Function(method As MethodBase)
               Return "LinqSamples101.Resources." & method.DeclaringType.Name & "." & method.Name + ".result"
             End Function
    End Get
  End Property

  ' When loading the resource, we must specify the full name as described above
  Protected Overrides ReadOnly Property LoadedResourceNameGenerator As System.Func(Of System.Reflection.MethodBase, String)
    Get
      Return Function(method As MethodBase)
               Return method.DeclaringType.Namespace & ".Resources." & method.DeclaringType.Name & "." & method.Name + ".result"
             End Function
    End Get
  End Property
End Class