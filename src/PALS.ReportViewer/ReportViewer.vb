Imports Microsoft.Reporting.WinForms

Public Class ReportViewer

#Region "1. Class Fields Declaration."
    Private m_ReportServerURL As String
    Private m_ReportPath As String
    Private m_NeedDateTimeFormat As Boolean
    Private m_ReportParams() As Reporting.ReportParameter
    Private m_ShowPrintLayout As Boolean = False
    Private m_AutoCloseViewer As Boolean = True
    Private m_DateFormat As String
    Private m_TimeFormat As String
    Private m_UserName As String
    Private m_Password As String
    Private m_Domain As String

    '<!-- previewZoom attribute mode: 0 - FullPage, 1 - PageWidth, 2 - Percent;
    'attribute percent: integer value of zoom percentage. Percentage will only be used when mode attribute is Percent -->
    '<previewZoom mode="1" percent="75" />
    Private m_PreviewZoomMode As Integer
    Private m_PreviewZoomPercent As Integer

#End Region

#Region "3. Class Properties Defination."

    Public Property AutoCloseViewer() As Boolean
        Get
            Return m_AutoCloseViewer
        End Get
        Set(ByVal value As Boolean)
            m_AutoCloseViewer = value
        End Set
    End Property

    Public Property NeedDateTimeFormat() As Boolean
        Get
            Return m_NeedDateTimeFormat
        End Get
        Set(ByVal Value As Boolean)
            m_NeedDateTimeFormat = Value
        End Set
    End Property

    Public Property TimeFormat() As String
        Get
            Return m_TimeFormat
        End Get
        Set(ByVal value As String)
            m_TimeFormat = value
        End Set
    End Property

    Public Property DateFormat() As String
        Get
            Return m_DateFormat
        End Get
        Set(ByVal value As String)
            m_DateFormat = value
        End Set
    End Property

    Public Property ShowPrintLayout() As Boolean
        Get
            Return m_ShowPrintLayout
        End Get
        Set(ByVal value As Boolean)
            m_ShowPrintLayout = value
        End Set
    End Property

    Public Property ReportServerURL() As String
        Get
            Return m_ReportServerURL
        End Get
        Set(ByVal value As String)
            m_ReportServerURL = value
        End Set
    End Property

    Public Property ReportPath() As String
        Get
            Return m_ReportPath
        End Get
        Set(ByVal value As String)
            m_ReportPath = value
        End Set
    End Property

    Public Property RepartParams() As Reporting.ReportParameter()
        Get
            Return m_ReportParams
        End Get
        Set(ByVal value As Reporting.ReportParameter())
            m_ReportParams = value
        End Set
    End Property

    Public Property UserName() As String
        Get
            Return m_UserName
        End Get
        Set(ByVal Value As String)
            m_UserName = Value
        End Set
    End Property

    Public Property Password() As String
        Get
            Return m_Password
        End Get
        Set(ByVal Value As String)
            m_Password = Value
        End Set
    End Property

    Public Property Domain() As String
        Get
            Return m_Domain
        End Get
        Set(ByVal Value As String)
            m_Domain = Value
        End Set
    End Property

    Public Property PreviewZoomMode() As Integer
        Get
            Return m_PreviewZoomMode
        End Get
        Set(ByVal Value As Integer)
            m_PreviewZoomMode = Value
        End Set
    End Property

    Public Property PreviewZoomPercent() As Integer
        Get
            Return m_PreviewZoomPercent
        End Get
        Set(ByVal Value As Integer)
            m_PreviewZoomPercent = Value
        End Set
    End Property

#End Region

    Private Function ParamDataValidate(ByVal ReportName As String, ByRef Param As Reporting.ReportParameter) As String
        Dim Value As String = Nothing

        Try
            Select Case UCase(Param.DataType)
                Case "STRING"
                    Value = Param.Value
                Case "INTEGER"
                    Value = CType(Param.Value, Integer).ToString
                Case "BOOLEAN"
                    Value = CType(Param.Value, Boolean).ToString
                Case "DOUBLE"
                    Value = CType(Param.Value, Double).ToString
                Case "DATETIME"
                    Dim DT As Date = Now.AddHours(CType(Param.Value, Double))

                    If Param.DateOnly Then
                        Value = Format(DT, m_DateFormat)
                    Else
                        Value = Format(Now.AddHours(CType(Param.Value, Double)), m_DateFormat & " " & m_TimeFormat)
                    End If
                Case Else
                    Value = Param.Value
            End Select

            Return Value
        Catch ex As Exception
            MsgBox("Wrong report parameter value was set in the XML configuration file!" & vbCrLf & vbCrLf & _
                    "Report: " & ReportName & vbCrLf & _
                    "Parameter: Name(" & Param.Name & "), DataType(" & Param.DataType & "), Value(" & _
                    Param.Value & ")", MsgBoxStyle.Critical, "Report Error")
            Return Nothing
        End Try
    End Function

    Public Sub ShowReport()
        Try
            ' Display a wait cursor while the TreeNodes are being created.
            Cursor.Current = Cursors.WaitCursor

            If m_ReportServerURL = "" Then
                Exit Sub
            End If

            If m_ReportPath = "" Then
                Exit Sub
            End If

            'Set the processing mode for the ReportViewer to Remote
            rptViewer.ProcessingMode = ProcessingMode.Remote

            'If user name is not given by XML configuration file, the use default credential for 
            'logging onto reporting server, otherwise, use given username, password, and domain name for 
            'logging.
            If m_UserName = Nothing Then
                rptViewer.ServerReport.ReportServerCredentials.NetworkCredentials = System.Net.CredentialCache.DefaultCredentials
            Else
                Dim myCred As New System.Net.NetworkCredential(m_UserName, m_Password, m_Domain)

                rptViewer.ServerReport.ReportServerCredentials.NetworkCredentials = myCred
            End If

            'MsgBox("UID=" & m_UserName & ", PWD=" & m_Password & ", Domain=" & m_Domain)

            rptViewer.ServerReport.ReportServerUrl = New Uri(m_ReportServerURL)
            rptViewer.ServerReport.ReportPath = m_ReportPath
            rptViewer.RefreshReport()

            If Not m_ReportParams Is Nothing Then
                Dim Len As Integer = m_ReportParams.Length

                If Len > 0 Then
                    Dim Parameters(Len - 1) As Microsoft.Reporting.WinForms.ReportParameter

                    Dim i As Integer
                    For i = 0 To Len - 1
                        'Create the sales order number report parameter
                        Dim Temp As New Microsoft.Reporting.WinForms.ReportParameter
                        Temp.Name = m_ReportParams(i).Name
                        Temp.Values.Add(ParamDataValidate(Me.Text, m_ReportParams(i)))
                        Parameters(i) = Temp
                    Next

                    If m_NeedDateTimeFormat Then
                        ReDim Preserve Parameters(Len - 1 + 2)

                        'If report template has "DFormat" and "TFormat" parameters, then pass the format string to it too.
                        Dim DF As New Microsoft.Reporting.WinForms.ReportParameter
                        DF.Name = "DFormat"
                        DF.Values.Add(m_DateFormat)
                        Parameters(i) = DF


                        Dim TF As New Microsoft.Reporting.WinForms.ReportParameter
                        TF.Name = "TFormat"
                        TF.Values.Add(m_TimeFormat)
                        Parameters(i + 1) = TF
                    End If

                    rptViewer.ServerReport.SetParameters(Parameters)
                End If
            End If

            If ShowPrintLayout Then
                rptViewer.SetDisplayMode(DisplayMode.PrintLayout)
            Else
                rptViewer.SetDisplayMode(DisplayMode.Normal)
                rptViewer.RefreshReport()
            End If

            Dim Mode As ZoomMode
            Select Case m_PreviewZoomMode
                Case 0
                    Mode = ZoomMode.FullPage
                Case 1
                    Mode = ZoomMode.PageWidth
                Case 2
                    Mode = ZoomMode.Percent
                Case Else
                    Mode = ZoomMode.PageWidth
            End Select
            rptViewer.ZoomMode = Mode
            rptViewer.ZoomPercent = m_PreviewZoomPercent

            rptViewer.RefreshReport()

        Catch ex As Exception
            'Close Preview window first before display the Message box, otherwise,
            'the message box may be hidden behind the top most preview window.
            Me.Close()
            MsgBox(ex.Message & vbCrLf & vbCrLf & "ReportServer: " & _
                     m_ReportServerURL, MsgBoxStyle.Critical, "Reporting Error")
        Finally
            ' Reset the cursor to the default for all controls.
            Cursor.Current = System.Windows.Forms.Cursors.Default
        End Try
    End Sub

    Private Sub ReportViewer_Deactivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Deactivate
        If m_AutoCloseViewer Then
            Me.Close()
        End If
    End Sub

    Private Sub ReportViewer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.TopMost = True

        'Add by Guo Wenyu 2014/04/08 for display winform in Extended Monitor
        Dim screen_width As Integer
        Dim screen_heigh As Integer
        Dim pos_x As Integer
        Dim pos_y As Integer
        screen_width = Screen.PrimaryScreen.Bounds.Width
        screen_heigh = Screen.PrimaryScreen.Bounds.Height
        pos_y = CInt(screen_heigh / 2 - Me.ClientSize.Height / 2)
        pos_x = CInt(screen_width / 4 - Me.ClientSize.Width / 2)
        Me.Location = New Point(pos_x, pos_y)
        Me.TopLevel = True
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    Private Sub rptViewer_ViewButtonClick(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles rptViewer.ViewButtonClick
        'rptViewer.RefreshReport()
    End Sub
End Class