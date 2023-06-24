#define EXPORT __declspec(dllexport)

#include "map.hpp"
#include "editor.hpp"
#include "logger.hpp"

extern "C"
{
    EXPORT void Logger_OpenDefaultFile()
    {
        editor::Logger::OpenDefaultFile();
    }

    EXPORT void Logger_LogInfo(const char* const message)
    {
        editor::Logger::LogInfo(message);
    }

    EXPORT void Map_Load(const char* const filepath)
    {
        celeste::Map* map = editor::Editor::GetInstance().map;
        delete map;
        map = new celeste::Map(filepath);
    }

    EXPORT void Shutdown()
    {
        delete editor::Editor::GetInstance().map;
        editor::Logger::Stop();
    }
}
