namespace BrokenLinkChecker
{
    partial class Main
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
            this.dataGridViewErrors = new System.Windows.Forms.DataGridView();
            this.Check_Button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.URL_TextBox = new System.Windows.Forms.TextBox();
            this.progressBarCrawling = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewErrors
            // 
            this.dataGridViewErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewErrors.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dataGridViewErrors.Location = new System.Drawing.Point(0, 123);
            this.dataGridViewErrors.Name = "dataGridViewErrors";
            this.dataGridViewErrors.RowHeadersWidth = 62;
            this.dataGridViewErrors.RowTemplate.Height = 28;
            this.dataGridViewErrors.Size = new System.Drawing.Size(1422, 696);
            this.dataGridViewErrors.TabIndex = 0;
            // 
            // Check_Button
            // 
            this.Check_Button.Location = new System.Drawing.Point(984, 36);
            this.Check_Button.Name = "Check_Button";
            this.Check_Button.Size = new System.Drawing.Size(173, 46);
            this.Check_Button.TabIndex = 1;
            this.Check_Button.Text = "Start";
            this.Check_Button.UseVisualStyleBackColor = true;
            this.Check_Button.Click += new System.EventHandler(this.Check_Button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(298, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "URL:";
            // 
            // URL_TextBox
            // 
            this.URL_TextBox.Location = new System.Drawing.Point(350, 42);
            this.URL_TextBox.Name = "URL_TextBox";
            this.URL_TextBox.Size = new System.Drawing.Size(481, 26);
            this.URL_TextBox.TabIndex = 3;
            // 
            // progressBarCrawling
            // 
            this.progressBarCrawling.Location = new System.Drawing.Point(0, 109);
            this.progressBarCrawling.Name = "progressBarCrawling";
            this.progressBarCrawling.Size = new System.Drawing.Size(1422, 14);
            this.progressBarCrawling.TabIndex = 4;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1422, 819);
            this.Controls.Add(this.progressBarCrawling);
            this.Controls.Add(this.URL_TextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Check_Button);
            this.Controls.Add(this.dataGridViewErrors);
            this.Name = "Main";
            this.Text = "URL Checker";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewErrors)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewErrors;
        private System.Windows.Forms.Button Check_Button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox URL_TextBox;
        private System.Windows.Forms.ProgressBar progressBarCrawling;
    }
}

