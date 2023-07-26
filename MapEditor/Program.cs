using Editor;
using Editor.Logging;
using Microsoft.Xna.Framework;

Logger.AddDefaultLoggingFiles();

using Game game = new MapEditor();
game.Run();

Logger.ClearLoggingFiles();
