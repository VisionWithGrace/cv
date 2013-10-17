namespace HelloWorld
{
    partial class Form1
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
            this.emguColorImageBox = new Emgu.CV.UI.ImageBox();
            this.emguDepthImageBox = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)(this.emguColorImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emguDepthImageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // emguColorImageBox
            // 
            this.emguColorImageBox.Location = new System.Drawing.Point(12, 12);
            this.emguColorImageBox.Name = "emguColorImageBox";
            this.emguColorImageBox.Size = new System.Drawing.Size(345, 323);
            this.emguColorImageBox.TabIndex = 2;
            this.emguColorImageBox.TabStop = false;
            // 
            // emguDepthImageBox
            // 
            this.emguDepthImageBox.Location = new System.Drawing.Point(363, 12);
            this.emguDepthImageBox.Name = "emguDepthImageBox";
            this.emguDepthImageBox.Size = new System.Drawing.Size(353, 323);
            this.emguDepthImageBox.TabIndex = 3;
            this.emguDepthImageBox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 347);
            this.Controls.Add(this.emguDepthImageBox);
            this.Controls.Add(this.emguColorImageBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.emguColorImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emguDepthImageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Emgu.CV.UI.ImageBox emguColorImageBox;
        private Emgu.CV.UI.ImageBox emguDepthImageBox;
    }
}