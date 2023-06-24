namespace VisualEditor
{
    partial class MainWindow
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
            this.RoomList = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // RoomList
            // 
            this.RoomList.Dock = System.Windows.Forms.DockStyle.Left;
            this.RoomList.HideSelection = false;
            this.RoomList.Location = new System.Drawing.Point(0, 0);
            this.RoomList.Name = "RoomList";
            this.RoomList.Size = new System.Drawing.Size(121, 561);
            this.RoomList.TabIndex = 0;
            this.RoomList.UseCompatibleStateImageBehavior = false;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.RoomList);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CelesteMapEditor";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView RoomList;
    }
}

