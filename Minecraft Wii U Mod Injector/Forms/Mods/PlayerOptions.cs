﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MetroFramework.Forms;
using Minecraft_Wii_U_Mod_Injector.Helpers;
using Minecraft_Wii_U_Mod_Injector.Helpers.Files;

namespace Minecraft_Wii_U_Mod_Injector.Forms.Mods
{
    public partial class PlayerOptions : MetroForm
    {
        private readonly IniFile _savedData = new IniFile(Application.StartupPath + @"\Saved\Data\Minecraft.Wii.U.Mod.Injector.Data.ini");
        private readonly string _savedDataDir = Application.StartupPath + @"\Saved\Data\";

        private readonly uint _localPlayer = MainForm.GeckoU.PeekUInt(MainForm.GeckoU.PeekUInt(0x10A0A648) + 0x2C);

        private readonly uint[] _capeIdTable =
        {
            0x80001518, 0x80001519, 0x8000151A, 0x8000151B,
            0x8000151C, 0x80000BB8, 0x80000C81, 0x80000C82,
            0x80000C83, 0x80000C84, 0x80000C85, 0x80000C92,
            0x80000519, 0x8000051C, 0x8000051D, 0x8000051E,
            0x80000B1A, 0x80000B1B, 0x80000B17, 0x80000B1E,
            0x80000B1F, 0x80000B07, 0x80000B0B, 0x80000B0D,
            0x80000B0E, 0x80000B10, 0x80000B18, 0x80000B13,
            0x80000B14, 0x80000B0B, 0x80000B1C, 0x80000B1D,
            0x8000065A
        };

        public PlayerOptions(MainForm injector)
        {
            InitializeComponent();
            StyleMngr.Style = Style = injector.StyleMngr.Style;
            StyleMngr.Theme = Theme = injector.StyleMngr.Theme;
        }

        private void Init(object sender, EventArgs e)
        {
            DiscordRp.SetPresence("Connected", "Player Options window");
#if DEBUG
            Console.WriteLine(@"LocalPlayer: 0x" + _localPlayer.ToString("X4"));
#endif
            if (!System.IO.Directory.Exists(_savedDataDir))
                System.IO.Directory.CreateDirectory(_savedDataDir);

            foreach (string skin in _savedData.GetKeys("Skins"))
                SkinList.Items.Add(_savedData.Read(skin, "Skins") + " | " + skin);
        }

        private void Exiting(object sender, FormClosingEventArgs e)
        {
            DiscordRp.SetPresence("Connected", new MainForm().MainTabs.SelectedTab.Text + " tab");
            Dispose();
        }

        private string GetIDFromList(string skinId)
        {
            var skinIdString = skinId;
            skinIdString = skinIdString.Substring(skinIdString.IndexOf('|') + 1);
            skinIdString = Regex.Replace(skinIdString, @"\s+", "");

            return skinIdString;
        }

        private void CloseContainersBtnClicked(object sender, EventArgs e)
        {
            MainForm.GeckoU.CallFunction(0x031E8710, _localPlayer, 0x0);
        }

        private void CameraBoxChanged(object sender, EventArgs e)
        {
            MainForm.GeckoU.CallFunction(0x031F22A4, _localPlayer, (uint) CameraBox.SelectedIndex);
        }

        private void OpenScoreboardBtnClicked(object sender, EventArgs e)
        {
            MainForm.GeckoU.CallFunction(0x031A09A8, _localPlayer, 0x01);
        }

        private void GameModeBoxChanged(object sender, EventArgs e)
        {
            MainForm.GeckoU.WriteUInt(0x1B000000, 0x0);
            MainForm.GeckoU.WriteUInt(0x1B000004, 0x1);
            MainForm.GeckoU.WriteUInt(0x1B000008, 0x2);

            switch (GameModeBox.SelectedIndex)
            {
                case 0:
                    MainForm.GeckoU.CallFunction(0x031F40A0, _localPlayer, 0x1B000000);
                    break;

                case 1:
                    MainForm.GeckoU.CallFunction(0x031F40A0, _localPlayer, 0x1B000004);
                    break;

                case 2:
                    MainForm.GeckoU.CallFunction(0x031F40A0, _localPlayer, 0x1B000008);
                    break;

                case 3:
                    MainForm.GeckoU.CallFunction(0x031F49B0, _localPlayer, 0x0);
                    break;
            }
        }

        private void DropStackBtnClicked(object sender, EventArgs e)
        {
            MainForm.GeckoU.CallFunction(0x031E7534, _localPlayer, 0x0);
        }

        private void CapeBoxChanged(object sender, EventArgs e)
        {
            var capeId = _capeIdTable[CapeBox.SelectedIndex];
            MainForm.GeckoU.CallFunction(0x02F6FC2C, 0, capeId, 0);
        }

        #region skins
        private void AddSkinBtnClicked(object sender, EventArgs e)
        {
            var skinId = Convert.ToString(SkinIDBox.Value, CultureInfo.InvariantCulture);

            _savedData.Write(skinId, SkinNameBox.Text, "Skins");

            SkinList.Items.Add(SkinNameBox.Text + " | " + skinId);
        }

        private void DeleteSkinBtnClicked(object sender, EventArgs e)
        {
            _savedData.DeleteKey(GetIDFromList(SkinList.Text), "Skins");
            SkinList.Items.RemoveAt(SkinList.SelectedIndex);
        }

        private void SkinListChanged(object sender, EventArgs e)
        {
            MainForm.GeckoU.CallFunction(0x02F70028, 0, Convert.ToUInt32(GetIDFromList(SkinList.Text)), 0);
        }

        private void GetSkinIdBtnClicked(object sender, EventArgs e)
        {
            SkinIDBox.Value = MainForm.GeckoU.CallFunction(0x02F70178, 0);
        }
        #endregion

        #region skin loop
        private void LoopSkinsToggled(object sender, EventArgs e)
        {
            SkinLoopTimer.Enabled = LoopSkins.Checked;
        }

        private void SkinLooper(object sender, EventArgs e)
        {
            SkinList.SelectedIndex = new Random().Next(0, SkinList.Items.Count);
        }
        #endregion
    }
}