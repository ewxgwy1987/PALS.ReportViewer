'=====================================================================================
' Copyright 2009, Xu Jian, All Rights Reserved.
'=====================================================================================
'FileName       Login.vb
'Revision:      1.0 -   09 Oct 2009, By Xu Jian 
'=====================================================================================

Option Explicit On
Option Strict On

Public Class Login

    Private _UID As String
    Private _PWD As String
    Private _Domain As String
    Private _isCanceled As Boolean

    Public Function CallMe(ByRef UID As String, ByRef PWD As String, ByRef Domain As String) As Boolean
        _UID = UID
        _PWD = PWD
        _Domain = Domain

        Me.ShowDialog()

        UID = _UID
        PWD = _PWD
        Domain = _Domain

        Return _isCanceled
    End Function

    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
        Dim UID As String = Trim(txtUID.Text)

        If UID = String.Empty Then
            MsgBox("Please enter valid user name!", MsgBoxStyle.Information, "Information")
        Else
            _UID = UID
            _PWD = txtPWD.Text
            _Domain = txtDomain.Text

            _isCanceled = False
            Me.Close()
        End If

    End Sub

    Private Sub Login_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        _UID = String.Empty
        _PWD = String.Empty
        txtDomain.Text = _Domain
        _isCanceled = True
    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        _isCanceled = True
        Me.Close()
    End Sub

    Private Sub txtDomain_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDomain.GotFocus
        txtDomain.SelectAll()
    End Sub

    Private Sub txtPWD_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtPWD.GotFocus
        txtPWD.SelectAll()
    End Sub

    Private Sub txtUID_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtUID.GotFocus
        txtUID.SelectAll()
    End Sub
End Class