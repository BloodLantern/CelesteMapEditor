using System.Collections.Generic;

namespace Editor
{
    public class CheckpointData
    {
        public string Level;
        public string Name;
        public bool Dreaming;
        public int Strawberries = 0;
        public string ColorGrade;
        public PlayerInventory Inventory;
        public AudioState AudioState;
        public HashSet<string> Flags;
        public CoreMode CoreMode;

        public CheckpointData(string level, string name, PlayerInventory? inventory = null, bool dreaming = false, AudioState audioState = null)
        {
            Level = level;
            Name = name;
            Dreaming = dreaming;
            Inventory = (PlayerInventory) inventory;
            AudioState = audioState;
        }
    }
}
