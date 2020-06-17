namespace CustomControlTest
{
    partial class Test
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Test));
            this.OpenImage = new System.Windows.Forms.Button();
            this.Image = new DevComponents.DotNetBar.Controls.ReflectionImage();
            this.tText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // OpenImage
            // 
            this.OpenImage.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.OpenImage.Location = new System.Drawing.Point(227, 344);
            this.OpenImage.Name = "OpenImage";
            this.OpenImage.Size = new System.Drawing.Size(75, 23);
            this.OpenImage.TabIndex = 0;
            this.OpenImage.Text = "Open Image";
            this.OpenImage.UseVisualStyleBackColor = true;
            this.OpenImage.Click += new System.EventHandler(this.OpenImage_Click);
            // 
            // Image
            // 
            this.Image.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            // 
            // 
            // 
            this.Image.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.Image.BackgroundStyle.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.Image.Image = ((System.Drawing.Image)(resources.GetObject("Image.Image")));
            this.Image.Location = new System.Drawing.Point(12, 278);
            this.Image.Name = "Image";
            this.Image.Size = new System.Drawing.Size(72, 122);
            this.Image.TabIndex = 1;
            // 
            // tText
            // 
            this.tText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tText.Location = new System.Drawing.Point(503, 346);
            this.tText.Name = "tText";
            this.tText.Size = new System.Drawing.Size(53, 21);
            this.tText.TabIndex = 2;
            // 
            // Test
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 412);
            this.Controls.Add(this.tText);
            this.Controls.Add(this.Image);
            this.Controls.Add(this.OpenImage);
            this.Name = "Test";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OpenImage;
        private DevComponents.DotNetBar.Controls.ReflectionImage Image;
        private System.Windows.Forms.TextBox tText;
    }
}

