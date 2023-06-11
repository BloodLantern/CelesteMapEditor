#include "map.hpp"

#include "logger.hpp"
#include "wind_pattern.hpp"

constexpr const char* const MapFile = "maps/0-Intro.bin";

int main()
{
    editor::Logger::OpenDefaultFile();
    
    celeste::Map mapData(MapFile);

    editor::Logger::Stop();

    return 0;
}
