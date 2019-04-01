namespace iFakeLocation
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainLabel = new System.Windows.Forms.Label();
            this.deviceComboBox = new System.Windows.Forms.ComboBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.mainGMapControl = new GMap.NET.WindowsForms.GMapControl();
            this.locationTextBox = new System.Windows.Forms.TextBox();
            this.searchLabel = new System.Windows.Forms.Label();
            this.goButton = new System.Windows.Forms.Button();
            this.hintLabel = new System.Windows.Forms.Label();
            this.sendButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.baseMapComboBox = new System.Windows.Forms.ComboBox();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.mainTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLabel
            // 
            this.mainLabel.AutoSize = true;
            this.mainLabel.Location = new System.Drawing.Point(12, 9);
            this.mainLabel.Name = "mainLabel";
            this.mainLabel.Size = new System.Drawing.Size(75, 13);
            this.mainLabel.TabIndex = 0;
            this.mainLabel.Text = "Device Name:";
            // 
            // deviceComboBox
            // 
            this.deviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceComboBox.FormattingEnabled = true;
            this.deviceComboBox.Location = new System.Drawing.Point(15, 25);
            this.deviceComboBox.Name = "deviceComboBox";
            this.deviceComboBox.Size = new System.Drawing.Size(443, 21);
            this.deviceComboBox.TabIndex = 1;
            // 
            // refreshButton
            // 
            this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshButton.Location = new System.Drawing.Point(464, 24);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(93, 23);
            this.refreshButton.TabIndex = 2;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // mainGMapControl
            // 
            this.mainGMapControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainGMapControl.Bearing = 0F;
            this.mainGMapControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mainGMapControl.CanDragMap = true;
            this.mainGMapControl.EmptyTileColor = System.Drawing.Color.Navy;
            this.mainGMapControl.GrayScaleMode = false;
            this.mainGMapControl.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.mainGMapControl.LevelsKeepInMemmory = 5;
            this.mainGMapControl.Location = new System.Drawing.Point(15, 52);
            this.mainGMapControl.MarkersEnabled = true;
            this.mainGMapControl.MaxZoom = 2;
            this.mainGMapControl.MinZoom = 2;
            this.mainGMapControl.MouseWheelZoomEnabled = true;
            this.mainGMapControl.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            this.mainGMapControl.Name = "mainGMapControl";
            this.mainGMapControl.NegativeMode = false;
            this.mainGMapControl.PolygonsEnabled = true;
            this.mainGMapControl.RetryLoadTile = 0;
            this.mainGMapControl.RoutesEnabled = true;
            this.mainGMapControl.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.mainGMapControl.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.mainGMapControl.ShowTileGridLines = false;
            this.mainGMapControl.Size = new System.Drawing.Size(542, 291);
            this.mainGMapControl.TabIndex = 3;
            this.mainGMapControl.Zoom = 0D;
            this.mainGMapControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mainGMapControl_MouseDoubleClick);
            // 
            // locationTextBox
            // 
            this.locationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.locationTextBox.Location = new System.Drawing.Point(109, 377);
            this.locationTextBox.Name = "locationTextBox";
            this.locationTextBox.Size = new System.Drawing.Size(349, 22);
            this.locationTextBox.TabIndex = 4;
            // 
            // searchLabel
            // 
            this.searchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(12, 380);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(91, 13);
            this.searchLabel.TabIndex = 5;
            this.searchLabel.Text = "Search Location:";
            // 
            // goButton
            // 
            this.goButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.goButton.Location = new System.Drawing.Point(464, 376);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(93, 23);
            this.goButton.TabIndex = 6;
            this.goButton.Text = "Search";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // hintLabel
            // 
            this.hintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintLabel.AutoSize = true;
            this.hintLabel.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hintLabel.Location = new System.Drawing.Point(14, 353);
            this.hintLabel.Name = "hintLabel";
            this.hintLabel.Size = new System.Drawing.Size(398, 12);
            this.hintLabel.TabIndex = 7;
            this.hintLabel.Text = "Click and drag mouse to move. Use mouse wheel to zoom. Double-click to manually s" +
    "elect a location.";
            // 
            // sendButton
            // 
            this.sendButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sendButton.Enabled = false;
            this.sendButton.Location = new System.Drawing.Point(3, 3);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(269, 25);
            this.sendButton.TabIndex = 8;
            this.sendButton.Text = "Send Location To Device";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(278, 3);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(269, 25);
            this.stopButton.TabIndex = 9;
            this.stopButton.Text = "Stop Fake Location";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // baseMapComboBox
            // 
            this.baseMapComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseMapComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.baseMapComboBox.FormattingEnabled = true;
            this.baseMapComboBox.Items.AddRange(new object[] {
            "Google Maps",
            "OpenStreetMaps"});
            this.baseMapComboBox.Location = new System.Drawing.Point(431, 349);
            this.baseMapComboBox.Name = "baseMapComboBox";
            this.baseMapComboBox.Size = new System.Drawing.Size(126, 21);
            this.baseMapComboBox.TabIndex = 10;
            this.baseMapComboBox.SelectedIndexChanged += new System.EventHandler(this.baseMapComboBox_SelectedIndexChanged);
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.ColumnCount = 2;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Controls.Add(this.sendButton, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.stopButton, 1, 0);
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(10, 402);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 1;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(550, 31);
            this.mainTableLayoutPanel.TabIndex = 11;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 439);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Controls.Add(this.baseMapComboBox);
            this.Controls.Add(this.hintLabel);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.searchLabel);
            this.Controls.Add(this.locationTextBox);
            this.Controls.Add(this.mainGMapControl);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.deviceComboBox);
            this.Controls.Add(this.mainLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MinimumSize = new System.Drawing.Size(585, 478);
            this.Name = "MainForm";
            this.Text = "iFakeLocation";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mainLabel;
        private System.Windows.Forms.ComboBox deviceComboBox;
        private System.Windows.Forms.Button refreshButton;
        private GMap.NET.WindowsForms.GMapControl mainGMapControl;
        private System.Windows.Forms.TextBox locationTextBox;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.Button goButton;
        private System.Windows.Forms.Label hintLabel;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.ComboBox baseMapComboBox;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
    }
}

