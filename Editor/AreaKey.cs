namespace Editor
{
    public struct AreaKey
    {
        public static readonly AreaKey None = new() { ID = -1 };
        public static readonly AreaKey Default = new();

        public int ID = 0;
        public AreaMode Mode = AreaMode.Normal;
        public string Campaign = "Celeste";

        public AreaKey()
        {
        }

        public AreaKey(int id, AreaMode mode = AreaMode.Normal, string campaign = "Uncategorized")
        {
            ID = id;
            Mode = mode;
            Campaign = campaign;
        }

        public override readonly bool Equals(object obj) => false;

        public override readonly int GetHashCode() => (int) (ID * 3 + Mode);

        public override readonly string ToString()
        {
            string result = Campaign + '/' + ID;
            switch (Mode)
            {
                case AreaMode.Normal:
                    result += 'A';
                    break;

                case AreaMode.BSide:
                    result += 'B';
                    break;

                case AreaMode.CSide:
                    result += 'C';
                    break;
            }
            return result;
        }

        public static bool operator ==(AreaKey a, AreaKey b)
        {
            return a.ID == b.ID && a.Mode == b.Mode;
        }

        public static bool operator !=(AreaKey a, AreaKey b)
        {
            return !(a == b);
        }
    }
}
