﻿using Editor.Extensions;
using Editor.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Editor
{
    public class Loading
    {
        public delegate void LoadingFunc(ref float progressVar, ref string resourceName);

        public Application App;

        private float progress = 0;
        public float Progress => progress;

        private string currentResource = string.Empty;
        public string CurrentResource => currentResource;

        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; } = null;

        public bool Started { get; private set; } = false;
        public bool Ended => EndTime.HasValue;

        private readonly LoadingFunc func;

        public float FadeInDuration = 1f;
        public float FadeOutDuration = 1f;

        public float LoadingCircleRadius = 100f;
        public int LoadingCircleResolution = 100;
        public int LoadingCircleThickness = 5;
        public float LoadingCircleStartingAngle = 3f * MathHelper.PiOver2;
        public string ProgressFormat = "P0";
        public bool ShowRemainingTime = false;
        public bool ShowElapsedTime = false;
        public bool ShowLoadingString = false;
        public string LoadingString = "Loading";

        private TimeSpan? lastTotalGameTime = null;
        private TimeSpan elapsedTime;
        private TimeSpan timeLeft;

        public float DrawAlpha = 0f;

        public event Action OnEnd;

        /// <summary>
        /// Creates a new instance of the <see cref="Loading"/> class, starting the
        /// loading process if <paramref name="startNow"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="func">
        /// The loading function. It can write to the current progress and
        /// current resource params to display the current loading information.
        /// </param>
        /// <param name="startNow">
        /// Whether to start the loading function now or wait for a call to <see cref="StartAsync"/>
        /// </param>
        public Loading(Application app, LoadingFunc func, bool startNow = true)
        {
            App = app;
            this.func = func;
            if (startNow)
                StartAsync();
        }

        /// <summary>
        /// Starts the loading function asynchronously if it wasn't already started.
        /// </summary>
        public async void StartAsync()
        {
            if (Started)
                return;

            Coroutine.Start(FadeRoutine(FadeInDuration, 0f, 1f));

            StartTime = DateTime.Now;
            Started = true;

            await Task.Run(
                () =>
                {
                    Thread.CurrentThread.Name = "Loader Thread";
                    func(ref progress, ref currentResource);
                }
            );
            progress = 1f;

            EndTime = DateTime.Now;

            Coroutine.Start(FadeRoutine(FadeOutDuration, 1f, 0f));

            OnEnd.Invoke();
        }

        public void Render(SpriteBatch spriteBatch, Application app, Session session, GameTime time)
        {
            if (!Started)
                return;

            bool darkStyle = session.Config.UiStyle == ImGuiStyles.Style.Dark;
            Color drawColor = darkStyle ? Color.White : Color.Black;
            Vector2 center = app.WindowSize.ToVector2() / 2;
            Vector2 low = new(center.X, app.WindowSize.Y * 3/4);

            if (Progress >= 1f)
                spriteBatch.DrawCircle(center, LoadingCircleRadius, LoadingCircleResolution, drawColor, LoadingCircleThickness);
            else
                spriteBatch.DrawArc(center, LoadingCircleRadius, LoadingCircleResolution, LoadingCircleStartingAngle, progress * MathHelper.TwoPi, drawColor, LoadingCircleThickness);
            
            string progressString = progress.ToString(ProgressFormat);
            spriteBatch.DrawString(session.UbuntuRegularFont, progressString, center - session.UbuntuRegularFont.MeasureString(progressString) / 2, drawColor);

            if (ShowLoadingString)
            {
                Vector2 loadingStringSize = session.UbuntuRegularFont.MeasureString(LoadingString);
                spriteBatch.DrawString(session.UbuntuRegularFont, LoadingString, low - loadingStringSize / 2f - Vector2.UnitY * loadingStringSize.Y, drawColor);
            }

            Vector2 drawOffset = Vector2.Zero;
            if (currentResource != string.Empty)
            {
                Vector2 currentResourceStringSize = session.UbuntuRegularFont.MeasureString(currentResource);
                spriteBatch.DrawString(session.UbuntuRegularFont, currentResource, low - currentResourceStringSize / 2f, drawColor);
                drawOffset.Y += currentResourceStringSize.Y;
            }

            if (lastTotalGameTime.HasValue && (Calc.OnInterval((float) time.TotalGameTime.TotalSeconds, (float) lastTotalGameTime.Value.TotalSeconds, 1f) || Ended))
            {
                elapsedTime = (EndTime ?? DateTime.Now) - StartTime;
                if (progress > 0f)
                    timeLeft = (DateTime.Now - StartTime) / Progress * (1f - Progress);
            }

            if (ShowRemainingTime && Progress > 0f)
            {
                string remainingTimeString = $"~{timeLeft.TotalSeconds:F0}s left";
                if (ShowElapsedTime)
                    remainingTimeString += $" (~{(elapsedTime + timeLeft).TotalSeconds:F0}s total)";
                Vector2 remainingTimeStringSize = session.UbuntuRegularFont.MeasureString(remainingTimeString);
                spriteBatch.DrawString(session.UbuntuRegularFont, remainingTimeString, low - remainingTimeStringSize / 2f + drawOffset, drawColor);
                drawOffset.Y += remainingTimeStringSize.Y;
            }

            if (ShowElapsedTime)
            {
                string elapsedTimeString = $"{elapsedTime.TotalSeconds:F0}s elapsed";
                Vector2 elapsedTimeStringSize = session.UbuntuRegularFont.MeasureString(elapsedTimeString);
                spriteBatch.DrawString(session.UbuntuRegularFont, elapsedTimeString, low - elapsedTimeStringSize / 2f + drawOffset, drawColor);
                drawOffset.Y += elapsedTimeStringSize.Y;
            }

            lastTotalGameTime = time.TotalGameTime;
        }

        public IEnumerator FadeRoutine(float duration, float fromAlpha, float toAlpha)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                DrawAlpha = Calc.EaseLerp(fromAlpha, toAlpha, timer, duration, Ease.ExpoOut);

                yield return null;
            }

            DrawAlpha = toAlpha;
        }
    }
}
