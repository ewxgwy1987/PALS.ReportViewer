'=====================================================================================
' Copyright 2007, Xu Jian, All Rights Reserved.
'=====================================================================================
'FileName       ReportParameter.vb
'Revision:      0.0 -   04 Oct 2007, By Xu Jian, First created in VB.Net 2005.
'=====================================================================================

Option Explicit On
Option Strict On

Namespace Reporting 'Root Namespace: PALS

    Public Class ReportParameter

#Region "1. Class Fields Declaration."
        Private m_Name As String
        Private m_DataType As String
        Private m_Value As String
        Private m_DateOnly As Boolean
#End Region

#Region "2. Class Constructor and Destructor Declaration."

        Public Sub New()

        End Sub

        Public Sub New(ByVal Name As String, ByVal DataType As String, ByVal Value As String, _
                       Optional ByVal DateOnly As Boolean = False, Optional ByVal DateFormat As String = Nothing)
            m_Name = Name
            m_DataType = DataType
            m_Value = Value
            m_DateOnly = DateOnly
        End Sub

#End Region

#Region "3. Class Properties Defination."

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal Value As String)
                m_Name = Value
            End Set
        End Property

        Public Property DataType() As String
            Get
                Return m_DataType
            End Get
            Set(ByVal Value As String)
                m_DataType = Value
            End Set
        End Property

        Public Property Value() As String
            Get
                Return m_Value
            End Get
            Set(ByVal Value As String)
                m_Value = Value
            End Set
        End Property

        Public Property DateOnly() As Boolean
            Get
                Return m_DateOnly
            End Get
            Set(ByVal Value As Boolean)
                m_DateOnly = Value
            End Set
        End Property

#End Region



    End Class

End Namespace


