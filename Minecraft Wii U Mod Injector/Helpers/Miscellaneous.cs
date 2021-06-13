﻿using MetroFramework.Controls;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Minecraft_Wii_U_Mod_Injector.Helpers
{
    public class Miscellaneous
    {
        public static MainForm Injector = new MainForm();

        public Miscellaneous(MainForm iw)
        {
            Injector = iw;
        }

        public static List<Control> AllMetroControls()
        {
            List<Control> controls = new List<Control>();

            foreach (Control c in Injector.Controls)
                if (c is MetroButton || c is MetroLabel || c is MetroTextBox || c is MetroTile)
                    controls.Add(c);

            foreach (MetroTabPage page in Injector.MainTabs.TabPages)
                foreach (Control c in page.Controls)
                    if (c is MetroCheckBox || c is MetroLabel || c is MetroButton)
                        controls.Add(c);

            foreach (MetroTabPage page in Injector.MinigamesTabs.TabPages)
                foreach (Control c in page.Controls)
                    if (c is MetroCheckBox || c is MetroLabel || c is MetroButton)
                        controls.Add(c);

            return controls;
        }

        public static void SetDoubleBuffered(Control c)
        {
            System.Reflection.PropertyInfo aProp =
                typeof(System.Windows.Forms.Control).GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }
    }
}