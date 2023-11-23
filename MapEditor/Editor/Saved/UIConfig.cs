﻿using Editor.PlatformSpecific;
using Editor.Utils;
using System;

namespace Editor.Saved
{
    [Serializable]
    public class UIConfig : ConfigBase
    {
        public ImGuiStyles.Style Style = ImGuiStyles.DefaultStyle;

        public override object Clone() => Clone<UIConfig>();

        internal override void FirstTimeSetup()
        {
#if WINDOWS
            if (!Windows.ShouldSystemUseDarkMode())
                Style = ImGuiStyles.Style.Light;
#endif
        }
    }
}
