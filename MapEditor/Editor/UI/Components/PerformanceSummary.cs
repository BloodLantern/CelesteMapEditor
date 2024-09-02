using System;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Editor.UI.Components
{
    public class PerformanceSummary : UiComponent, ICloseable, IUpdateable
    {
        private bool windowOpen;
        public bool WindowOpen { get => windowOpen; set => windowOpen = value; }
        public string KeyboardShortcut { get; }

        private Application app;

        private int totalSamples;
        private const int MaxSamples = 50;
        private int arrayIndex = 0;

        private float lastFps;
        private float[] frameRateArray = new float[100];
        private float highestArrayFps;
        private const float MinHighestArrayFps = 60f;
        
        private float lastMemory;
        private float[] memoryArray = new float[100];
        private float highestArrayMemory;
        private const float MinHighestArrayMemory = 50f;
        
        public PerformanceSummary(Application app) : base(RenderingCall.StateEditor) => this.app = app;

        public void Update(GameTime time)
        {
            if (Calc.OnInterval(app.TotalGameTimeSeconds, app.LastTotalGameTime, Application.StatisticsUpdateInterval))
            {
                lastFps = app.Fps;
                frameRateArray[arrayIndex] = lastFps;
                highestArrayFps = MathF.Max(frameRateArray.Max(), MinHighestArrayFps);

                lastMemory = (float) app.Memory / 0x100000f;
                memoryArray[arrayIndex] = lastMemory;
                highestArrayMemory = MathF.Max(memoryArray.Max(), MinHighestArrayMemory);

                arrayIndex = (arrayIndex + 1) % frameRateArray.Length;
                if (totalSamples < MaxSamples)
                    totalSamples++;
            }
        }

        public override void Render()
        {
            if (!windowOpen)
                return;
            
            ImGui.Begin("Performance Summary", ref windowOpen);

            ImGui.PlotLines("##fps", ref frameRateArray[0], Math.Min(totalSamples, frameRateArray.Length), arrayIndex,
                $"FPS: {lastFps:0}", 0, highestArrayFps, new(ImGui.GetContentRegionAvail().X, 50));

            ImGui.PlotLines("##memory", ref memoryArray[0], Math.Min(totalSamples, memoryArray.Length), arrayIndex,
                $"Memory: {lastMemory:F2}MB", 0, highestArrayMemory, new(ImGui.GetContentRegionAvail().X, 50));
            
            ImGui.End();
        }
    }
}
