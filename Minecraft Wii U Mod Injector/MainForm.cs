﻿using MetroFramework.Controls;
using MetroFramework.Forms;
using MetroFramework;
using Minecraft_Wii_U_Mod_Injector.Helpers;
using Minecraft_Wii_U_Mod_Injector.Forms;
using Minecraft_Wii_U_Mod_Injector.Helpers.Files;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using Minecraft_Wii_U_Mod_Injector.Forms.Managers;
using Minecraft_Wii_U_Mod_Injector.Forms.Mods;
using Minecraft_Wii_U_Mod_Injector.Helpers.Win_Forms;
using Minecraft_Wii_U_Mod_Injector.Properties;
using Minecraft_Wii_U_Mod_Injector.Wii_U.Gecko_U;
using static System.Windows.Forms.DialogResult;

namespace Minecraft_Wii_U_Mod_Injector
{
    public partial class MainForm : MetroForm
    {
        #region base variables

        #region references

        public static GeckoUCore GeckoU;

        #endregion

        #region bools

        public bool Connected;

        public bool IsConnected
        {
            get => Connected;
            set
            {
                Connected = value;
                DiscordRp.SetPresence(Connected ? "Connected" : "Disconnected", MainTabs.SelectedTab.Text + " tab");
            }
        }

        public bool IsMinecraft()
        {
            string[] mcIds = new[] {"50000101d7500", "50000101d9d00", "50000101dbe00"};

            return mcIds.Contains(GeckoU.ReadTitleId());
        }

        private bool _tooHighWarn;
        private bool _invulEnWarn;

        #endregion

        #region assembly

        public static uint On = 0x38600001;
        public static uint Off = 0x38600000;
        public uint On2 = 0x3BE00001;
        public uint Off2 = 0x3BE00000;
        public static uint Blr = 0x4E800020;
        public uint Mflr = 0x7C0802A6;
        public uint Nop = 0x60000000;

        #endregion

        #region commands

        public static uint[] Enchantments =
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            16,
            17,
            18,
            19,
            20,
            21,
            32,
            33,
            34,
            35,
            48,
            49,
            50,
            51,
            61,
            62,
            70,
            71,
            80,
            81,
            82,
            83
        };

        #endregion

        #region pointers

        private uint _localPlayer;

        #endregion

        #endregion

        public MainForm()
        {
            InitializeComponent();
            MainTabs.ItemSize = new System.Drawing.Size(MainTabs.ItemSize.Width, 1);
        }

        private void Init(object sender, EventArgs e)
        {
            _ = new States(this);
            _ = new Messaging(this);
            _ = new Setup(this);
            _ = new Miscellaneous(this);

            Setup.SetupInjector();
        }


        private void Exit(object sender, FormClosingEventArgs e)
        {
            DiscordRp.Deinitialize();
            Settings.Default.TabIndex = MainTabs.SelectedIndex;
            Settings.Default.Save();
        }

        private void SwapTab(object sender, EventArgs e)
        {
            var tile = (MetroTile) sender;

            if ((string)tile.Tag == "MgTile")
            {
                if (MinigamesTabs.SelectedIndex != tile.TileCount)
                    MinigamesTabs.SelectedIndex = tile.TileCount;
                else return;

                return;
            }

            if (MainTabs.SelectedIndex != tile.TileCount)
                MainTabs.SelectedIndex = tile.TileCount;
            else
                return;

            DiscordRp.SetPresence(IsConnected ? "Connected" : "Disconnected", MainTabs.SelectedTab.Text + " tab");
        }

        private void SliderClicked(object sender, EventArgs e)
        {
            var slider = (NumericUpDown) sender;

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (slider.DecimalPlaces != 10)
                    slider.DecimalPlaces++;
            }
            else if (ModifierKeys.HasFlag(Keys.Alt))
            {
                slider.DecimalPlaces--;
            }
        }

        private async Task PutTaskDelay(int sleep)
        {
            await Task.Delay(sleep);
        }

        private void ConnectBtnClicked(object sender, EventArgs e)
        {
            try
            {
                GeckoU ??= new GeckoUCore(WiiUIPv4Box.Text);

                switch (States.CurrentState)
                {
                    case States.StatesIds.Disconnected:
                        States.SwapState(States.StatesIds.Connecting);
                        GeckoU.Tcp.Connect();

                        if (!IsMinecraft())
                        {
                            Messaging.Show(
                                "This Mod Injector is intended to be used with Minecraft: Wii U Edition, please launch Minecraft and try again.");
                            GeckoU.Tcp.Close();
                            States.SwapState(States.StatesIds.Disconnected);
                            break;
                        }

                        States.SwapState(States.StatesIds.Connected);
                        IsConnected = true;
                        break;

                    case States.StatesIds.Connected:
                        GeckoU.Tcp.Close();
                        IsConnected = false;
                        States.SwapState(States.StatesIds.Disconnected);
                        break;
                }
            }
            catch (SocketException)
            {
                Messaging.Show(MessageBoxIcon.Error,
                    "Couldn't detect TCPGecko running or IP Address is wrong.\nMake sure you have TCPGecko running and entered the correct IP Address for you Wii U");
                States.SwapState(States.StatesIds.Disconnected);
            }
            catch (IOException)
            {
                Messaging.Show(MessageBoxIcon.Error,
                    "Couldn't detect TCPGecko running or IP Address is wrong.\nMake sure you have TCPGecko running and entered the correct IP Address for you Wii U");
                States.SwapState(States.StatesIds.Disconnected);
            }
            catch (Exception err)
            {
                Exceptions.LogError(err, "Unable to Connect to the Wii U, unknown error", true);
                States.SwapState(States.StatesIds.Disconnected);
            }
        }

        private void CaptureWiiUiPv4Box(object sender, EventArgs e)
        {
            switch (StringUtils.ValidateIPv4(WiiUIPv4Box.Text))
            {
                case true:
                    WiiUIPv4Box.Style = MetroColorStyle.Green;
                    ConnectBtn.Enabled = true;
                    break;
                case false:
                    WiiUIPv4Box.Style = MetroColorStyle.Red;
                    ConnectBtn.Enabled = false;
                    break;
            }
        }

        private void CaptureWiiUiPv4BoxInput(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ConnectBtnClicked(null, null);
        }

        private void FormColorSelected(object sender, EventArgs e)
        {
            try
            {
                Style = (MetroColorStyle) Enum.Parse(typeof(MetroColorStyle), ColorsBox.Text);
                StyleMngr.Style = (MetroColorStyle) Enum.Parse(typeof(MetroColorStyle), ColorsBox.Text);
                Refresh();
                Settings.Default.Style = Style;
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Failed to Change Injector Form Color", true);
            }
        }

        private void FormTxtAlgnSelected(object sender, EventArgs e)
        {
            try
            {
                TextAlign = (MetroFormTextAlign)Enum.Parse(typeof(MetroFormTextAlign), TextAllignBox.Text);
                Refresh();
                Settings.Default.FormTxtAlign = TextAlign;
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Failed to Change Injector Text Align", true);
            }
        }

        private void DiscordRpcToggleChecked(object sender, EventArgs e)
        {
            try
            {
                if (discordRpcCheckBox.Checked)
                {
                    DiscordRp.Initialize();
                    DiscordRp.SetPresence(IsConnected ? "Connected" : "Disconnected",
                        MainTabs.SelectedTab.Text + " tab");
                }
                else
                {
                    DiscordRp.Deinitialize();
                }

                Settings.Default.DiscordRPC = discordRpcCheckBox.Checked;
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Failed to Toggle Discord RPC", true);
            }
        }

        private async void CheckUpdatesClicked(object sender, EventArgs e)
        {
            updateBtn.Enabled = false;
            updateBtn.Text = @"Checking for Updates...";
            await Setup.RetrieveGitVersion(false);
            updateBtn.Text = @"Check for Updates";
            updateBtn.Enabled = true;
        }

        private void ReleaseNotesToggleClicked(object sender, EventArgs e)
        {
            try
            {
                if (releaseNotesToggle.Checked)
                {
                    BuildNotesBox.Text = Resources.releaseNote;
                    Settings.Default.ReleaseNotes = "Current";
                }
                else
                {
                    BuildNotesBox.Text = Resources.releaseNotes;
                    Settings.Default.ReleaseNotes = "All";
                }
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Failed to Toggle Release Notes", true);
            }
        }

        private void CheckForPreReleaseToggled(object sender, EventArgs e)
        {
            try
            {
                Settings.Default.PrereleaseOptIn = CheckForPreRelease.Checked;
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Failed to Toggle Release Notes", true);
            }
        }

        private void HostIndicatorsToggled(object sender, EventArgs e)
        {
            try
            {
                if (!Settings.Default.HostIndicators)
                    Messaging.Show(
                        "All mods will now be highlighted with a blue and orange color\n\nOrange: Host Only mod\nBlue: Non-Host mod");

                Settings.Default.HostIndicators = HostIndicators.Checked;
                Miscellaneous.DoHostIndicators(HostIndicators.Checked);
                Refresh();
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Failed to Toggle Host Indicators", true);
            }
        }

        private void OpenLangMngrBtnClicked(object sender, EventArgs e)
        {
            new LanguageMngr(this).ShowDialog();
        }

        private void QuickModsManagerBtnClicked(object sender, EventArgs e)
        {
            new QuickModsMngr(this).ShowDialog();
        }

        private void CemuPckMngrBtnClicked(object sender, EventArgs e)
        {
            new CemuPckMngr(this).ShowDialog();
        }

        private void OpenFaqInfoClicked(object sender, EventArgs e)
        {
            new Faq(this).ShowDialog();
        }

        private void SetupTutorialBtnClicked(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/watch?v=be5fNSgxhrU");
        }

        private void DiscordSrvBtnClicked(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/jrzZWaDc7a");
        }

        private void CreditsBtnClicked(object sender, EventArgs e)
        {
            new Credits(this).ShowDialog();
        }

        private void MapTextEditorClicked(object sender, EventArgs e)
        {
            new MapTextEditor(this).ShowDialog();
        }

        private void NnidEditorBtnClicked(object sender, EventArgs e)
        {
            new NnidEditor(this).ShowDialog();
        }

        private void PlayerOptionsBtnClicked(object sender, EventArgs e)
        {
            if (IsPointerLoaded())
                new PlayerOptions(this).Show();
            else
                Messaging.Show("Player not found, please load into a world before using Player Options.");
        }

        private void LootTableEditorBtnClicked(object sender, EventArgs e)
        {
            new LootTableEditor(this).ShowDialog();
        }

        private void WorldGenerationOptsBtnClicked(object sender, EventArgs e)
        {
            new WorldGenerationEditor(this).ShowDialog(this);
        }

        private void EntityEditorBtnClicked(object sender, EventArgs e)
        {
            new EntityEditor(this).ShowDialog();
        }

        private void EnchantmentEditorBtnClicked(object sender, EventArgs e)
        {
            new EnchantmentEditor(this).ShowDialog();
        }

        private void AchievementEditorClicked(object sender, EventArgs e)
        {
            new AchievementEditor(this).ShowDialog();
        }

        private void DLCManagerBtn_Click(object sender, EventArgs e)
        {
            new DLCManager(this).ShowDialog();
        }

        private void ItemIdHelpBtnClicked(object sender, EventArgs e)
        {
            Messaging.Show(MessageBoxIcon.Information,
                "Item IDs can be found at https://minecraft-ids.grahamedgecombe.com/ \nData Values are the numbers behind the : in the ID.\nFor example, if you want Birch Wood the ID would be 17 and the data value would be 2");
        }

        private void IncompatibilityCheck(object sender, EventArgs e)
        {
            javaTellBox.Checked = !bedrockTellBox.Checked && javaTellBox.Checked;
            bedrockTellBox.Checked = !javaTellBox.Checked && bedrockTellBox.Checked;
        }

        #region memory editing

        public static bool IsPointerLoaded()
        {
            return GeckoU.PeekUInt(GeckoU.PeekUInt(0x10A0A624) + 0x9C) != 0x0;
        }

        private void SwimFastToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x1066879C, 0x3DA00000, 0x3CA3D70A, SwimFast.Checked);
        }

        private void BreakBedrockToggled(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Please load a world before toggling this mod");
                BreakBedrock.Checked = false;
                return;
            }

            var pointer = GeckoU.PeekUInt(GeckoU.PeekUInt(0x109C3720) + 0x3F8);
            GeckoU.WriteULongToggle(pointer + 0x40, 0x3E88888900000000, 0x7F7FFFFF4B895440, BreakBedrock.Checked);
        }

        private void InsaneCriticalHitsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x106D0D4C, 0x43F00000, 0x3FC00000, InsaneCriticalHits.Checked);
        }

        private void AlwaysSwimmingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteULongToggle(0x023405EC, 0x386000014E800020, 0x3D800233398C69B8,
                AlwaysSwimming.Checked); //Entity::isSwimming((void))
        }

        private void InfiniteRiptideToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0232C210, On, Off, InfiniteRiptide.Checked); //Entity::isInWaterOrRain((void))
        }

        private void FullRotationToggled(object sender, EventArgs e)
        {
            GeckoU.WriteULongToggle(0x105DDAF8, 0xC3B4000043B40000, 0xC2B4000042B40000, FullRotation.Checked);
        }

        private void AlwaysDamagedPlayersToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x027206CC, On, 0x57E3063E, AlwaysDamagedPlayers.Checked); //Player::isInWall((void))
        }

        private void InfiniteItemsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteLongToggle(0x024872AC, 0x600000004E800020, 0x3D800248398C7294,
                InfiniteItems.Checked); //ItemInstance::shrink((int))
        }

        private void RapidBowToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02162F04, 0x3BE00001, 0x3BE00014,
                RapidBow.Checked); //GetMaxBowDuration__7BowItemSFv
        }

        private void BloodVisionToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0257E000, Off, 0x57E3063E, BloodVision.Checked); //LivingEntity::isAlive((void))
        }

        private void IgnorePotionsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0258E4BC, Off, On,
                IgnorePotions.Checked); //LivingEntity::isAffectedByPotions((void))
        }

        private void ForeverLastingPotionsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02576994, Blr, 0x9421FF40,
                ForeverLastingPotions.Checked); //LivingEntity::tickEffects((void))
        }

        private void BypassInvulnerabilityToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0225458C, On, 0x88630009,
                BypassInvulnerability.Checked); //DamageSource::isBypassInvul(const(void))
        }

        private void PlaceBlocksonHeadToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0207F604, On, 0x7FC3F378,
                PlaceBlocksonHead.Checked); //ArmorSlot::mayPlace((not_null_ptr__tm__15_12ItemInstance))
        }

        private void WalkonWaterToggled(object sender, EventArgs e)
        {
            GeckoU.WriteULongToggle(0x025A963C, 0x3C60109E80639948, 0x3C6010568063B61C,
                WalkonWater.Checked); //LiquidBlock::getClipAABB((BlockState const *, LevelSource *, BlockPos const &))
        }

        private void AlwaysElytraToggled(object sender, EventArgs e)
        {
            GeckoU.WriteULongToggle(0x0258DACC, 0x386000014E800020, 0x3D800233398C69B8,
                AlwaysElytra.Checked); //LivingEntity::isFallFlying((void))
        }

        private void CaveFinderToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x030E4924, 0x38800000, 0x38800001,
                CaveFinder.Checked); //enableState__14GlStateManagerSFi
        }

        private void WallhackToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x031B2B4C, On, 0x5403063E, Wallhack.Checked); //renderDebug__9MinecraftSFv
        }

        private void LargeXpDropsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0239A26C, Nop, 0x4180000C,
                LargeXPDrops.Checked); //getExperienceValue__13ExperienceOrbSFi:
            GeckoU.WriteUIntToggle(0x0239A270, 0x38607FFF, 0x386009AD, LargeXPDrops.Checked);
        }

        private void WallClimbingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0257DF90, On, 0x57E3063E, WallClimbing.Checked); //LivingEntity::onLadder((void))
        }

        private void NoCollisionToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x020E7BF4, Blr, 0x9421FFE0,
                NoCollision.Checked); //Block::addCollisionAABBs((BlockState const *, Level *, BlockPos const &, AABB const *, std::vector__tm__39_P4AABBQ2_3std23allocator__tm__7_P4AABB *, boost::shared_ptr__tm__8_6Entity, bool))
        }

        private void InfiniteAirToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02572AA4, Blr, Mflr, InfiniteAir.Checked); //LivingEntity::decreaseAirSupply((int))
        }

        private void InfiniteDurabilityToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0248909C, Nop, 0x4181001C,
                InfiniteDurability.Checked); //ItemInstance::hurt((int,Random *)) (bgt)
            GeckoU.WriteUIntToggle(0x024889CC, Off, On,
                InfiniteDurability.Checked); //ItemInstance::isDamageableItem((void))
        }

        private void SuperKnockbackToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0257D980, 0xFC00E838, 0xFC006378,
                SuperKnockback.Checked); //Knockback__12LivingEntityFQ2_5boost25shared_ptr__tm__8_6EntityfdT3
            GeckoU.WriteUIntToggle(0x0257D990, 0xFD20B890, 0xFD290372, SuperKnockback.Checked);
        }

        private void DisabledKnockbackToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0257D85C, Blr, 0x9421FFA8,
                DisabledKnockback.Checked); //Knockback__12LivingEntityFQ2_5boost25shared_ptr__tm__8_6EntityfdT3
        }

        private void SilkTouchAnythingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x020EA77C, On, 0x57C3063E,
                SilkTouchAnything.Checked); //Block::isSilkTouchable((void))
        }

        private void DeveloperModeToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02F5C874, On, Off, DeveloperMode.Checked); //CMinecraftApp::DebugArtToolsOn((void))
        }

        private void PickLiquidBlocksToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x025A98504, On, 0x57C3063E,
                PickLiquidBlocks.Checked); //LiquidBlock::mayPick((BlockState const *, bool))
        }

        private void DuelWieldanyItemToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x024FD7F4, On, Off,
                DuelWieldanyItem
                    .Checked); //_12ItemInstance::mayPlace__Q2_13InventoryMenu11OffhandSlotF35not_null_ptr__tm(void)
        }

        private void DisableStarvingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0271BA6C, On, 0x7FE3FB78,
                DisableStarving.Checked); //Player::isAllowedToIgnoreExhaustion((void))
        }

        private void InstantMiningToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x020E75C8, 0xC0230000, 0xC0230040,
                InstantMining.Checked); //Block::getDestroySpeed((BlockState const *, Level *, BlockPos const &))
        }

        private void FlyingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0271AA74, On, 0x7FE3FB78, Flying.Checked); //Player::isAllowedToFly((void))
        }

        private void DisableSuffocatingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x027206CC, Off, 0x57E3063E, DisableSuffocating.Checked); //Player::isInWall((void))
        }

        private void NoFallDamageToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02723540, Blr, 0x9421FFB0, NoFallDamage.Checked); //causeFallDamage__6PlayerFfT1
        }

        private void CraftAnythingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02F70970, On, Off,
                CraftAnything.Checked); //IsDebugSettingEnabled__12GameSettingsSF13eDebugSettingi
            GeckoU.WriteUIntToggle(0x032283CC, 0x38800000, 0x38800001,
                CraftAnything.Checked); //run__15MinecraftServerFLPv
        }

        private void CreativeModeToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02456F4C, On, 0x5403063E,
                CreativeMode.Checked); //GameType::isCreative(const(void))
        }

        private void NoFogToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x030E4954, 0x38800000, 0x38800001, NoFog.Checked);
        }

        private void StaticLiquidBlocksToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x022B9B8C, Blr, 0x9421FF00,
                StaticLiquidBlocks
                    .Checked); //DynamicLiquidBlock::maintick((Level *, BlockPos const &, BlockState const *, Random *))
        }

        private void FoggyWeatherToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x031C06A4, On, Off, FoggyWeather.Checked); //isInCloud__13LevelRendererFdN21f
        }

        private void AcidLiquidBlocksToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x022B8EC4, On, 0x57E3063E,
                AcidLiquidBlocks
                    .Checked); //DynamicLiquidBlock::canSpreadTo((Level *, BlockPos const &, BlockState const *))
        }

        private void DisableCreativeFlagToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x025C2B8C, On, 0x886300A9,
                DisableCreativeFlag.Checked); // LevelData::getHasBeenInCreative((void))
        }

        private void UncapEntitySpawnLimitToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0256CDFC, On, 0x5403063E,
                UncapEntitySpawnLimit.Checked); //Level::canCreateMore((eINSTANCEOF,Level::ESPAWN_TYPE))
        }

        private void AllDlcUnlockedToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02AE8B30, On, 0x7FE3FB78,
                AllDLCUnlocked.Checked); //DLCManager::IsDlcPackTemporarilyFree((int))
            GeckoU.WriteUIntToggle(0x02AEF828, On, 0x7FE3FB78,
                AllDLCUnlocked.Checked); //DLCManager::IsMiniGameTemporarilyFree((int))
        }

        private void HostOptionsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02F17B30, 0x38807D00, 0x388303F4,
                HostOptions.Checked); //CMinecraftApp::SetGameHostOption((eGameHostOption, unsigned int))
        }

        private void DisablePermanentKicksToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0340D428, Blr, Mflr,
                DisablePermanentKicks.Checked); //CProfile::QuadrantOfflineXUID((int))
        }

        private void EnchantmentLevelSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteUInt(0x022F4900, GeckoU.Mix(0x38600000, EnchantmentLevelSlider.Value)); //getenchentmentslevel
        }

        private void JumpHeightSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x1066AAD4, (float) JumpHeightSlider.Value);
        }

        private void ReachSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x108F5620, Convert.ToSingle(ReachSlider.Value));
            GeckoU.WriteFloat(0x108E0BDC, Convert.ToSingle(ReachSlider.Value));
        }

        private void RiptideFlyingSpeedSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x1079C3E0, (float) RiptideFlyingSpeedSlider.Value);
        }

        private void SprintingSpeedScaleSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x10692528, (float) SprintingSpeedScaleSlider.Value);
        }

        private void VisibleHitboxesToggled(object sender, EventArgs e)
        {
            uint whiteBoxAddress = GeckoU.PeekUInt(GeckoU.PeekUInt(0x10A72A94) + 0xC0);
            GeckoU.WriteUIntToggle(whiteBoxAddress + 0x90, 0x0001F000, 0x000100000, VisibleHitboxes.Checked);
        }

        private void ExitGameClicked(object sender, EventArgs e)
        {
            DialogResult confirmCg =
                Messaging.Show("You're about to close the game, continue?", MessageBoxButtons.YesNo);

            if (confirmCg == Yes)
                GeckoU.CallFunction64(0x02F50028, 0x00000000); //CConsoleMinecraftApp::ExitGame((void))
        }

        private void BypassFriendsOnlyToggled(object sender, EventArgs e)
        {
            GeckoU.WriteULongToggle(0x02D5731C, 0x386000014E800020, 0x7C0802A69421FFF0,
                BypassFriendsOnly.Checked); //CGameNetworkManager::IsInPublicJoinableGame((void))
        }

        private void WalkingSpeedScaleChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x1066ACC8, (float) WalkingSpeedScaleSlider.Value);
        }

        private void CraftingTableAnywhereToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02F59534, 0x480002E8, 0x7C0802A6, CraftingTableAnywhere.Checked);
        }

        private void HiddenGameModesUnlockedToggled(object sender, EventArgs e)
        {
            var pointer = GeckoU.PeekUInt(GeckoU.PeekUInt(0x109C4CEC) + 0x4C);
            GeckoU.WriteUIntToggle(pointer, 0x5, 0x2, HiddenGameModesUnlocked.Checked);
        }

        private void FieldOfViewSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x108911B8, (float) FieldOfViewSlider.Value);
        }

        private void TakeEverythingAnywhereToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02DEC0B4, On, 0x57E3063E, TakeEverythingAnywhere.Checked);
        }

        private void ArmorHudToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02E9B1B0, 0x7FE4FB78, 0x7FC4F378, ArmorHUD.Checked);
        }

        private void SlowMotionToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x109473C8, 0x3F700000, 0x3FF00000, SlowMotion.Checked);
        }

        private void DeadMauFiveModeToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02F5D53C, On, Off, DeadMauFiveMode.Checked);
        }

        private void PlayerModelScaleSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x1091B90C, Convert.ToSingle(playerModelScaleSlider.Value));
        }

        private void GamepadSplitscreenToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02F6EFFC, Off, 0x8863072A,
                GamepadSplitscreen.Checked); //CConsoleMinecraftApp::IsDrcPreventingSplitscreen((void))
        }

        private void DisableTeleportingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0336367C, Blr, 0x9421FED0, DisableTeleporting.Checked); //TeleportCommand::execute
        }

        private void GodModeToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02727824, Nop, 0xFC20F890, GodMode.Checked); //Player::getAbsorptionAmount((void))
        }

        private void GodModeAllToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x025795C0, Nop, 0xFC20F890, GodMode.Checked); //LivingEntity::setHealth((float))
        }

        private void FieldOfViewSplitSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x108C7870, (float) FieldOfViewSplitSlider.Value);
        }

        private void FrictionSliderChanged(object sender, EventArgs e)
        {
            if (!_tooHighWarn && FrictionSlider.Value > 2 || !_tooHighWarn && FrictionSlider.Value < -2)
            {
                Messaging.Show(MessageBoxIcon.Exclamation,
                    "Setting the friction too high (aswell as too negative) can crash your game, be warned.");
                _tooHighWarn = true;
            }

            GeckoU.WriteFloat(0x1066AAE8, (float) FrictionSlider.Value);
        }

        private void UnderwaterEffectsToggled(object sender, EventArgs e)
        {
            switch (UnderwaterEffects.CheckState)
            {
                case CheckState.Unchecked:
                    GeckoU.WriteUInt(0x0253C3CC, 0x88630011);
                    UnderwaterEffects.Text = @"Underwater Effects (Default)";
                    break;

                case CheckState.Checked:
                    GeckoU.WriteUInt(0x0253C3CC, On);
                    UnderwaterEffects.Text = @"Underwater Effects (On)";
                    break;

                case CheckState.Indeterminate:
                    GeckoU.WriteUInt(0x0253C3CC, Off);
                    UnderwaterEffects.Text = @"Underwater Effects (Off)";
                    break;
            }
        }

        private void HideBlocksToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02048160, On, Off, HideBlocks.Checked);
        }

        private void AlwaysInLavaToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0255F818, 0x38000001, 0x38000000, AlwaysInLava.Checked);
        }

        private void SeeThroughBlocksToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x020E751C, 0x3BE00000, 0x3BE00001, SeeThroughBlocks.Checked);
        }

        private void LevelXToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02B47934, 0x2C1D0001, 0x2C1D0000, LevelX.Checked);
        }

        private void AlwaysInWaterToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0255E81C, 0x38000001, 0x38000000, AlwaysInWater.Checked);
        }

        private void EspToggled(object sender, EventArgs e)
        {
            if (!Wallhack.Checked)
                GeckoU.WriteUIntToggle(0x031B2B4C, 0x38000001, 0x6CC08000, ESP.Checked);

            GeckoU.WriteUIntToggle(0x030EA178, 0xFC80E890, 0xFC60E890, ESP.Checked);
            GeckoU.WriteUIntToggle(0x030EA0E8, 0xFC80F890, 0xFC60F890, ESP.Checked);
            GeckoU.WriteUIntToggle(0x030EA0E4, 0xFC80F890, 0xFC40F890, ESP.Checked);
            GeckoU.WriteUIntToggle(0x030EA1C8, 0x6CC00000, 0x6CC08000, ESP.Checked);
        }

        private void NoSlowDownsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x108E0E60, 0x3F800000, 0x3E4CCCCD, NoSlowDowns.Checked);
        }

        private void UiColorPickerBtnClicked(object sender, EventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog {AllowFullOpen = true, AnyColor = true};

            if (colorDlg.ShowDialog() == OK)
            {
                GeckoU.WriteFloat(0x109C8E68, colorDlg.Color.R / 255.0F);
                GeckoU.WriteFloat(0x109C8E6C, colorDlg.Color.G / 255.0F);
                GeckoU.WriteFloat(0x109C8E70, colorDlg.Color.B / 255.0F);
                GeckoU.WriteFloat(0x109C8E74, colorDlg.Color.A / 255.0F);
            }
        }

        private void SuperchargedCreepersToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02275934, On, 0x8869000C, SuperchargedCreepers.Checked);
        }

        private void IgnitedCreepersToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02273030, On, 0x8869000C, IgnitedCreepers.Checked);
        }

        private void ZombieTowerToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02A3B77C, On, 0x88630740, ZombieTower.Checked);
        }

        private void SunProofMobsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02A3D808, Off, On, SunProofMobs.Checked);
        }

        private void PotionAmplifierSliderChanged(object sender, EventArgs e)
        {
            if (PotionAmplifierSlider.Value == 0)
            {
                GeckoU.WriteUInt(0x02692DF0, 0x80630008);
                return;
            }

            GeckoU.WriteUInt(0x02692DF0, GeckoU.Mix(0x38600000, PotionAmplifierSlider.Value));
        }

        private void XpLevelSliderChanged(object sender, EventArgs e)
        {
            var level = (uint) XPLevelSlider.Value;
            _localPlayer = GeckoU.PeekUInt(GeckoU.PeekUInt(0x10A0A648) + 0x2C);

            GeckoU.CallFunction(0x031EA12C, _localPlayer, level, level, level);
        }

        private void DisableVPadInputToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x011293D0, Blr, 0x38E00001, disableVPadInput.Checked);
        }

        private void VpadDisplaySwitchToggled(object sender, EventArgs e)
        {
            GeckoU.RpcToggle(0x0112A9E4, 0x0112A9EC, 0x0, 0x0, vpadDisplaySwitch.Checked);
        }

        private async void DebugConsoleToggled(object sender, EventArgs e)
        {
            uint[] bools = {0x01, 0x00};

            GeckoU.RpcToggle(0x02DABA0C, 0x109F95E0, bools, DebugConsole.Checked, 0, 1);

            DebugConsole.Enabled = false;
            await PutTaskDelay(1000);
            DebugConsole.Enabled = true;
        }

        private void UnlockSignKeyboardToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02F88110, 0x39400002, 0x39400003, UnlockSignKeyboard.Checked);
            GeckoU.WriteUIntToggle(0x02FAF4F0, 0x39400002, 0x39400003, UnlockSignKeyboard.Checked);
            GeckoU.WriteUIntToggle(0x02FAF560, 0x39400002, 0x39400003, UnlockSignKeyboard.Checked);
            GeckoU.WriteUIntToggle(0x02FAF5DC, 0x39400002, 0x39400003, UnlockSignKeyboard.Checked);
            GeckoU.WriteUIntToggle(0x02FAF64C, 0x39400002, 0x39400003, UnlockSignKeyboard.Checked);
        }

        private void InvisibleToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x031E2F40, On, 0x57C3063E, Invisible.Checked);
        }

        private void LeashAnyMobsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02078964, On, Off, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x02078964, On, Off, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x022D2FFC, On, Off, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x0234A7B8, On, Off, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x023CE0B8, On, Off, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x02651494, On, 0x7FC3F378, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x029D1BB4, On, 0x7FC3F378, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x02A59DF4, On, Off, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x02A7BBE4, On, Off, LeashAnyMobs.Checked);
            GeckoU.WriteUIntToggle(0x02A884E0, On, 0x57E3063E, LeashAnyMobs.Checked);
        }

        private void WaterLogAnythingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x020E7624, On, 0x5463063E, WaterLogAnything.Checked);
        }

        private void UnlimitedEnchantsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0249602C, On, Off, UnlimitedEnchants.Checked);
        }

        private void SuperFurnaceToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x023F66A4, On, 0x386000C8, SuperFurnace.Checked); //FurnaceBlockEntity::getTotalCookTime((not_null_ptr__tm__15_12ItemInstance))
        }

        private void MaxStackSliderChanged(object sender, EventArgs e)
        {
            uint[] stackFunctions =
            {
                0x0207F510, 0x02009030, 0x02081B94, 0x020BF710, 0x0218FAB8, 0x0218F9B0, 0x0218F16C, 0x021B2044,
                0x021B3A18, 0x02233114, 0x02295380,
                0x0228DB84, 0x023E4878, 0x023F6B10, 0x0245605C, 0x0247A1B8, 0x0252F280, 0x024A100C, 0x024CFFC4,
                0x0262AFA4, 0x02618CF4, 0x0284A694,
                0x02878850, 0x028DD624
            };

            GeckoU.WriteUInt(stackFunctions, GeckoU.Mix(0x38600000, MaxStackSlider.Value));
        }

        private void LeftHandedToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02728620, Off, 0x80630088, LeftHanded.Checked);
        }

        private void UnlimitedJumpsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0232F3A0, 0x38800001, 0x38800000, UnlimitedJumps.Checked);
        }

        private void EntitySpeedSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(GeckoU.PeekUInt(GeckoU.PeekUInt(0x109CD8E4) + 0x20), (float)EntitySpeedSlider.Value);
        }

        private void SleepingDoesntClearWeatherToggled(object sender, EventArgs e)
        {
            GeckoU.WriteULongToggle(0x032AE7A4, 0x6000000060000000, 0x3D80032B398CE608,
                SleepingDoesntClearWeather.Checked);
        }

        private void InvulnerableEntitiesToggled(object sender, EventArgs e)
        {
            if (!_invulEnWarn)
            {
                var warn = Messaging.Show(
                    "Enabling this option can cause significant frame drops and potentially crash your game.\nAre you sure you want to enable?",
                    MessageBoxButtons.YesNo);
                _invulEnWarn = true;

                if (warn == No)
                {
                    InvulnerableEntities.Checked = false;
                    return;
                }
            }

            GeckoU.WriteUIntToggle(0x0232B540, 0x39600000, 0x39600001, InvulnerableEntities.Checked);
        }

        private void BreakAnythingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x03197340, On, 0x5403063E, BreakAnything.Checked);
        }

        private void LiquidSpreadTimeSliderChanged(object sender, EventArgs e)
        {
            uint[] liquidAdrs = { 0x025AAB54, 0x025AAB9C, 0x025AABA8 };

            GeckoU.WriteUInt(liquidAdrs, GeckoU.Mix(0x38600000, LiquidSpreadTimeSlider.Value));
        }

        private void DisableFallingBlocksToggled(object sender, EventArgs e)
        {
            GeckoU.WriteULongToggle(0x0235C624, 0x600000004E800020, 0x3D800236398CBDD8, DisableFallingBlocks.Checked);
        }

        private void BreatheBoxChanged(object sender, EventArgs e)
        {
            switch (breatheBox.SelectedIndex)
            {
                case 0:
                    GeckoU.WriteUInt(0x0256FDA4, On); //Air
                    GeckoU.WriteUInt(0x0256FDAC, Off); //Lava
                    GeckoU.WriteUInt(0x0256FDB4, Off); //Blocks
                    GeckoU.WriteUInt(0x0256FD9C, Off); //Water
                    break;

                case 1:
                    GeckoU.WriteUInt(0x0256FDA4, Off); //Air
                    GeckoU.WriteUInt(0x0256FDAC, On); //Lava
                    GeckoU.WriteUInt(0x0256FDB4, Off); //Blocks
                    GeckoU.WriteUInt(0x0256FD9C, Off); //Water
                    break;

                case 2:
                    GeckoU.WriteUInt(0x0256FDA4, Off); //Air
                    GeckoU.WriteUInt(0x0256FDAC, Off); //Lava
                    GeckoU.WriteUInt(0x0256FDB4, On); //Blocks
                    GeckoU.WriteUInt(0x0256FD9C, Off); //Water
                    break;

                case 3:
                    GeckoU.WriteUInt(0x0256FDA4, Off); //Air
                    GeckoU.WriteUInt(0x0256FDAC, Off); //Lava
                    GeckoU.WriteUInt(0x0256FDB4, Off); //Blocks
                    GeckoU.WriteUInt(0x0256FD9C, On); //Water
                    break;

                case 4:
                    GeckoU.WriteUInt(0x0256FDA4, On); //Air
                    GeckoU.WriteUInt(0x0256FDAC, On); //Lava
                    GeckoU.WriteUInt(0x0256FDB4, On); //Blocks
                    GeckoU.WriteUInt(0x0256FD9C, On); //Water
                    break;

                case 5:
                    GeckoU.WriteUInt(0x0256FDA4, Off); //Air
                    GeckoU.WriteUInt(0x0256FDAC, Off); //Lava
                    GeckoU.WriteUInt(0x0256FDB4, Off); //Blocks
                    GeckoU.WriteUInt(0x0256FD9C, Off); //Water
                    break;
            }
        }

        private void MuteMicrophoneToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x10997EA8, 0x30000000, 0x3F000000, MuteMicrophone.Checked);
        }

        private void CollisionToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0258BA4C, 0x3BC00000, 0x3BC00001, Collision.Checked);
        }

        private void WaterDownStrengthSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteFloat(0x1066AAE0, (float)WaterDownStrengthSlider.Value);
        }

        private void RainModeToggled(object sender, EventArgs e)
        {
            switch (RainMode.CheckState)
            {
                case CheckState.Unchecked:
                    GeckoU.WriteUInt(0x025C2AC4, 0x8863006C);
                    RainMode.Text = @"Rain Mode (Default)";
                    break;

                case CheckState.Checked:
                    GeckoU.WriteUInt(0x025C2AC4, On);
                    RainMode.Text = @"Rain Mode (Always)";
                    break;

                case CheckState.Indeterminate:
                    GeckoU.WriteUInt(0x025C2AC4, Off);
                    RainMode.Text = @"Rain Mode (Never)";
                    break;
            }
        }

        private void ThunderModeToggled(object sender, EventArgs e)
        {
            switch (ThunderMode.CheckState)
            {
                case CheckState.Unchecked:
                    GeckoU.WriteUInt(0x025C2AA4, 0x88630074);
                    RainMode.CheckState = CheckState.Unchecked;
                    ThunderMode.Text = @"Thunder Mode (Default)";
                    break;

                case CheckState.Checked:
                    GeckoU.WriteUInt(0x025C2AA4, On);
                    RainMode.CheckState = CheckState.Checked;
                    ThunderMode.Text = @"Thunder Mode (Always)";
                    break;

                case CheckState.Indeterminate:
                    GeckoU.WriteUInt(0x025C2AA4, Off);
                    RainMode.CheckState = CheckState.Indeterminate;
                    ThunderMode.Text = @"Thunder Mode (Never)";
                    break;
            }
        }

        private void AchievementsEverywhereToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02F66274, 0x3B800001, 0x3B800000, AchievementsEverywhere.Checked);
        }

        private void NoDamageToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0271AA90, Blr, 0x9421FF90, NoDamage.Checked);
        }

        private void FreezeGameWhenPausedToggled(object sender, EventArgs e)
        {
            switch (FreezeGameWhenPaused.CheckState)
            {
                case CheckState.Unchecked:
                    GeckoU.WriteUInt(0x02F37D98, 0x5684063E);
                    FreezeGameWhenPaused.Text = @"Freeze game when paused (Default)";
                    break;

                case CheckState.Checked:
                    GeckoU.WriteUInt(0x02F37D98, 0x38800001);
                    FreezeGameWhenPaused.Text = @"Freeze game when paused (Always Frozen)";
                    break;

                case CheckState.Indeterminate:
                    GeckoU.WriteUInt(0x02F37D98, 0x38800000);
                    FreezeGameWhenPaused.Text = @"Freeze game when paused (Never)";
                    break;
            }
        }

        private void SplashLingeringPotionsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x029916A4, On, Off, SplashLingeringPotions.Checked);
        }

        private void ItemOfUndyingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0257B984, On, 0x7C035840, ItemOfUndying.Checked);
        }

        private void UnlimitedTotemsOfUndyingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0257BB60, 0x38800000, 0x38800001, UnlimitedTotemsOfUndying.Checked);
        }

        private void PunchToRideToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x0271D184, 0x48AC5738, 0x9421FD90, PunchToRide.Checked);
        }

        private void CursedToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x108E021C, 0x40000000, 0x3F800000, Cursed.Checked);
        }

        #region commands

        private void GiveCommandBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            if (string.IsNullOrWhiteSpace(giveIDBox.Text) || string.IsNullOrWhiteSpace(giveAmountBox.Text) ||
                string.IsNullOrWhiteSpace(giveDataBox.Text))
            {
                Messaging.Show("Make sure all fields are filled out before executing a command");
                return;
            }

            if (Convert.ToBoolean(giveIDBox.Tag))
            {
                if (Convert.ToInt32(giveDataBox.Text) < -32768 || Convert.ToInt32(giveDataBox.Text) > 32767)
                {
                    Messaging.Show("Data value must be in between -32768 and 32767.");
                    return;
                }
            }
            else
            {
                if (Convert.ToInt32(giveDataBox.Text) < 0 || Convert.ToInt32(giveDataBox.Text) > 15)
                {
                    Messaging.Show("Data value must be in between 0 and 15.");
                    return;
                }
            }

            GeckoU.WriteUInt(0x10303008, uint.Parse(giveIDBox.Text));
            GeckoU.WriteUInt(0x1030300C, uint.Parse(giveAmountBox.Text));
            GeckoU.WriteUInt(0x10303010, (uint) int.Parse(giveDataBox.Text));
            GeckoU.WriteBytes(0x03B00000,
                new byte[]
                {
                    0x94, 0x21, 0xFF, 0xE8, 0x7C, 0x08, 0x02, 0xA6, 0x3D, 0x00, 0x10, 0x30, 0x3D, 0x40, 0x10, 0x30,
                    0x93, 0xE1, 0x00, 0x14, 0x3F, 0xE0, 0x03, 0x16, 0x63, 0xFF, 0x68, 0x18, 0x90, 0x01, 0x00, 0x1C,
                    0x93, 0x81, 0x00, 0x08, 0x7F, 0xE9, 0x03, 0xA6, 0x93, 0xA1, 0x00, 0x0C, 0x3D, 0x20, 0x10, 0x30,
                    0x93, 0xC1, 0x00, 0x10, 0x61, 0x08, 0x30, 0x08, 0x61, 0x4A, 0x30, 0x0C, 0x61, 0x29, 0x30, 0x10,
                    0x83, 0xC8, 0x00, 0x00, 0x83, 0xAA, 0x00, 0x00, 0x83, 0x89, 0x00, 0x00, 0x4E, 0x80, 0x04, 0x21,
                    0x80, 0x83, 0x00, 0x34, 0x2C, 0x04, 0x00, 0x00, 0x41, 0x82, 0x00, 0x6C, 0x3D, 0x20, 0x02, 0x46,
                    0x3C, 0x60, 0x10, 0x30, 0x61, 0x29, 0x0E, 0x54, 0x39, 0x04, 0x07, 0x40, 0x7D, 0x29, 0x03, 0xA6,
                    0x7F, 0x87, 0xE3, 0x78, 0x7F, 0xA6, 0xEB, 0x78, 0x7F, 0xC5, 0xF3, 0x78, 0x60, 0x63, 0x30, 0x00,
                    0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x7F, 0xE9, 0x03, 0xA6, 0x4E, 0x80, 0x04, 0x21,
                    0x3D, 0x20, 0x03, 0x1B, 0x61, 0x29, 0x26, 0x54, 0x38, 0x80, 0x00, 0x00, 0x7D, 0x29, 0x03, 0xA6,
                    0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20, 0x03, 0x04, 0x61, 0x29, 0xA5, 0xD8,
                    0x3C, 0x80, 0x10, 0x30, 0x7D, 0x29, 0x03, 0xA6, 0x60, 0x84, 0x30, 0x00, 0x4C, 0xC6, 0x31, 0x82,
                    0x4E, 0x80, 0x04, 0x21, 0x80, 0x01, 0x00, 0x1C, 0x3D, 0x20, 0x01, 0x0F, 0x61, 0x29, 0x6A, 0xE0,
                    0x83, 0x81, 0x00, 0x08, 0x83, 0xA1, 0x00, 0x0C, 0x7D, 0x29, 0x03, 0xA6, 0x83, 0xC1, 0x00, 0x10,
                    0x7C, 0x08, 0x03, 0xA6, 0x83, 0xE1, 0x00, 0x14, 0x38, 0x21, 0x00, 0x18, 0x4E, 0x80, 0x00, 0x20,
                    0x60, 0x00, 0x00, 0x00
                });
            GeckoU.CallFunction(0x03B00000, new uint[1]);
        }

        private void EnchantCommandBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            if (enchantIDBox.SelectedIndex == -1 || string.IsNullOrWhiteSpace(enchantLevelBox.Text))
            {
                Messaging.Show("Make sure all fields are filled out before executing a command");
                return;
            }

            if (Convert.ToInt32(enchantLevelBox.Text) < -32768 || Convert.ToInt32(enchantLevelBox.Text) > 32767)
            {
                Messaging.Show("Enchantment level must be in between -32768 and 32767.");
                return;
            }

            GeckoU.WriteUInt(0x10303008, Enchantments[enchantIDBox.SelectedIndex]);
            GeckoU.WriteUInt(0x1030300C, (uint) int.Parse(enchantLevelBox.Text));
            GeckoU.WriteUInt(0x10303010, 0x1);
            GeckoU.WriteBytes(0x03B10000,
                new byte[]
                {
                    0x94, 0x21, 0xFF, 0xE8, 0x7C, 0x08, 0x02, 0xA6, 0x3D, 0x00, 0x10, 0x30, 0x3D, 0x40, 0x10, 0x30,
                    0x93, 0xE1, 0x00, 0x14, 0x3F, 0xE0, 0x03, 0x16, 0x63, 0xFF, 0x68, 0x18, 0x90, 0x01, 0x00, 0x1C,
                    0x93, 0x81, 0x00, 0x08, 0x7F, 0xE9, 0x03, 0xA6, 0x93, 0xA1, 0x00, 0x0C, 0x3D, 0x20, 0x10, 0x30,
                    0x93, 0xC1, 0x00, 0x10, 0x61, 0x08, 0x30, 0x08, 0x61, 0x4A, 0x30, 0x0C, 0x61, 0x29, 0x30, 0x10,
                    0x83, 0xC8, 0x00, 0x00, 0x83, 0xAA, 0x00, 0x00, 0x83, 0x89, 0x00, 0x00, 0x4E, 0x80, 0x04, 0x21,
                    0x80, 0x83, 0x00, 0x34, 0x2C, 0x04, 0x00, 0x00, 0x41, 0x82, 0x00, 0x6C, 0x3D, 0x20, 0x02, 0x2F,
                    0x3C, 0x60, 0x10, 0x30, 0x61, 0x29, 0x15, 0x18, 0x39, 0x04, 0x07, 0x40, 0x7D, 0x29, 0x03, 0xA6,
                    0x7F, 0x87, 0xE3, 0x78, 0x7F, 0xA6, 0xEB, 0x78, 0x7F, 0xC5, 0xF3, 0x78, 0x60, 0x63, 0x30, 0x00,
                    0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x7F, 0xE9, 0x03, 0xA6, 0x4E, 0x80, 0x04, 0x21,
                    0x3D, 0x20, 0x03, 0x1B, 0x61, 0x29, 0x26, 0x54, 0x38, 0x80, 0x00, 0x00, 0x7D, 0x29, 0x03, 0xA6,
                    0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20, 0x03, 0x04, 0x61, 0x29, 0xA5, 0xD8,
                    0x3C, 0x80, 0x10, 0x30, 0x7D, 0x29, 0x03, 0xA6, 0x60, 0x84, 0x30, 0x00, 0x4C, 0xC6, 0x31, 0x82,
                    0x4E, 0x80, 0x04, 0x21, 0x80, 0x01, 0x00, 0x1C, 0x3D, 0x20, 0x01, 0x0F, 0x61, 0x29, 0x6A, 0xE0,
                    0x83, 0x81, 0x00, 0x08, 0x83, 0xA1, 0x00, 0x0C, 0x7D, 0x29, 0x03, 0xA6, 0x83, 0xC1, 0x00, 0x10,
                    0x7C, 0x08, 0x03, 0xA6, 0x83, 0xE1, 0x00, 0x14, 0x38, 0x21, 0x00, 0x18, 0x4E, 0x80, 0x00, 0x20,
                    0x60, 0x00, 0x00, 0x00
                });
            GeckoU.CallFunction(0x03B10000, new uint[1]);
        }

        private void TimeCommandBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            if (string.IsNullOrWhiteSpace(timeAmountBox.Text))
            {
                Messaging.Show("Make sure all fields are filled out before executing a command");
                return;
            }

            byte[] command = new byte[]
            {
                0x94, 0x21, 0xFF, 0xE8, 0x7C, 0x08, 0x02, 0xA6, 0x3D, 0x00, 0x10, 0x30, 0x3D, 0x40, 0x10, 0x30, 0x93,
                0xE1, 0x00, 0x14, 0x3F, 0xE0, 0x03, 0x16, 0x63, 0xFF, 0x68, 0x18, 0x90, 0x01, 0x00, 0x1C, 0x93, 0x81,
                0x00, 0x08, 0x7F, 0xE9, 0x03, 0xA6, 0x93, 0xA1, 0x00, 0x0C, 0x3D, 0x20, 0x10, 0x30, 0x93, 0xC1, 0x00,
                0x10, 0x61, 0x08, 0x30, 0x08, 0x61, 0x4A, 0x30, 0x0C, 0x61, 0x29, 0x30, 0x10, 0x83, 0xC8, 0x00, 0x00,
                0x83, 0xAA, 0x00, 0x00, 0x83, 0x89, 0x00, 0x00, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x83, 0x00, 0x34, 0x2C,
                0x04, 0x00, 0x00, 0x41, 0x82, 0x00, 0x6C, 0x3D, 0x20, 0x02, 0x9F, 0x3C, 0x60, 0x10, 0x30, 0x61, 0x29,
                0x92, 0xE0, 0x39, 0x04, 0x07, 0x40, 0x7D, 0x29, 0x03, 0xA6, 0x7F, 0x87, 0xE3, 0x78, 0x7F, 0xA6, 0xEB,
                0x78, 0x7F, 0xC5, 0xF3, 0x78, 0x60, 0x63, 0x30, 0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21,
                0x7F, 0xE9, 0x03, 0xA6, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20, 0x03, 0x1B, 0x61, 0x29, 0x26, 0x54, 0x38,
                0x80, 0x00, 0x00, 0x7D, 0x29, 0x03, 0xA6, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20,
                0x03, 0x04, 0x61, 0x29, 0xA5, 0xD8, 0x3C, 0x80, 0x10, 0x30, 0x7D, 0x29, 0x03, 0xA6, 0x60, 0x84, 0x30,
                0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x01, 0x00, 0x1C, 0x3D, 0x20, 0x01, 0x0F,
                0x61, 0x29, 0x6A, 0xE0, 0x83, 0x81, 0x00, 0x08, 0x83, 0xA1, 0x00, 0x0C, 0x7D, 0x29, 0x03, 0xA6, 0x83,
                0xC1, 0x00, 0x10, 0x7C, 0x08, 0x03, 0xA6, 0x83, 0xE1, 0x00, 0x14, 0x38, 0x21, 0x00, 0x18, 0x4E, 0x80,
                0x00, 0x20, 0x60, 0x00, 0x00, 0x00
            };
            GeckoU.WriteUInt(0x10303008, 0x0);
            GeckoU.WriteUInt(0x1030300C, (uint) int.Parse(timeAmountBox.Text));
            GeckoU.WriteUInt(0x10303010, 0x0);
            GeckoU.WriteBytes(0x03B20000, command);
            GeckoU.CallFunction(0x03B20000, new uint[1]);
        }

        private void KillCommandBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            byte[] command = new byte[]
            {
                0x94, 0x21, 0xFF, 0xE8, 0x7C, 0x08, 0x02, 0xA6, 0x3D, 0x00, 0x10, 0x30, 0x3D, 0x40, 0x10, 0x30, 0x93,
                0xE1, 0x00, 0x14, 0x3F, 0xE0, 0x03, 0x16, 0x63, 0xFF, 0x68, 0x18, 0x90, 0x01, 0x00, 0x1C, 0x93, 0x81,
                0x00, 0x08, 0x7F, 0xE9, 0x03, 0xA6, 0x93, 0xA1, 0x00, 0x0C, 0x3D, 0x20, 0x10, 0x30, 0x93, 0xC1, 0x00,
                0x10, 0x61, 0x08, 0x30, 0x08, 0x61, 0x4A, 0x30, 0x0C, 0x61, 0x29, 0x30, 0x10, 0x83, 0xC8, 0x00, 0x00,
                0x83, 0xAA, 0x00, 0x00, 0x83, 0x89, 0x00, 0x00, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x83, 0x00, 0x34, 0x2C,
                0x04, 0x00, 0x00, 0x41, 0x82, 0x00, 0x6C, 0x3D, 0x20, 0x02, 0x52, 0x3C, 0x60, 0x10, 0x30, 0x61, 0x29,
                0x0B, 0x58, 0x39, 0x04, 0x07, 0x40, 0x7D, 0x29, 0x03, 0xA6, 0x7F, 0x87, 0xE3, 0x78, 0x7F, 0xA6, 0xEB,
                0x78, 0x7F, 0xC5, 0xF3, 0x78, 0x60, 0x63, 0x30, 0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21,
                0x7F, 0xE9, 0x03, 0xA6, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20, 0x03, 0x1B, 0x61, 0x29, 0x26, 0x54, 0x38,
                0x80, 0x00, 0x00, 0x7D, 0x29, 0x03, 0xA6, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20,
                0x03, 0x04, 0x61, 0x29, 0xA5, 0xD8, 0x3C, 0x80, 0x10, 0x30, 0x7D, 0x29, 0x03, 0xA6, 0x60, 0x84, 0x30,
                0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x01, 0x00, 0x1C, 0x3D, 0x20, 0x01, 0x0F,
                0x61, 0x29, 0x6A, 0xE0, 0x83, 0x81, 0x00, 0x08, 0x83, 0xA1, 0x00, 0x0C, 0x7D, 0x29, 0x03, 0xA6, 0x83,
                0xC1, 0x00, 0x10, 0x7C, 0x08, 0x03, 0xA6, 0x83, 0xE1, 0x00, 0x14, 0x38, 0x21, 0x00, 0x18, 0x4E, 0x80,
                0x00, 0x20, 0x60, 0x00, 0x00, 0x00
            };
            GeckoU.WriteUInt(0x10303008, 0x0);
            GeckoU.WriteUInt(0x1030300C, 0x0);
            GeckoU.WriteUInt(0x10303010, 0x0);
            GeckoU.WriteBytes(0x03B30000, command);
            GeckoU.CallFunction(0x03B30000, new uint[1]);
        }

        private void DownfallCommandBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            byte[] command = new byte[]
            {
                0x94, 0x21, 0xFF, 0xE8, 0x7C, 0x08, 0x02, 0xA6, 0x3D, 0x00, 0x10, 0x30, 0x3D, 0x40, 0x10, 0x30, 0x93,
                0xE1, 0x00, 0x14, 0x3F, 0xE0, 0x03, 0x16, 0x63, 0xFF, 0x68, 0x18, 0x90, 0x01, 0x00, 0x1C, 0x93, 0x81,
                0x00, 0x08, 0x7F, 0xE9, 0x03, 0xA6, 0x93, 0xA1, 0x00, 0x0C, 0x3D, 0x20, 0x10, 0x30, 0x93, 0xC1, 0x00,
                0x10, 0x61, 0x08, 0x30, 0x08, 0x61, 0x4A, 0x30, 0x0C, 0x61, 0x29, 0x30, 0x10, 0x83, 0xC8, 0x00, 0x00,
                0x83, 0xAA, 0x00, 0x00, 0x83, 0x89, 0x00, 0x00, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x83, 0x00, 0x34, 0x2C,
                0x04, 0x00, 0x00, 0x41, 0x82, 0x00, 0x6C, 0x3D, 0x20, 0x02, 0xA0, 0x3C, 0x60, 0x10, 0x30, 0x61, 0x29,
                0x00, 0xA4, 0x39, 0x04, 0x07, 0x40, 0x7D, 0x29, 0x03, 0xA6, 0x7F, 0x87, 0xE3, 0x78, 0x7F, 0xA6, 0xEB,
                0x78, 0x7F, 0xC5, 0xF3, 0x78, 0x60, 0x63, 0x30, 0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21,
                0x7F, 0xE9, 0x03, 0xA6, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20, 0x03, 0x1B, 0x61, 0x29, 0x26, 0x54, 0x38,
                0x80, 0x00, 0x00, 0x7D, 0x29, 0x03, 0xA6, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20,
                0x03, 0x04, 0x61, 0x29, 0xA5, 0xD8, 0x3C, 0x80, 0x10, 0x30, 0x7D, 0x29, 0x03, 0xA6, 0x60, 0x84, 0x30,
                0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x01, 0x00, 0x1C, 0x3D, 0x20, 0x01, 0x0F,
                0x61, 0x29, 0x6A, 0xE0, 0x83, 0x81, 0x00, 0x08, 0x83, 0xA1, 0x00, 0x0C, 0x7D, 0x29, 0x03, 0xA6, 0x83,
                0xC1, 0x00, 0x10, 0x7C, 0x08, 0x03, 0xA6, 0x83, 0xE1, 0x00, 0x14, 0x38, 0x21, 0x00, 0x18, 0x4E, 0x80,
                0x00, 0x20, 0x60, 0x00, 0x00, 0x00
            };
            GeckoU.WriteUInt(0x10303008, 0x0);
            GeckoU.WriteUInt(0x1030300C, 0x0);
            GeckoU.WriteUInt(0x10303010, 0x0);
            GeckoU.WriteBytes(0x03B30000, command);
            GeckoU.CallFunction(0x03B30000, new uint[1]);
        }

        private void SetWorldSpawnBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            byte[] command = new byte[]
            {
                0x94, 0x21, 0xFF, 0xE8, 0x7C, 0x08, 0x02, 0xA6, 0x3D, 0x00, 0x10, 0x30, 0x3D, 0x40, 0x10, 0x30, 0x93,
                0xE1, 0x00, 0x14, 0x3F, 0xE0, 0x03, 0x16, 0x63, 0xFF, 0x68, 0x18, 0x90, 0x01, 0x00, 0x1C, 0x93, 0x81,
                0x00, 0x08, 0x7F, 0xE9, 0x03, 0xA6, 0x93, 0xA1, 0x00, 0x0C, 0x3D, 0x20, 0x10, 0x30, 0x93, 0xC1, 0x00,
                0x10, 0x61, 0x08, 0x30, 0x08, 0x61, 0x4A, 0x30, 0x0C, 0x61, 0x29, 0x30, 0x10, 0x83, 0xC8, 0x00, 0x00,
                0x83, 0xAA, 0x00, 0x00, 0x83, 0x89, 0x00, 0x00, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x83, 0x00, 0x34, 0x2C,
                0x04, 0x00, 0x00, 0x41, 0x82, 0x00, 0x6C, 0x3D, 0x20, 0x02, 0x8B, 0x3C, 0x60, 0x10, 0x30, 0x61, 0x29,
                0x51, 0x58, 0x39, 0x04, 0x07, 0x40, 0x7D, 0x29, 0x03, 0xA6, 0x7F, 0x87, 0xE3, 0x78, 0x7F, 0xA6, 0xEB,
                0x78, 0x7F, 0xC5, 0xF3, 0x78, 0x60, 0x63, 0x30, 0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21,
                0x7F, 0xE9, 0x03, 0xA6, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20, 0x03, 0x1B, 0x61, 0x29, 0x26, 0x54, 0x38,
                0x80, 0x00, 0x00, 0x7D, 0x29, 0x03, 0xA6, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20,
                0x03, 0x04, 0x61, 0x29, 0xA5, 0xD8, 0x3C, 0x80, 0x10, 0x30, 0x7D, 0x29, 0x03, 0xA6, 0x60, 0x84, 0x30,
                0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x01, 0x00, 0x1C, 0x3D, 0x20, 0x01, 0x0F,
                0x61, 0x29, 0x6A, 0xE0, 0x83, 0x81, 0x00, 0x08, 0x83, 0xA1, 0x00, 0x0C, 0x7D, 0x29, 0x03, 0xA6, 0x83,
                0xC1, 0x00, 0x10, 0x7C, 0x08, 0x03, 0xA6, 0x83, 0xE1, 0x00, 0x14, 0x38, 0x21, 0x00, 0x18, 0x4E, 0x80,
                0x00, 0x20, 0x60, 0x00, 0x00, 0x00
            };
            GeckoU.WriteUInt(0x10303008, 0x0);
            GeckoU.WriteUInt(0x1030300C, 0x0);
            GeckoU.WriteUInt(0x10303010, 0x0);
            GeckoU.WriteBytes(0x03B30000, command);
            GeckoU.CallFunction(0x03B30000, new uint[1]);
        }

        private void GameModeCommandBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            byte[] command = new byte[]
            {
                0x94, 0x21, 0xFF, 0xE8, 0x7C, 0x08, 0x02, 0xA6, 0x3D, 0x00, 0x10, 0x30, 0x3D, 0x40, 0x10, 0x30, 0x93,
                0xE1, 0x00, 0x14, 0x3F, 0xE0, 0x03, 0x16, 0x63, 0xFF, 0x68, 0x18, 0x90, 0x01, 0x00, 0x1C, 0x93, 0x81,
                0x00, 0x08, 0x7F, 0xE9, 0x03, 0xA6, 0x93, 0xA1, 0x00, 0x0C, 0x3D, 0x20, 0x10, 0x30, 0x93, 0xC1, 0x00,
                0x10, 0x61, 0x08, 0x30, 0x08, 0x61, 0x4A, 0x30, 0x0C, 0x61, 0x29, 0x30, 0x10, 0x83, 0xC8, 0x00, 0x00,
                0x83, 0xAA, 0x00, 0x00, 0x83, 0x89, 0x00, 0x00, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x83, 0x00, 0x34, 0x2C,
                0x04, 0x00, 0x00, 0x41, 0x82, 0x00, 0x6C, 0x3D, 0x20, 0x02, 0x2D, 0x3C, 0x60, 0x10, 0x30, 0x61, 0x29,
                0x02, 0xC4, 0x39, 0x04, 0x07, 0x40, 0x7D, 0x29, 0x03, 0xA6, 0x7F, 0x87, 0xE3, 0x78, 0x7F, 0xA6, 0xEB,
                0x78, 0x7F, 0xC5, 0xF3, 0x78, 0x60, 0x63, 0x30, 0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21,
                0x7F, 0xE9, 0x03, 0xA6, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20, 0x03, 0x1B, 0x61, 0x29, 0x26, 0x54, 0x38,
                0x80, 0x00, 0x00, 0x7D, 0x29, 0x03, 0xA6, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x3D, 0x20,
                0x03, 0x04, 0x61, 0x29, 0xA5, 0xD8, 0x3C, 0x80, 0x10, 0x30, 0x7D, 0x29, 0x03, 0xA6, 0x60, 0x84, 0x30,
                0x00, 0x4C, 0xC6, 0x31, 0x82, 0x4E, 0x80, 0x04, 0x21, 0x80, 0x01, 0x00, 0x1C, 0x3D, 0x20, 0x01, 0x0F,
                0x61, 0x29, 0x6A, 0xE0, 0x83, 0x81, 0x00, 0x08, 0x83, 0xA1, 0x00, 0x0C, 0x7D, 0x29, 0x03, 0xA6, 0x83,
                0xC1, 0x00, 0x10, 0x7C, 0x08, 0x03, 0xA6, 0x83, 0xE1, 0x00, 0x14, 0x38, 0x21, 0x00, 0x18, 0x4E, 0x80,
                0x00, 0x20, 0x60, 0x00, 0x00, 0x00
            };
            GeckoU.WriteUInt(0x10303008, 0x0);
            GeckoU.WriteUInt(0x1030300C, 0x0);
            GeckoU.WriteUInt(0x10303010, 0x0);
            GeckoU.WriteBytes(0x03B30000, command);
            GeckoU.CallFunction(0x03B30000, new uint[1]);
        }

        private void GiveXpOrbsBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            if (string.IsNullOrWhiteSpace(xpAmountBox.Text))
            {
                Messaging.Show("Make sure all fields are filled out before executing a command");
                return;
            }

            try
            {
                var playerPtr = GeckoU.PeekUInt(GeckoU.PeekUInt(0x109CD8E4) + 0x34);
                var amount = Convert.ToInt32(xpAmountBox.Text);
                GeckoU.CallFunction(0x027250DC, playerPtr, unchecked((uint) amount));
            }
            catch (OverflowException)
            {
                Messaging.Show(MessageBoxIcon.Error,
                    "Please input a value that is between -2147483648 and 2147483647.");
            }
            catch (Exception)
            {
                Messaging.Show(MessageBoxIcon.Error, "An unknown error has occurred.");
            }
        }

        private void GiveXpLevelsBtnClicked(object sender, EventArgs e)
        {
            if (!IsPointerLoaded())
            {
                Messaging.Show("Commands only work in-game, please load a world before executing a command");
                return;
            }

            if (string.IsNullOrWhiteSpace(xpAmountBox.Text))
            {
                Messaging.Show("Make sure all fields are filled out before executing a command");
                return;
            }

            try
            {
                var playerPtr = GeckoU.PeekUInt(GeckoU.PeekUInt(0x109CD8E4) + 0x34);
                var amount = Convert.ToInt32(xpAmountBox.Text);
                GeckoU.CallFunction(0x02725330, playerPtr, unchecked((uint) amount));
            }
            catch (OverflowException)
            {
                Messaging.Show(MessageBoxIcon.Error,
                    "Please input a value that is between -2147483648 and 2147483647.");
            }
            catch (Exception)
            {
                Messaging.Show(MessageBoxIcon.Error, "An unknown error has occurred.");
            }
        }

        private async void TellrawCmdBtnClicked(object sender, EventArgs e)
        {
            uint textStart = 0x107A097C;
            uint textEnd = 0x107A0A7C;
            string message = string.Empty;

            if (bedrockTellBox.Checked)
            {
                javaTellBox.Checked = false;
                message = "\0" + StringUtils.InsertStrings("<" + tellNameBox.Text + "> " + tellMsgBox.Text, 1, "\0");
            }

            if (javaTellBox.Checked)
            {
                bedrockTellBox.Checked = false;
                message = "\0" + StringUtils.InsertStrings("[" + tellNameBox.Text + "] " + tellMsgBox.Text, 1, "\0");
            }

            GeckoU.ClearString(textStart, textEnd);
            await PutTaskDelay(200);
            GeckoU.WriteString(textStart, message);
            DownfallCommandBtnClicked(null, null);
        }

        private void CommandsInMinigamesToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x022F0774, 0x38600000, 0x38600002, CommandsInMinigames.Checked);
            GeckoU.WriteUIntToggle(0x0245FC9C, 0x38600000, 0x38600002, CommandsInMinigames.Checked);
            GeckoU.WriteUIntToggle(0x02520964, 0x38600000, 0x38600002, CommandsInMinigames.Checked);
            GeckoU.WriteUIntToggle(0x029F8FE0, 0x38600000, 0x38600002, CommandsInMinigames.Checked);
            GeckoU.WriteUIntToggle(0x029FFE60, 0x38600000, 0x38600002, CommandsInMinigames.Checked);
        }

        #endregion

        #region minigames

        private void AllPermissionsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C57E94, On, 0x57E3063E,
                AllPermissions.Checked); //MiniGameDef::MayUseBlock(const(int))
            GeckoU.WriteUIntToggle(0x02C57E34, On, 0x57E3063E,
                AllPermissions.Checked); //MiniGameDef::MayUse(const(int))
            GeckoU.WriteUIntToggle(0x02C51C20, On, 0x57E3063E,
                AllPermissions.Checked); //MiniGameDef::MayUseBlock(const(int))
            GeckoU.WriteUIntToggle(0x02C5CC84, On, 0X88630124,
                AllPermissions.Checked); //MiniGameDef::CanCraft(const(void))
            GeckoU.WriteUIntToggle(0x02C57D74, On, 0x57E3063E,
                AllPermissions.Checked); //MiniGameDef::MayPlace(const(int))
            GeckoU.WriteUIntToggle(0x02C57DD4, On, 0x57E3063E,
                AllPermissions.Checked); //MiniGameDef::MayDestroy(const(int))
        }

        private void AlwaysDamagedToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C7DDD0, On, Off,
                AlwaysDamaged.Checked); //MasterGameMode::IsInKillBox((boost::shared_ptr__tm__8_6Entity))
        }

        private void TntGriefingToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C59E04, On, Off,
                TNTGriefing.Checked); //MiniGameDef::canTntDestroyBlocks(const(void))
        }

        private void DisabledKillBarriersToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C7DD38, Off, On,
                DisabledKillBarriers.Checked); //MasterGameMode::IsInKillBox((boost::shared_ptr__tm__8_6Entity))
        }

        private void AntiEndGameToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C9554C, Off, On, AntiEndGame.Checked); //MasterGameMode::CanEndPostGame((void))
        }

        private async void EndGameClicked(object sender, EventArgs e)
        {
            GeckoU.WriteUInt(0x02C93A90, Blr);
            await PutTaskDelay(200);
            GeckoU.WriteUInt(0x02C93A90, Mflr);
        }

        private void AllowMobsToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x032252FC, 0x38800001, 0x7C641B78, AllowMobs.Checked); //Animals
            GeckoU.WriteUIntToggle(0x03225208, 0x38800001, 0x7C641B78, AllowMobs.Checked);

            GeckoU.WriteUIntToggle(0x03225330, 0x38800001, 0x7C641B78, AllowMobs.Checked); //NPCs
            GeckoU.WriteUIntToggle(0x0322523C, 0x38800001, 0x7C641B78, AllowMobs.Checked);
        }

        private void LiquidsCanConvertToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C59DB8, On, 0x5403063E, LiquidsCanConvert.Checked);
        }

        private void MapSizeBoxChanged(object sender, EventArgs e)
        {
            switch (MapSizeBox.SelectedIndex)
            {
                case 0:
                    GeckoU.WriteUInt(0x02D16014, 0x806300B4);
                    break;

                case 1:
                    GeckoU.WriteUInt(0x02D16014, 0x38600001);
                    break;

                case 2:
                    GeckoU.WriteUInt(0x02D16014, 0x38600000);
                    break;
            }
        }

        private void TumbleHudToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02E3A950, Off, 0x57E3063E, TumbleHUD.Checked); //UIScene_HUD::HideMainHUD((void))
            GeckoU.WriteUIntToggle(0x02C57EBC, Off, 0x8863012A,
                TumbleHUD.Checked); //MiniGameDef::LockedInventory(const(void))
        }

        private void RequiredPlayersSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteUInt(0x02C7C004,
                GeckoU.Mix(0x38600000, RequiredPlayersSlider.Value)); //MasterGameMode::GetMinimumPlayersToStart
        }

        private void RefillIntervalSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteUInt(0x02C5CC74,
                GeckoU.Mix(0x38600000, RefillIntervalSlider.Value)); //MiniGameDef::GetItemRespawnTime
        }

        private void GlideRingScoreSliderChanged(object sender, EventArgs e)
        {
            GeckoU.WriteUInt(0x02D6EE14,
                GeckoU.Mix(0x38600000, ringScoreGreen.Value)); //ScoreRingRuleDefinition::getPointValue((void))
            GeckoU.WriteUInt(0x02D6EE1C, GeckoU.Mix(0x38600000, ringScoreBlue.Value));
            GeckoU.WriteUInt(0x02D6EE24, GeckoU.Mix(0x38600000, ringScoreOrange.Value));
        }

        private void SqueakInfinitelyToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x031A2730, Blr, Mflr,
                SqueakInfinitely.Checked); //MultiPlayerGameMode::CanMakeSpectateSound((void))
        }

        private void NoPositionLockToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x027CD4B0, Off, 0x88630814, NoPosLock.Checked);
            GeckoU.WriteUIntToggle(0x02CDF9B0, Off, 0x88630814, NoPosLock.Checked);
            GeckoU.WriteUIntToggle(0x02FC804C, Off, 0x88630814, NoPosLock.Checked);
            GeckoU.WriteUIntToggle(0x0323F7F8, Off, 0x88630814, NoPosLock.Checked);
            GeckoU.WriteUIntToggle(0x0332d270, Off, 0x88630814, NoPosLock.Checked);
        }

        private void UnlockInventoryToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C57EBC, Off, 0x8863012A, UnlockInventoty.Checked);
        }

        private void DisableCameraAnimationToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C5F808, Off, 0x5403D97E, DisableCameraAnimation.Checked);
        }

        private void SoloToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02C50078, Nop, 0x41820010, Solo.Checked);
            GeckoU.WriteUIntToggle(0x02C50078, 0x38600002, 0x7FE3FB78, Solo.Checked);
        }
        #endregion

        #endregion memory editing

        private void AirborneSpeedSliderChanged(object sender, EventArgs e)
        {
            var airbornespeed = Miscellaneous.FloatToInt32Bits((float) AirborneSpeedSlider.Value);
            uint realValue = GeckoU.PeekUInt(0x10665EF4);
            GeckoU.WriteUInt(0x11010000, realValue);
            GeckoU.WriteUInt(0x0258229c, 0x3D001101); // load our address :)
            GeckoU.WriteUInt(0x025822a4, 0xC3E80000); // lfs f31,0x...(..)
            GeckoU.WriteUInt(0x11010000, (uint)airbornespeed);
        }

        private void AlwaysDaylightToggled(object sender, EventArgs e)
        {
            GeckoU.WriteUIntToggle(0x02555318, 0xFC210824, 0x4e800421, AlwaysDaylight.Checked); // fdiv f1, f1, f1
        }
    }
}
