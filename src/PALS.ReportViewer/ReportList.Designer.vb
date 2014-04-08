<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ReportList
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ReportList))
        Me.gbType = New System.Windows.Forms.GroupBox()
        Me.rbType6 = New System.Windows.Forms.RadioButton()
        Me.rbType5 = New System.Windows.Forms.RadioButton()
        Me.rbType1 = New System.Windows.Forms.RadioButton()
        Me.rbType2 = New System.Windows.Forms.RadioButton()
        Me.rbType4 = New System.Windows.Forms.RadioButton()
        Me.rbType3 = New System.Windows.Forms.RadioButton()
        Me.btnPreview = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.lbReports = New System.Windows.Forms.ListBox()
        Me.tvReports = New System.Windows.Forms.TreeView()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ApplicationInfoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.gbType.SuspendLayout()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'gbType
        '
        Me.gbType.Controls.Add(Me.rbType6)
        Me.gbType.Controls.Add(Me.rbType5)
        Me.gbType.Controls.Add(Me.rbType1)
        Me.gbType.Controls.Add(Me.rbType2)
        Me.gbType.Controls.Add(Me.rbType4)
        Me.gbType.Controls.Add(Me.rbType3)
        Me.gbType.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.gbType.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.gbType.ForeColor = System.Drawing.SystemColors.ActiveCaption
        Me.gbType.Location = New System.Drawing.Point(372, 6)
        Me.gbType.Name = "gbType"
        Me.gbType.Size = New System.Drawing.Size(110, 133)
        Me.gbType.TabIndex = 1
        Me.gbType.TabStop = False
        Me.gbType.Text = "Type"
        '
        'rbType6
        '
        Me.rbType6.AutoSize = True
        Me.rbType6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbType6.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rbType6.Location = New System.Drawing.Point(10, 107)
        Me.rbType6.Name = "rbType6"
        Me.rbType6.Size = New System.Drawing.Size(55, 17)
        Me.rbType6.TabIndex = 1
        Me.rbType6.TabStop = True
        Me.rbType6.Text = "Type6"
        Me.rbType6.UseVisualStyleBackColor = True
        Me.rbType6.Visible = False
        '
        'rbType5
        '
        Me.rbType5.AutoSize = True
        Me.rbType5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbType5.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rbType5.Location = New System.Drawing.Point(10, 90)
        Me.rbType5.Name = "rbType5"
        Me.rbType5.Size = New System.Drawing.Size(55, 17)
        Me.rbType5.TabIndex = 0
        Me.rbType5.TabStop = True
        Me.rbType5.Text = "Type5"
        Me.rbType5.UseVisualStyleBackColor = True
        Me.rbType5.Visible = False
        '
        'rbType1
        '
        Me.rbType1.AutoSize = True
        Me.rbType1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbType1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rbType1.Location = New System.Drawing.Point(10, 19)
        Me.rbType1.Name = "rbType1"
        Me.rbType1.Size = New System.Drawing.Size(70, 17)
        Me.rbType1.TabIndex = 0
        Me.rbType1.TabStop = True
        Me.rbType1.Text = "Overview"
        Me.rbType1.UseVisualStyleBackColor = True
        Me.rbType1.Visible = False
        '
        'rbType2
        '
        Me.rbType2.AutoSize = True
        Me.rbType2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbType2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rbType2.Location = New System.Drawing.Point(10, 37)
        Me.rbType2.Name = "rbType2"
        Me.rbType2.Size = New System.Drawing.Size(78, 17)
        Me.rbType2.TabIndex = 0
        Me.rbType2.TabStop = True
        Me.rbType2.Text = "Detail View"
        Me.rbType2.UseVisualStyleBackColor = True
        Me.rbType2.Visible = False
        '
        'rbType4
        '
        Me.rbType4.AutoSize = True
        Me.rbType4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbType4.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rbType4.Location = New System.Drawing.Point(10, 72)
        Me.rbType4.Name = "rbType4"
        Me.rbType4.Size = New System.Drawing.Size(55, 17)
        Me.rbType4.TabIndex = 0
        Me.rbType4.TabStop = True
        Me.rbType4.Text = "Type4"
        Me.rbType4.UseVisualStyleBackColor = True
        Me.rbType4.Visible = False
        '
        'rbType3
        '
        Me.rbType3.AutoSize = True
        Me.rbType3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbType3.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rbType3.Location = New System.Drawing.Point(10, 54)
        Me.rbType3.Name = "rbType3"
        Me.rbType3.Size = New System.Drawing.Size(55, 17)
        Me.rbType3.TabIndex = 0
        Me.rbType3.TabStop = True
        Me.rbType3.Text = "Type3"
        Me.rbType3.UseVisualStyleBackColor = True
        Me.rbType3.Visible = False
        '
        'btnPreview
        '
        Me.btnPreview.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnPreview.Location = New System.Drawing.Point(407, 207)
        Me.btnPreview.Name = "btnPreview"
        Me.btnPreview.Size = New System.Drawing.Size(75, 21)
        Me.btnPreview.TabIndex = 3
        Me.btnPreview.Text = "Preview"
        Me.btnPreview.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnClose.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClose.Location = New System.Drawing.Point(407, 234)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(75, 21)
        Me.btnClose.TabIndex = 4
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'lbReports
        '
        Me.lbReports.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbReports.FormattingEnabled = True
        Me.lbReports.Location = New System.Drawing.Point(10, 11)
        Me.lbReports.Name = "lbReports"
        Me.lbReports.Size = New System.Drawing.Size(348, 238)
        Me.lbReports.TabIndex = 5
        '
        'tvReports
        '
        Me.tvReports.FullRowSelect = True
        Me.tvReports.HideSelection = False
        Me.tvReports.ImageIndex = 0
        Me.tvReports.ImageList = Me.ImageList1
        Me.tvReports.Location = New System.Drawing.Point(10, 11)
        Me.tvReports.Name = "tvReports"
        Me.tvReports.SelectedImageIndex = 2
        Me.tvReports.ShowNodeToolTips = True
        Me.tvReports.Size = New System.Drawing.Size(348, 255)
        Me.tvReports.TabIndex = 6
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "Group1")
        Me.ImageList1.Images.SetKeyName(1, "Group2")
        Me.ImageList1.Images.SetKeyName(2, "Report1")
        Me.ImageList1.Images.SetKeyName(3, "Report2")
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ApplicationInfoToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(178, 26)
        '
        'ApplicationInfoToolStripMenuItem
        '
        Me.ApplicationInfoToolStripMenuItem.Name = "ApplicationInfoToolStripMenuItem"
        Me.ApplicationInfoToolStripMenuItem.Size = New System.Drawing.Size(177, 22)
        Me.ApplicationInfoToolStripMenuItem.Text = "Application Info..."
        '
        'ReportList
        '
        Me.AcceptButton = Me.btnPreview
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnClose
        Me.ClientSize = New System.Drawing.Size(494, 278)
        Me.ContextMenuStrip = Me.ContextMenuStrip1
        Me.Controls.Add(Me.tvReports)
        Me.Controls.Add(Me.gbType)
        Me.Controls.Add(Me.lbReports)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnPreview)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "ReportList"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Select BHS Report"
        Me.TopMost = True
        Me.gbType.ResumeLayout(False)
        Me.gbType.PerformLayout()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents gbType As System.Windows.Forms.GroupBox
    Friend WithEvents rbType5 As System.Windows.Forms.RadioButton
    Friend WithEvents rbType4 As System.Windows.Forms.RadioButton
    Friend WithEvents rbType3 As System.Windows.Forms.RadioButton
    Friend WithEvents rbType2 As System.Windows.Forms.RadioButton
    Friend WithEvents rbType1 As System.Windows.Forms.RadioButton
    Friend WithEvents btnPreview As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents lbReports As System.Windows.Forms.ListBox
    Friend WithEvents rbType6 As System.Windows.Forms.RadioButton
    Friend WithEvents tvReports As System.Windows.Forms.TreeView
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ApplicationInfoToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
