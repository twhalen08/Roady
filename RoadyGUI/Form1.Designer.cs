namespace RoadyGUI
{
    partial class RoadyGUI
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            txtWorld = new TextBox();
            txtUser = new TextBox();
            txtPass = new TextBox();
            label7 = new Label();
            label8 = new Label();
            label6 = new Label();
            btnLogin = new Button();
            tabPage2 = new TabPage();
            btnBrowse = new Button();
            btnSetDimensions = new Button();
            label5 = new Label();
            txtSegments = new TextBox();
            label4 = new Label();
            txtUvScaling = new TextBox();
            label3 = new Label();
            txtRoadWidth = new TextBox();
            label2 = new Label();
            txtFileName = new TextBox();
            label1 = new Label();
            txtCacheFolder = new TextBox();
            btnReset = new Button();
            btnStartRoad = new Button();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(681, 254);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(txtWorld);
            tabPage1.Controls.Add(txtUser);
            tabPage1.Controls.Add(txtPass);
            tabPage1.Controls.Add(label7);
            tabPage1.Controls.Add(label8);
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(btnLogin);
            tabPage1.Location = new Point(4, 32);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(673, 218);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Connection";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtWorld
            // 
            txtWorld.Location = new Point(118, 93);
            txtWorld.Name = "txtWorld";
            txtWorld.Size = new Size(125, 30);
            txtWorld.TabIndex = 2;
            // 
            // txtUser
            // 
            txtUser.Location = new Point(118, 21);
            txtUser.Name = "txtUser";
            txtUser.Size = new Size(125, 30);
            txtUser.TabIndex = 0;
            // 
            // txtPass
            // 
            txtPass.Location = new Point(118, 54);
            txtPass.Name = "txtPass";
            txtPass.PasswordChar = '*';
            txtPass.Size = new Size(125, 30);
            txtPass.TabIndex = 1;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(18, 57);
            label7.Name = "label7";
            label7.Size = new Size(80, 23);
            label7.TabIndex = 7;
            label7.Text = "Password";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(57, 93);
            label8.Name = "label8";
            label8.Size = new Size(55, 23);
            label8.TabIndex = 5;
            label8.Text = "World";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(18, 21);
            label6.Name = "label6";
            label6.Size = new Size(87, 23);
            label6.TabIndex = 1;
            label6.Text = "Username";
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(149, 135);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(94, 29);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(btnBrowse);
            tabPage2.Controls.Add(btnSetDimensions);
            tabPage2.Controls.Add(label5);
            tabPage2.Controls.Add(txtSegments);
            tabPage2.Controls.Add(label4);
            tabPage2.Controls.Add(txtUvScaling);
            tabPage2.Controls.Add(label3);
            tabPage2.Controls.Add(txtRoadWidth);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(txtFileName);
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(txtCacheFolder);
            tabPage2.Controls.Add(btnReset);
            tabPage2.Controls.Add(btnStartRoad);
            tabPage2.Location = new Point(4, 32);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(673, 218);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Road";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(552, 57);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(94, 29);
            btnBrowse.TabIndex = 14;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnSetDimensions
            // 
            btnSetDimensions.Location = new Point(563, 104);
            btnSetDimensions.Name = "btnSetDimensions";
            btnSetDimensions.Size = new Size(83, 31);
            btnSetDimensions.TabIndex = 1;
            btnSetDimensions.Text = "Set";
            btnSetDimensions.UseVisualStyleBackColor = true;
            btnSetDimensions.Click += btnSetDimensions_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(390, 107);
            label5.Name = "label5";
            label5.Size = new Size(85, 23);
            label5.TabIndex = 13;
            label5.Text = "Segments";
            label5.Click += label5_Click;
            // 
            // txtSegments
            // 
            txtSegments.Location = new Point(481, 104);
            txtSegments.Name = "txtSegments";
            txtSegments.Size = new Size(76, 30);
            txtSegments.TabIndex = 12;
            txtSegments.Text = "20";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(200, 107);
            label4.Name = "label4";
            label4.Size = new Size(92, 23);
            label4.TabIndex = 11;
            label4.Text = "UV Scaling";
            // 
            // txtUvScaling
            // 
            txtUvScaling.Location = new Point(308, 104);
            txtUvScaling.Name = "txtUvScaling";
            txtUvScaling.Size = new Size(76, 30);
            txtUvScaling.TabIndex = 10;
            txtUvScaling.Text = "1.0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 107);
            label3.Name = "label3";
            label3.Size = new Size(100, 23);
            label3.TabIndex = 9;
            label3.Text = "Road Width";
            // 
            // txtRoadWidth
            // 
            txtRoadWidth.Location = new Point(118, 105);
            txtRoadWidth.Name = "txtRoadWidth";
            txtRoadWidth.Size = new Size(76, 30);
            txtRoadWidth.TabIndex = 8;
            txtRoadWidth.Text = "1.0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(36, 24);
            label2.Name = "label2";
            label2.Size = new Size(86, 23);
            label2.TabIndex = 7;
            label2.Text = "File Name";
            // 
            // txtFileName
            // 
            txtFileName.Location = new Point(128, 21);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(418, 30);
            txtFileName.TabIndex = 6;
            txtFileName.Text = "testroad1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 60);
            label1.Name = "label1";
            label1.Size = new Size(110, 23);
            label1.TabIndex = 5;
            label1.Text = "Model Cache";
            // 
            // txtCacheFolder
            // 
            txtCacheFolder.Location = new Point(128, 57);
            txtCacheFolder.Name = "txtCacheFolder";
            txtCacheFolder.Size = new Size(418, 30);
            txtCacheFolder.TabIndex = 4;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(508, 155);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(138, 46);
            btnReset.TabIndex = 3;
            btnReset.Text = "Clear/Reset";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // btnStartRoad
            // 
            btnStartRoad.Location = new Point(364, 155);
            btnStartRoad.Name = "btnStartRoad";
            btnStartRoad.Size = new Size(138, 46);
            btnStartRoad.TabIndex = 2;
            btnStartRoad.Text = "Start Road";
            btnStartRoad.UseVisualStyleBackColor = true;
            btnStartRoad.Click += btnStartRoad_Click;
            // 
            // RoadyGUI
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(706, 276);
            Controls.Add(tabControl1);
            Name = "RoadyGUI";
            RightToLeftLayout = true;
            Text = "Roady";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button btnStartRoad;
        private Button btnSetDimensions;
        private Label label3;
        private TextBox txtRoadWidth;
        private Label label2;
        private TextBox txtFileName;
        private Label label1;
        private TextBox txtCacheFolder;
        private Button btnReset;
        private Label label5;
        private TextBox txtSegments;
        private Label label4;
        private TextBox txtUvScaling;
        private Button btnBrowse;
        private TextBox textBox6;
        private Label label8;
        private MaskedTextBox maskedTextBox1;
        private TextBox textBox1;
        private Label label6;
        private Button btnLogin;
        private TextBox txtPass;
        private Label label7;
        private TextBox txtWorld;
        private TextBox txtUser;
    }
}
