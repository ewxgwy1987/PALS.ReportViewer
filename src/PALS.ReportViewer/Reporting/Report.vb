'=====================================================================================
' Copyright 2007, Xu Jian, All Rights Reserved.
'=====================================================================================
'FileName       Report.vb
'Revision:      0.0 -   04 Oct 2007, By Xu Jian, First created in VB.Net 2005.
'               1.0 -   09 Oct 2009, By Xu Jian, Revised in VB.NET 2008.
'=====================================================================================

Option Explicit On
Option Strict On


Namespace Reporting

    Public Class Report

#Region "1. Class Fields Declaration."

        Private m_Name As String
        Private m_Type As String
        Private m_Group As String
        Private m_Enabled As Boolean
        Private m_ReportServerURL As String
        Private m_ReportPath As String
        Private m_NeedDateTimeFormat As Boolean
        Private m_ReportParams() As Reporting.ReportParameter
        Private m_Authentication As Boolean

        Private m_SubReports() As Reporting.Report
#End Region

#Region "2. Class Constructor and Destructor Declaration."

        Public Sub New()

        End Sub

        Public Sub New(ByVal Name As String)
            m_Name = Name
        End Sub

#End Region

#Region "3. Class Properties Defination."

        Public Property Group() As String
            Get
                Return m_Group
            End Get
            Set(ByVal Value As String)
                m_Group = Value
            End Set
        End Property

        Public Property ReportServerURL() As String
            Get
                Return m_ReportServerURL
            End Get
            Set(ByVal Value As String)
                m_ReportServerURL = Value
            End Set
        End Property

        Public Property ReportPath() As String
            Get
                Return m_ReportPath
            End Get
            Set(ByVal Value As String)
                m_ReportPath = Value
            End Set
        End Property

        Public ReadOnly Property ReportParamsCount() As Integer
            Get
                Dim Count As Integer = 0

                If m_ReportParams Is Nothing Then
                    Count = 0
                Else
                    Count = m_ReportParams.Length
                End If

                Return Count
            End Get
        End Property

        Public Property ReportParams() As ReportParameter()
            Get
                Return m_ReportParams
            End Get
            Set(ByVal Value As ReportParameter())
                m_ReportParams = Value
            End Set
        End Property

        Public ReadOnly Property SubReportCount() As Integer
            Get
                Dim Count As Integer = 0

                If m_SubReports Is Nothing Then
                    Count = 0
                Else
                    Count = m_SubReports.Length
                End If

                Return Count
            End Get
        End Property

        Public Property SubReports() As Report()
            Get
                Return m_SubReports
            End Get
            Set(ByVal Value As Report())
                m_SubReports = Value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal Value As String)
                m_Name = Value
            End Set
        End Property

        Public Property Type() As String
            Get
                Return m_Type
            End Get
            Set(ByVal Value As String)
                m_Type = Value
            End Set
        End Property

        Public Property Enabled() As Boolean
            Get
                Return m_Enabled
            End Get
            Set(ByVal Value As Boolean)
                m_Enabled = Value
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

        Public Property Authentication() As Boolean
            Get
                Return m_Authentication
            End Get
            Set(ByVal Value As Boolean)
                m_Authentication = Value
            End Set
        End Property


#End Region

    End Class

End Namespace