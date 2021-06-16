﻿using MetroFramework;
using Minecraft_Wii_U_Mod_Injector.Helpers.Files;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Minecraft_Wii_U_Mod_Injector.Forms;
using Minecraft_Wii_U_Mod_Injector.Helpers.Win_Forms;

namespace Minecraft_Wii_U_Mod_Injector.Helpers
{
    class Setup
    {
        public static MainForm Injector = new();

        public static string LocalVer = "v5.1.8";
        public static string GitVer = string.Empty;
        public static bool PreRelease;

        public Setup(MainForm window)
        {
            Injector = window;
        }

        public static void SetupInjector()
        {
            try
            {
#if !DEBUG
                RetrieveGitVersion();
#endif

#if DEBUG
                LocalVer = LocalVer + " (Debugging)";
#endif

                Injector.BuildNotesBox.Text = Properties.Resources.releaseNotes;
                Injector.BuildVerTitleLbl.Text = @"Patch Notes for " + LocalVer;
                Injector.BuildTile.Text = LocalVer;

                DiscordRp.Initialize();

                SetupUserPrefs();

                DiscordRp.SetPresence("Disconnected", "Idle");
                States.SwapState(States.StatesIds.Disconnected);
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Failed to setup", false, true);
                DiscordRp.Deinitialize();
                Environment.Exit(0);
            }
        }

        public static async Task RetrieveGitVersion()
        {
            try
            {
                GitHubClient verChecker = new GitHubClient(new ProductHeaderValue("MCWiiUMIClient"));
                IReadOnlyList<Release> releases = await verChecker.Repository.Release.GetAll("Kashiiera", "Minecraft-Wii-U-Mod-Injector");

                GitVer = releases[0].TagName;
                PreRelease = releases[0].Prerelease;

                if (LocalVer != GitVer && !PreRelease)
                {
                    Messaging.Show("The current injector version " + LocalVer + " is outdated! Please download version " + GitVer + " at " +
                        "https://github.com/Kashiiera/Minecraft-Wii-U-Mod-Injector/releases \nas it may contain feature updates, bug fixes and more.");
                    Environment.Exit(0);
                }
                if (LocalVer != GitVer && Settings.EqualsTo("PrereleaseOptIn", "True", "Updates") && PreRelease)
                {
                    Messaging.Show("Thank you for being interested in testing pre-releases!\nThe current injector version " + LocalVer +
                        " is outdated! Please download version " + GitVer + " at " + "https://github.com/Kashiiera/Minecraft-Wii-U-Mod-Injector/releases \nas it may contain feature updates, bug fixes and more.");
                    Environment.Exit(0);
                }
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Something went wrong while retrieving the latest release, please try re-launching the Injector.\n" +
                    "If this issue persist please contact the developers.", false, false);
            }
        }

        public static void SetupUserPrefs()
        {
            try
            {
                try
                {
                    if(!Settings.Exists("FirstLaunch", "Display"))
                        Settings.Write("FirstLaunch", "True", "Display");

                    var firstLaunch = Convert.ToBoolean(Settings.Read("FirstLaunch", "Display"));

                    Injector.Theme = Injector.StyleMngr.Theme =
                        (MetroThemeStyle) Enum.Parse(typeof(MetroThemeStyle), Settings.Read("Theme", "Display"));
                    Injector.Style = Injector.StyleMngr.Style =
                        (MetroColorStyle) Enum.Parse(typeof(MetroColorStyle), Settings.Read("Color", "Display"));

                    Injector.themeBox.Text = Settings.Read("Theme", "Display");
                    Injector.colorsBox.Text = Settings.Read("Color", "Display");

                    Injector.MainTabs.SelectedIndex = Convert.ToInt32(Settings.Read("TabIndex", "Display"));

                    Injector.CheckForPreRelease.Checked =
                        Convert.ToBoolean(Settings.Read("PrereleaseOptIn", "Updates"));

                    Injector.discordRpcCheckBox.Checked = Convert.ToBoolean(Settings.Read("DiscordRPC", "Discord"));

                    new LanguageMngr(Injector).ApplyLanguage(Settings.Read("Language", "Display"));

                    if (firstLaunch)
                        Messaging.Show("Welcome to the Minecraft: Wii U Mod Injector! the first and longest lasting Mod Injector for Minecraft: Wii U Edition!\n" +
                                       "Before we get started, please take a look at the setup tutorial here: https://www.youtube.com/watch?v=be5fNSgxhrU. \nIf you enjoy my " +
                                       "work then a sub on my YouTube channel is appreciated. Happy modding!");

                    Settings.Write("FirstLaunch", "False", "Display");
                }
                catch (Exception)
                {
                    //Failing to set User Preferences isn't a big deal, so we ignore the exception if one happens
                }

                if (Settings.EqualsTo("ReleaseNotes", "all", "Display"))
                {
                    Injector.releaseNotesToggle.Checked = false;
                    Injector.BuildNotesBox.Text = Properties.Resources.releaseNotes;
                }
                else if (Settings.EqualsTo("ReleaseNotes", "current", "Display"))
                {
                    Injector.releaseNotesToggle.Checked = true;
                    Injector.BuildNotesBox.Text = Properties.Resources.releaseNote;
                }
            }
            catch (Exception error)
            {
                Exceptions.LogError(error, "Exception in SetupUserPrefs()", false, true);
                Environment.Exit(0);
            }
        }
    }
}