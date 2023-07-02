namespace Editor.Celeste
{
    public struct PlayerInventory
    {
        public static readonly PlayerInventory Prologue = new() { Dashes = 0, DreamDash = false };
        public static readonly PlayerInventory Default = new();
        public static readonly PlayerInventory OldSite = new() { DreamDash = false };
        public static readonly PlayerInventory CH6End = new() { Dashes = 2 };
        public static readonly PlayerInventory TheSummit = new() { Dashes = 2, DreamDash = true, Backpack = false };
        public static readonly PlayerInventory Core = new() { Dashes = 2, NoRefills = true };
        public static readonly PlayerInventory Farewell = new() { Backpack = false };

        public int Dashes = 1;
        public bool DreamDash = true;
        public bool Backpack = true;
        public bool NoRefills = false;

        public PlayerInventory()
        {
        }

        public static PlayerInventory Parse(string value)
        {
            return value switch
            {
                "Prologue" => Prologue,
                "Default" => Default,
                "OldSite" => OldSite,
                "CH6End" => CH6End,
                "TheSummit" => TheSummit,
                "Core" => Core,
                "Farewell" => Farewell,
                _ => Default,
            };
        }
    }
}
