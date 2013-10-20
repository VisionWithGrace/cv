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
            this.components = new System.ComponentModel.Container();
            this.emguColorImageBox = new Emgu.CV.UI.ImageBox();
            this.emguDepthImageBox = new Emgu.CV.UI.ImageBox();
            this.emguDepthProcessedImageBox = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)(this.emguColorImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emguDepthImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emguDepthProcessedImageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // emguColorImageBox
            // 
            this.emguColorImageBox.Location = new System.Drawing.Point(11, 11);
            this.emguColorImageBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.emguColorImageBox.Name = "emguColorImageBox";
            this.emguColorImageBox.Size = new System.Drawing.Size(334, 215);
            this.emguColorImageBox.TabIndex = 2;
            this.emguColorImageBox.TabStop = false;
            this.emguColorImageBox.Click += new System.EventHandler(this.emguColorImageBox_Click);
            // 
            // emguDepthImageBox
            // 
            this.emguDepthImageBox.Location = new System.Drawing.Point(349, 11);
            this.emguDepthImageBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.emguDepthImageBox.Name = "emguDepthImageBox";
            this.emguDepthImageBox.Size = new System.Drawing.Size(334, 215);
            this.emguDepthImageBox.TabIndex = 3;
            this.emguDepthImageBox.TabStop = false;
            // 
            // emguDepthProcessedImageBox
            // 
            this.emguDepthProcessedImageBox.Location = new System.Drawing.Point(349, 230);
            this.emguDepthProcessedImageBox.Margin = new System.Windows.Forms.Padding(2);
            this.emguDepthProcessedImageBox.Name = "emguDepthProcessedImageBox";
            this.emguDepthProcessedImageBox.Size = new System.Drawing.Size(334, 215);
            this.emguDepthProcessedImageBox.TabIndex = 4;
            this.emguDepthProcessedImageBox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(695, 456);
            this.Controls.Add(this.emguDepthProcessedImageBox);
            this.Controls.Add(this.emguDepthImageBox);
            this.Controls.Add(this.emguColorImageBox);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.emguColorImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emguDepthImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emguDepthProcessedImageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Emgu.CV.UI.ImageBox emguColorImageBox;
        private Emgu.CV.UI.ImageBox emguDepthImageBox;
        private Emgu.CV.UI.ImageBox emguDepthProcessedImageBox;
    }
}