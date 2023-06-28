using System;
using System.Collections.Generic;
namespace Editor
{
    public class AudioTrackState
    {
        /// <summary>
        /// Private event field.
        /// </summary>
        private string evt;
        public List<MultiExpressionProgramming> Parameters = new();

        public string Event
        {
            get => evt;
            set
            {
                if (evt == value)
                    return;
                evt = value;
                Parameters.Clear();
            }
        }

        public int Progress
        {
            get => (int)GetParam("progress");
            set => Param("progress", value);
        }

        public AudioTrackState()
        {
        }

        public AudioTrackState(string ev) => Event = ev;

        public AudioTrackState Layer(int index, float value) => Param(AudioState.LayerParameters[index], value);

        public AudioTrackState Layer(int index, bool value) => Param(AudioState.LayerParameters[index], value);

        public AudioTrackState SetProgress(int value)
        {
            Progress = value;
            return this;
        }

        public AudioTrackState Param(string key, float value)
        {
            foreach (MultiExpressionProgramming parameter in Parameters)
            {
                if (parameter.Key != null && parameter.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    parameter.Value = value;
                    return this;
                }
            }
            Parameters.Add(new MultiExpressionProgramming(key, value));

            return this;
        }

        public AudioTrackState Param(string key, bool value) => Param(key, value ? 1f : 0f);

        public float GetParam(string key)
        {
            foreach (MultiExpressionProgramming parameter in Parameters)
            {
                if (parameter.Key != null && parameter.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    return parameter.Value;
            }

            return 0f;
        }

        public AudioTrackState Clone()
        {
            AudioTrackState result = new()
            {
                Event = Event
            };

            foreach (MultiExpressionProgramming parameter in Parameters)
                result.Parameters.Add(new MultiExpressionProgramming(parameter.Key, parameter.Value));

            return result;
        }
    }
}
