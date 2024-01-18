using Editor.Logging;
using System.Data;
using System.IO;

namespace Editor.Celeste
{
    public struct AreaKey
    {
        public static readonly AreaKey None = new(-1);
        public static readonly AreaKey Default = new();

        public const string DefaultCampaign = "Uncategorized";
        public const string VanillaCampaign = "Celeste";

        public int Id = 0;
        public AreaMode Mode = AreaMode.Normal;
        public string Campaign = DefaultCampaign;

        public AreaKey(int id, AreaMode mode = AreaMode.Normal, string campaign = DefaultCampaign)
        {
            Id = id;
            Mode = mode;
            Campaign = campaign;
        }

        /// <summary>
        /// Tries to load the <see cref="Id"/> and the <see cref="Mode"/> using the map file name.
        /// Also checks if the file is located in <see cref="Session.CelesteContentDirectory"/> and
        /// sets the <see cref="Campaign"/> to the vanilla one if it is the case.
        /// </summary>
        /// <param name="filePath">The map file path to try to load values from.</param>
        public void TryLoadFromFileName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
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

                    Id = int.Parse(prefix[..digitCount]);

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
                if (Id == -1 || suffixIndex == 2)
                {
                    string suffix = fileNameParts[suffixIndex];

                    if (suffix.Length > 1)
                        Logger.Log("Invalid map file name suffix: " + suffix, LogLevel.Warning);

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

            // Check if the map is a vanilla one
            if (Directory.GetParent(filePath)?.Parent?.FullName == Session.Current.CelesteContentDirectory)
                Campaign = VanillaCampaign;
        }

        public override readonly bool Equals(object obj) => obj is AreaKey && this == obj as AreaKey?;

        public override readonly int GetHashCode() => (int) (Id * 3 + Mode);

        public override readonly string ToString()
        {
            string result = Campaign + '/' + Id;
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
            return a.Id == b.Id && a.Mode == b.Mode && a.Campaign == b.Campaign;
        }

        public static bool operator !=(AreaKey a, AreaKey b)
        {
            return !(a == b);
        }
    }
}
