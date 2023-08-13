using Editor.ImGuiNet;
using Editor.Logging;
using ImGuiNET;
using System.IO;
using System.Numerics;

namespace Editor.Utils
{
    public static class ImGuiStyles
    {
        public enum Style
        {
            Dark,
            Light
        }

        public const Style DefaultStyle = Style.Dark;

        public static void Setup(Style style)
        {
            switch (style)
            {
                case Style.Dark:
                    SetupDark();
                    break;

                case Style.Light:
                    SetupLight();
                    break;

                default:
                    Logger.Log($"Invalid style: {style}", LogLevel.Error);
                    break;
            }
        }

        public static void SetupDark()
        {
            // Future Dark stylerewrking from ImThemes
            ImGuiStylePtr style = ImGui.GetStyle();

            style.Alpha = 0.85f; // Changed from 1.0f
            style.DisabledAlpha = 1.0f;
            style.WindowPadding = new Vector2(12.0f, 12.0f);
            style.WindowRounding = 0.0f;
            style.WindowBorderSize = 0.0f;
            style.WindowMinSize = new Vector2(20.0f, 20.0f);
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.None;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(6.0f, 6.0f);
            style.FrameRounding = 0.0f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(12.0f, 6.0f);
            style.ItemInnerSpacing = new Vector2(6.0f, 3.0f);
            style.CellPadding = new Vector2(12.0f, 6.0f);
            style.IndentSpacing = 20.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 12.0f;
            style.ScrollbarRounding = 0.0f;
            style.GrabMinSize = 12.0f;
            style.GrabRounding = 0.0f;
            style.TabRounding = 0.0f;
            style.TabBorderSize = 0.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int) ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.2745098173618317f, 0.3176470696926117f, 0.4509803950786591f, 1.0f);
            style.Colors[(int) ImGuiCol.WindowBg] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int) ImGuiCol.ChildBg] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int) ImGuiCol.PopupBg] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int) ImGuiCol.Border] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int) ImGuiCol.BorderShadow] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int) ImGuiCol.FrameBg] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.2352941185235977f, 0.2156862765550613f, 0.5960784554481506f, 1.0f);
            style.Colors[(int) ImGuiCol.TitleBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int) ImGuiCol.CheckMark] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.5372549295425415f, 0.5529412031173706f, 1.0f, 1.0f);
            style.Colors[(int) ImGuiCol.Button] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.196078434586525f, 0.1764705926179886f, 0.5450980663299561f, 1.0f);
            style.Colors[(int) ImGuiCol.ButtonActive] = new Vector4(0.2352941185235977f, 0.2156862765550613f, 0.5960784554481506f, 1.0f);
            style.Colors[(int) ImGuiCol.Header] = new Vector4(0.1576470592617989f, 0.1733333402872086f, 0.3890196138620377f, 1.0f); // Changed from 0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f
            style.Colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.196078434586525f, 0.1764705926179886f, 0.5450980663299561f, 1.0f);
            style.Colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.2352941185235977f, 0.2156862765550613f, 0.5960784554481506f, 1.0f);
            style.Colors[(int) ImGuiCol.Separator] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.196078434586525f, 0.1764705926179886f, 0.5450980663299561f, 1.0f);
            style.Colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.2352941185235977f, 0.2156862765550613f, 0.5960784554481506f, 1.0f);
            style.Colors[(int) ImGuiCol.Tab] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int) ImGuiCol.TabHovered] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int) ImGuiCol.TabActive] = new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int) ImGuiCol.PlotLines] = new Vector4(0.5215686559677124f, 0.6000000238418579f, 0.7019608020782471f, 1.0f);
            style.Colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(0.03921568766236305f, 0.9803921580314636f, 0.9803921580314636f, 1.0f);
            style.Colors[(int) ImGuiCol.PlotHistogram] = new Vector4(1.0f, 0.2901960909366608f, 0.5960784554481506f, 1.0f);
            style.Colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(0.9960784316062927f, 0.4745098054409027f, 0.6980392336845398f, 1.0f);
            style.Colors[(int) ImGuiCol.TableHeaderBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int) ImGuiCol.TableBorderStrong] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int) ImGuiCol.TableBorderLight] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int) ImGuiCol.TableRowBg] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int) ImGuiCol.TableRowBgAlt] = new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.2352941185235977f, 0.2156862765550613f, 0.5960784554481506f, 1.0f);
            style.Colors[(int) ImGuiCol.DragDropTarget] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int) ImGuiCol.NavHighlight] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.196078434586525f, 0.1764705926179886f, 0.5450980663299561f, 0.501960813999176f);
            style.Colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.196078434586525f, 0.1764705926179886f, 0.5450980663299561f, 0.501960813999176f);
        }

        public static void SetupLight()
        {
            // Light styledougbinks from ImThemes
            var style = ImGui.GetStyle();

            style.Alpha = 0.85f; // Changed from 1.0f
            style.DisabledAlpha = 0.6000000238418579f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 0.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 0.0f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.CellPadding = new Vector2(4.0f, 2.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 14.0f;
            style.ScrollbarRounding = 9.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 0.0f;
            style.TabRounding = 4.0f;
            style.TabBorderSize = 0.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int) ImGuiCol.Text] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.6000000238418579f, 0.6000000238418579f, 0.6000000238418579f, 1.0f);
            style.Colors[(int) ImGuiCol.WindowBg] = new Vector4(0.9372549057006836f, 0.9372549057006836f, 0.9372549057006836f, 1.0f);
            style.Colors[(int) ImGuiCol.ChildBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int) ImGuiCol.PopupBg] = new Vector4(1.0f, 1.0f, 1.0f, 0.9800000190734863f);
            style.Colors[(int) ImGuiCol.Border] = new Vector4(0.0f, 0.0f, 0.0f, 0.300000011920929f);
            style.Colors[(int) ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int) ImGuiCol.FrameBg] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.4000000059604645f);
            style.Colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.6700000166893005f);
            style.Colors[(int) ImGuiCol.TitleBg] = new Vector4(0.95686274766922f, 0.95686274766922f, 0.95686274766922f, 1.0f);
            style.Colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.8196078538894653f, 0.8196078538894653f, 0.8196078538894653f, 1.0f);
            style.Colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(1.0f, 1.0f, 1.0f, 0.5099999904632568f);
            style.Colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.8588235378265381f, 0.8588235378265381f, 0.8588235378265381f, 1.0f);
            style.Colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.5299999713897705f);
            style.Colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.686274528503418f, 0.686274528503418f, 0.686274528503418f, 0.800000011920929f);
            style.Colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.4862745106220245f, 0.4862745106220245f, 0.4862745106220245f, 0.800000011920929f);
            style.Colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.4862745106220245f, 0.4862745106220245f, 0.4862745106220245f, 1.0f);
            style.Colors[(int) ImGuiCol.CheckMark] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 1.0f);
            style.Colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.7799999713897705f);
            style.Colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.4588235318660736f, 0.5372549295425415f, 0.800000011920929f, 0.6000000238418579f);
            style.Colors[(int) ImGuiCol.Button] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.4000000059604645f);
            style.Colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 1.0f);
            style.Colors[(int) ImGuiCol.ButtonActive] = new Vector4(0.05882352963089943f, 0.529411792755127f, 0.9764705896377563f, 1.0f);
            style.Colors[(int) ImGuiCol.Header] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.3100000023841858f);
            style.Colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 1.0f);
            style.Colors[(int) ImGuiCol.Separator] = new Vector4(0.3882353007793427f, 0.3882353007793427f, 0.3882353007793427f, 0.6200000047683716f);
            style.Colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.1372549086809158f, 0.4392156898975372f, 0.800000011920929f, 0.7799999713897705f);
            style.Colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.1372549086809158f, 0.4392156898975372f, 0.800000011920929f, 1.0f);
            style.Colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.3490196168422699f, 0.3490196168422699f, 0.3490196168422699f, 0.1700000017881393f);
            style.Colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.6700000166893005f);
            style.Colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.949999988079071f);
            style.Colors[(int) ImGuiCol.Tab] = new Vector4(0.7607843279838562f, 0.7960784435272217f, 0.8352941274642944f, 0.9309999942779541f);
            style.Colors[(int) ImGuiCol.TabHovered] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int) ImGuiCol.TabActive] = new Vector4(0.5921568870544434f, 0.7254902124404907f, 0.8823529481887817f, 1.0f);
            style.Colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.9176470637321472f, 0.9254902005195618f, 0.9333333373069763f, 0.9861999750137329f);
            style.Colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.7411764860153198f, 0.8196078538894653f, 0.9137254953384399f, 1.0f);
            style.Colors[(int) ImGuiCol.PlotLines] = new Vector4(0.3882353007793427f, 0.3882353007793427f, 0.3882353007793427f, 1.0f);
            style.Colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.0f, 0.4274509847164154f, 0.3490196168422699f, 1.0f);
            style.Colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.8980392217636108f, 0.6980392336845398f, 0.0f, 1.0f);
            style.Colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.4470588266849518f, 0.0f, 1.0f);
            style.Colors[(int) ImGuiCol.TableHeaderBg] = new Vector4(0.7764706015586853f, 0.8666666746139526f, 0.9764705896377563f, 1.0f);
            style.Colors[(int) ImGuiCol.TableBorderStrong] = new Vector4(0.5686274766921997f, 0.5686274766921997f, 0.6392157077789307f, 1.0f);
            style.Colors[(int) ImGuiCol.TableBorderLight] = new Vector4(0.6784313917160034f, 0.6784313917160034f, 0.7372549176216125f, 1.0f);
            style.Colors[(int) ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int) ImGuiCol.TableRowBgAlt] = new Vector4(0.2980392277240753f, 0.2980392277240753f, 0.2980392277240753f, 0.09000000357627869f);
            style.Colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.3499999940395355f);
            style.Colors[(int) ImGuiCol.DragDropTarget] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.949999988079071f);
            style.Colors[(int) ImGuiCol.NavHighlight] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(0.6980392336845398f, 0.6980392336845398f, 0.6980392336845398f, 0.699999988079071f);
            style.Colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2000000029802322f, 0.2000000029802322f);
            style.Colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2000000029802322f, 0.3499999940395355f);
        }

        public static void SetupFont(Session session, ImGuiRenderer imguiRenderer)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.ClearFonts();
            io.Fonts.AddFontFromFileTTF(Path.Combine(session.ContentDirectory, "Fonts", "imgui.ttf"), 15);
            imguiRenderer.RebuildFontAtlas();
        }
    }
}
