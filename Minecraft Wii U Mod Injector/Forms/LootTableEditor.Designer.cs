﻿namespace Minecraft_Wii_U_Mod_Injector.Forms
{
    partial class LootTableEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LootTableEditor));
            this.StyleMngr = new MetroFramework.Components.MetroStyleManager(this.components);
            this.ToolTips = new MetroFramework.Components.MetroToolTip();
            this.withBox = new MetroFramework.Controls.MetroComboBox();
            this.ApplyBtn = new MetroFramework.Controls.MetroButton();
            this.replaceBox = new MetroFramework.Controls.MetroComboBox();
            this.withLbl = new MetroFramework.Controls.MetroLabel();
            this.replaceLbl = new MetroFramework.Controls.MetroLabel();
            ((System.ComponentModel.ISupportInitialize)(this.StyleMngr)).BeginInit();
            this.SuspendLayout();
            // 
            // StyleMngr
            // 
            this.StyleMngr.Owner = this;
            this.StyleMngr.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // ToolTips
            // 
            this.ToolTips.Style = MetroFramework.MetroColorStyle.Default;
            this.ToolTips.StyleManager = null;
            this.ToolTips.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // withBox
            // 
            this.withBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.withBox.DisplayMember = "1";
            this.withBox.FormattingEnabled = true;
            this.withBox.ItemHeight = 23;
            this.withBox.Items.AddRange(new object[] {
            "Bat",
            "Blaze",
            "Cave Spider",
            "Chicken",
            "Cow",
            "Creeper",
            "Dolphin",
            "Donkey",
            "Drowned",
            "Elder Guardian",
            "Ender Dragon",
            "Ender Man",
            "Endermite",
            "Evocation Illager",
            "Fish",
            "Ghast",
            "Giant",
            "Guardian",
            "Horse",
            "Husk",
            "Illusion Illager",
            "Llama",
            "Magma Cube",
            "Mule",
            "Ocelot",
            "Parrot",
            "Phantom",
            "Pig",
            "Pig Zombie",
            "Polar Bear",
            "Pufferfish",
            "Rabbit",
            "Salmon",
            "Sheep",
            "Shulker",
            "Skeleton",
            "Skeleton Horse",
            "Slime",
            "Snow Man",
            "Spider",
            "Squid",
            "Stray",
            "Tropical Fish",
            "Turtle",
            "Vex",
            "Villager",
            "Iron Golem",
            "Vindication Illager",
            "Witch",
            "Wither Skeleton",
            "Wolf",
            "Zombie",
            "Zombie Horse",
            "Zombie Villager"});
            this.withBox.Location = new System.Drawing.Point(72, 98);
            this.withBox.MaxDropDownItems = 3;
            this.withBox.Name = "withBox";
            this.withBox.Size = new System.Drawing.Size(468, 29);
            this.withBox.TabIndex = 8;
            this.withBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.ToolTips.SetToolTip(this.withBox, "What loot table should said entity be replaced with?");
            this.withBox.UseSelectable = true;
            this.withBox.ValueMember = "1";
            // 
            // ApplyBtn
            // 
            this.ApplyBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ApplyBtn.Location = new System.Drawing.Point(23, 133);
            this.ApplyBtn.Name = "ApplyBtn";
            this.ApplyBtn.Size = new System.Drawing.Size(517, 23);
            this.ApplyBtn.TabIndex = 9;
            this.ApplyBtn.Text = "Apply";
            this.ApplyBtn.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.ToolTips.SetToolTip(this.ApplyBtn, "Applies the loot table");
            this.ApplyBtn.UseSelectable = true;
            this.ApplyBtn.Click += new System.EventHandler(this.ApplyBtnClicked);
            // 
            // replaceBox
            // 
            this.replaceBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.replaceBox.DisplayMember = "1";
            this.replaceBox.FormattingEnabled = true;
            this.replaceBox.ItemHeight = 23;
            this.replaceBox.Items.AddRange(new object[] {
            "Bat",
            "Blaze",
            "Cave Spider",
            "Chicken",
            "Cow",
            "Creeper",
            "Dolphin",
            "Donkey",
            "Drowned",
            "Elder Guardian",
            "Ender Dragon",
            "Ender Man",
            "Endermite",
            "Evocation Illager",
            "Fish",
            "Ghast",
            "Giant",
            "Guardian",
            "Horse",
            "Husk",
            "Illusion Illager",
            "Llama",
            "Magma Cube",
            "Mule",
            "Ocelot",
            "Parrot",
            "Phantom",
            "Pig",
            "Pig Zombie",
            "Polar Bear",
            "Pufferfish",
            "Rabbit",
            "Salmon",
            "Sheep",
            "Shulker",
            "Skeleton",
            "Skeleton Horse",
            "Slime",
            "Snow Man",
            "Spider",
            "Squid",
            "Stray",
            "Tropical Fish",
            "Turtle",
            "Vex",
            "Villager",
            "Iron Golem",
            "Vindication Illager",
            "Witch",
            "Wither Skeleton",
            "Wolf",
            "Zombie",
            "Zombie Horse",
            "Zombie Villager"});
            this.replaceBox.Location = new System.Drawing.Point(100, 63);
            this.replaceBox.MaxDropDownItems = 3;
            this.replaceBox.Name = "replaceBox";
            this.replaceBox.Size = new System.Drawing.Size(440, 29);
            this.replaceBox.TabIndex = 11;
            this.replaceBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.ToolTips.SetToolTip(this.replaceBox, "What entity\'s loot table should be swapped?");
            this.replaceBox.UseSelectable = true;
            this.replaceBox.ValueMember = "0";
            // 
            // withLbl
            // 
            this.withLbl.AutoSize = true;
            this.withLbl.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.withLbl.Location = new System.Drawing.Point(19, 98);
            this.withLbl.Name = "withLbl";
            this.withLbl.Size = new System.Drawing.Size(47, 25);
            this.withLbl.TabIndex = 7;
            this.withLbl.Text = "with:";
            this.withLbl.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // replaceLbl
            // 
            this.replaceLbl.AutoSize = true;
            this.replaceLbl.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.replaceLbl.Location = new System.Drawing.Point(19, 63);
            this.replaceLbl.Name = "replaceLbl";
            this.replaceLbl.Size = new System.Drawing.Size(75, 25);
            this.replaceLbl.TabIndex = 10;
            this.replaceLbl.Text = "Replace:";
            this.replaceLbl.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // LootTableEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 175);
            this.Controls.Add(this.replaceBox);
            this.Controls.Add(this.replaceLbl);
            this.Controls.Add(this.ApplyBtn);
            this.Controls.Add(this.withBox);
            this.Controls.Add(this.withLbl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(3840, 175);
            this.MinimumSize = new System.Drawing.Size(565, 175);
            this.Name = "LootTableEditor";
            this.Text = "Minecraft: Wii U Mod Injector - Loot Table Editor";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Exiting);
            this.Load += new System.EventHandler(this.Init);
            ((System.ComponentModel.ISupportInitialize)(this.StyleMngr)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MetroFramework.Components.MetroStyleManager StyleMngr;
        private MetroFramework.Components.MetroToolTip ToolTips;
        private MetroFramework.Controls.MetroComboBox withBox;
        private MetroFramework.Controls.MetroLabel withLbl;
        private MetroFramework.Controls.MetroButton ApplyBtn;
        private MetroFramework.Controls.MetroComboBox replaceBox;
        private MetroFramework.Controls.MetroLabel replaceLbl;
    }
}