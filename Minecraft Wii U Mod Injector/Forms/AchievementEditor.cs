﻿using MetroFramework.Forms;
using Minecraft_Wii_U_Mod_Injector.Helpers;
using System;
using System.Windows.Forms;
using Minecraft_Wii_U_Mod_Injector.Helpers.Win_Forms;

namespace Minecraft_Wii_U_Mod_Injector.Forms {
    public partial class AchievementEditor : MetroForm {

        // Why did 4J not use achievement ID 0x1C???? This just makes it so much more confusing...
        //static uint[] achievementAddresses = {
        //    0x023FA41C, 0x023FA41C + (0x1C * 1), 0x023FA41C + (0x1C * 2), 0x023FA41C + (0x1C * 3), 0x023FA41C + (0x1C * 4), 0x023FA41C + (0x1C * 5), 0x023FA41C + (0x1C * 6), 0x023FA41C + (0x1C * 7), 0x023FA41C + (0x1C * 8), 0x023FA41C + (0x1C * 9), 0x023FA41C + (0x1C * 10), 0x023FA41C + (0x1C * 11), 0x023FA41C + (0x1C * 12), 0x023FA41C + (0x1C * 13), 0x023FA41C + (0x1C * 14), 0x023FA41C + (0x1C * 15), 0x023FA6F4, 0x023FA710, 0x023FA72C, 0x023FA748, 0x023FA80C, 0x023FA828, 0x023FA844, 0x023FA5DC, 0x023FA5F8, 0x023FA614, 0x023FA630, 0x023FA64C, 0x023FA668, 0x023FA684, 0x023FA6A0, 0x023FA6BC, 0x023FA6D8, 0x023FA7B8, 0x023FA7D4, 0x023FA7F0, 0x023FA860, 0x023FA87C, 0x023FA898, 0x023FA8B4, 0x023FA8D0, 0x023FA8EC, 0x023FA908, 0x023FA924, 0x023FA924 + (0x1C * 1), 0x023FA924 + (0x1C * 2), 0x023FA924 + (0x1C * 3), 0x023FA924 + (0x1C * 4), 0x023FA924 + (0x1C * 5), 0x023FA924 + (0x1C * 6), 0x023FA924 + (0x1C * 7), 0x023FA924 + (0x1C * 8), 0x023FA924 + (0x1C * 9), 0x023FA924 + (0x1C * 10), 0x023FA924 + (0x1C * 11), 0x023FA924 + (0x1C * 12), 0x023FA924 + (0x1C * 13), 0x023FA924 + (0x1C * 14), 0x023FA924 + (0x1C * 15), 0x023FA924 + (0x1C * 16), 0x023FA924 + (0x1C * 17), 0x023FA924 + (0x1C * 18), 0x023FA924 + (0x1C * 19), 0x023FA924 + (0x1C * 20), 0x023FA924 + (0x1C * 21), 0x023FA924 + (0x1C * 22), 0x023FA924 + (0x1C * 23), 0x023FA924 + (0x1C * 24), 0x023FA924 + (0x1C * 25), 0x023FA924 + (0x1C * 26), 0x023FA924 + (0x1C * 27), 0x023FA924 + (0x1C * 28), 0x023FA924 + (0x1C * 29), 0x023FA924 + (0x1C * 30), 0x023FA924 + (0x1C * 31), 0x023FA924 + (0x1C * 32), 0x023FA924 + (0x1C * 33), 0x023FA924 + (0x1C * 34), 0x023FA924 + (0x1C * 35), 0x023FA924 + (0x1C * 36), 0x023FA924 + (0x1C * 37), 0x023FA924 + (0x1C * 38), 0x023FA924 + (0x1C * 39), 0x023FA924 + (0x1C * 40), 0x023FA924 + (0x1C * 41), 0x023FA924 + (0x1C * 42), 0x023FA924 + (0x1C * 43), 0x023FA924 + (0x1C * 44), 0x023FA924 + (0x1C * 45), 0x023FA924 + (0x1C * 46), 0x023FA924 + (0x1C * 47), 0x023FA924 + (0x1C * 48), 0x023FA924 + (0x1C * 49)
        //};
        //static ushort[] achievementIDs = {
        //    0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x16, 0x17, 0x18, 0x1A, 0x1B, 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F, 0x60
        //};

        public AchievementEditor(MainForm injector) {
            InitializeComponent();
            StyleMngr.Style = Style = injector.StyleMngr.Style;
            StyleMngr.Theme = Theme = injector.StyleMngr.Theme;
        }

        private void Init(object sender, EventArgs e) {
            DiscordRp.SetPresence("Connected", "Achievement Editor");
        }

        private void Exiting(object sender, FormClosingEventArgs e) {
            DiscordRp.SetPresence("Connected", new MainForm().worldTab.Text + " tab");
        }

        private void ApplyBtnClicked(object sender, EventArgs e) {
            uint address = ((uint)(replaceBox.SelectedIndex) * 0x1cu) + 0x023FA41Cu;
            MainForm.GeckoU.WriteUInt(address, 0x38800000u + (uint)(withBox.SelectedIndex)); // why bother listing :)
            this.Close();
        }
    }
}
