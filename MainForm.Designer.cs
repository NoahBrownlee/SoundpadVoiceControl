namespace SoundpadVoiceControl
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
            this.mainlabel = new System.Windows.Forms.Label();
            this.errorlabel = new System.Windows.Forms.Label();
            this.listlabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainlabel
            // 
            this.mainlabel.AutoSize = true;
            this.mainlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainlabel.Location = new System.Drawing.Point(25, 25);
            this.mainlabel.Name = "mainlabel";
            this.mainlabel.Size = new System.Drawing.Size(129, 31);
            this.mainlabel.TabIndex = 0;
            this.mainlabel.Text = "mainlabel";
            // 
            // errorlabel
            // 
            this.errorlabel.AutoSize = true;
            this.errorlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorlabel.ForeColor = System.Drawing.Color.Red;
            this.errorlabel.Location = new System.Drawing.Point(500, 25);
            this.errorlabel.Name = "errorlabel";
            this.errorlabel.Size = new System.Drawing.Size(128, 31);
            this.errorlabel.TabIndex = 1;
            this.errorlabel.Text = "errorlabel";
            // 
            // listlabel
            // 
            this.listlabel.AutoSize = true;
            this.listlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listlabel.Location = new System.Drawing.Point(500, 200);
            this.listlabel.Name = "listlabel";
            this.listlabel.Size = new System.Drawing.Size(114, 31);
            this.listlabel.TabIndex = 2;
            this.listlabel.Text = "listLabel";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 445);
            this.Controls.Add(this.listlabel);
            this.Controls.Add(this.errorlabel);
            this.Controls.Add(this.mainlabel);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mainlabel;
        private System.Windows.Forms.Label errorlabel;
        private System.Windows.Forms.Label listlabel;
    }
}