using Editor.PlatformSpecific;
using Editor.Utils;
using System;
using OperatingSystem = System.OperatingSystem;

namespace Editor.Saved;

[Serializable]
public class UiConfig : ConfigBase
{
    public ImGuiStyles.Style Style = ImGuiStyles.DefaultStyle;

    public override object Clone() => Clone<UiConfig>();

    internal override void FirstTimeSetup()
    {
        if (!Platform.ShouldUseDarkMode())
            Style = ImGuiStyles.Style.Light;
    }
}
