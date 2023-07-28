using Editor.Logging;

namespace Editor.Celeste
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

        /// <summary>
        /// Tries to load the area <see cref="ID"/> and <see cref="Mode"/> using the map file name.
        /// </summary>
        /// <param name="fileName">The map file name to try to load values from.</param>
        public void TryLoadFromFileName(string fileName)
        {
            string[] fileNameParts = fileName.Split('-');
            if (fileNameParts.Length > 1)
            {
                int suffixIndex = fileNameParts.Length - 1;
                string prefix = fileNameParts[0];
                if (char.IsNumber(prefix[0]))
                {
                    int digitCount = 1;
                    while (digitCount < prefix.Length && char.IsNumber(prefix[digitCount]))
                        digitCount++;

                    ID = int.Parse(prefix[..digitCount]);

                    if (digitCount < prefix.Length)
                    {
                        if (prefix.Length - digitCount > 1) // If there are too many characters after the digits (there should be at most 1)
                            Logger.Log("Invalid map file name prefix: " + prefix, LogLevel.Warning);

                        switch (prefix[digitCount])
                        {
                            case 'H':
                                Mode = AreaMode.BSide;
                                break;
                            case 'X':
                                Mode = AreaMode.CSide;
                                break;
                            default:
                                break;
                        };
                    }
                }
                else
                    suffixIndex = 1;

                // If there was no prefix or if there was one but there is also a suffix
                if (ID == -1 || suffixIndex == 2)
                {
                    string suffix = fileNameParts[suffixIndex];

                    if (suffix.Length > 1)
                        Logger.Log("Invalid map file name suffix: " + suffix);

                    switch (suffix[0])
                    {
                        case 'B':
                            Mode = AreaMode.BSide;
                            break;
                        case 'C':
                            Mode = AreaMode.CSide;
                            break;
                        default:
                            break;
                    };
                }
            }
        }

        public override readonly bool Equals(object obj) => obj is AreaKey && this == obj as AreaKey?;

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
