Imports System.Net
Imports System.Web.Http

Namespace Controllers
    Public Class GameCommandController
        Inherits ApiController

        ' GET: api/Default/5
        Public Function RenderCommand(ByVal command As String) As String
            Return "value"
        End Function
    End Class
End Namespace