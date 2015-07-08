Imports System.Web.Mvc

Namespace Controllers
    Public Class GameClientController
        Inherits Controller

        ' GET: GameClient
        Function Index() As ActionResult
            Return View()
        End Function
    End Class
End Namespace