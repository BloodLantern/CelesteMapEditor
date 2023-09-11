using Editor.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Editor.Utils
{
    public class FrameCounter
    {
        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }

        public const int MaximumSamples = 100;

        private readonly Queue<float> sampleBuffer = new();

        public void Update(float deltaTime)
        {
            CurrentFramesPerSecond = 1.0f / deltaTime;

            sampleBuffer.Enqueue(CurrentFramesPerSecond);

            if (sampleBuffer.Count > MaximumSamples)
            {
                sampleBuffer.Dequeue();
                AverageFramesPerSecond = sampleBuffer.Average(i => i);
            }
            else
            {
                AverageFramesPerSecond = CurrentFramesPerSecond;
            }

            TotalFrames++;
            TotalSeconds += deltaTime;
        }

        public void Render(SpriteBatch spriteBatch, Session session, LeftPanel leftPanel)
            => spriteBatch.DrawString(session.UbuntuRegularFont, $"FPS: {(int) AverageFramesPerSecond}", new Vector2((leftPanel != null ? leftPanel.Width : 0f) + 10f, 30f), Color.White);
    }
}
