#include "map.hpp"

#include "logger.hpp"

constexpr const char* const MapFile = "maps/0-Intro.bin";

int main()
{
    editor::Logger::OpenDefaultFile();
    
    celeste::Map mapData(MapFile);

    return 0;
}
