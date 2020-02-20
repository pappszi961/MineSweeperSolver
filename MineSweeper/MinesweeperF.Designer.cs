namespace MineSweeper
{
    partial class MinesweeperF
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
            this.cellGrid = new MineSweeper.MinesweeperF.CellGrid();
            this.startBtn = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.difficultyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.beginnerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intermediateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.startBtn)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // cellGrid
            //
            this.cellGrid.BackColor = System.Drawing.SystemColors.ControlLight;
            this.cellGrid.Location = new System.Drawing.Point(10, 50);
            this.cellGrid.Name = "cellGrid";
            this.cellGrid.Size = new System.Drawing.Size(787, 388);
            this.cellGrid.TabIndex = 0;
            //
            // startBtn
            //
            this.startBtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.startBtn.BackgroundImage = global::MineSweeper.Properties.Resources.Start;
            this.startBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.startBtn.Location = new System.Drawing.Point(383, 12);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(35, 35);
            this.startBtn.TabIndex = 0;
            this.startBtn.TabStop = false;
            this.startBtn.Click += new System.EventHandler(this.LoadGame);
            //
            // menuStrip1
            //
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.difficultyToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            //
            // difficultyToolStripMenuItem
            //
            this.difficultyToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.beginnerToolStripMenuItem,
            this.intermediateToolStripMenuItem,
            this.expertToolStripMenuItem});
            this.difficultyToolStripMenuItem.Name = "difficultyToolStripMenuItem";
            this.difficultyToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.difficultyToolStripMenuItem.Text = "Difficulty";
            //
            // beginnerToolStripMenuItem
            //
            this.beginnerToolStripMenuItem.Name = "beginnerToolStripMenuItem";
            this.beginnerToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.beginnerToolStripMenuItem.Text = "Beginner";
            this.beginnerToolStripMenuItem.Click += new System.EventHandler(this.Beginner_Click);
            //
            // intermediateToolStripMenuItem
            //
            this.intermediateToolStripMenuItem.Name = "intermediateToolStripMenuItem";
            this.intermediateToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.intermediateToolStripMenuItem.Text = "Intermediate";
            this.intermediateToolStripMenuItem.Click += new System.EventHandler(this.Intermediate_Click);
            //
            // expertToolStripMenuItem
            //
            this.expertToolStripMenuItem.Name = "expertToolStripMenuItem";
            this.expertToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.expertToolStripMenuItem.Text = "Expert";
            this.expertToolStripMenuItem.Click += new System.EventHandler(this.Expert_Click);
            //
            // label1
            //
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(10, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 31);
            this.label1.TabIndex = 0;
            //
            // MinesweeperF
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cellGrid);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.menuStrip1);
            this.MaximizeBox = false;
            this.Name = "MinesweeperF";
            this.Text = "Minesweeper";
            ((System.ComponentModel.ISupportInitialize)(this.startBtn)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CellGrid cellGrid;
        private System.Windows.Forms.PictureBox startBtn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem difficultyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem beginnerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem intermediateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expertToolStripMenuItem;
        public System.Windows.Forms.Label label1;
    }
}

