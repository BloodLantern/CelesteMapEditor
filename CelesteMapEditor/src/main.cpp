#include "map.hpp"

constexpr const char* const MapFile = "maps/0-Intro.bin";

int main()
{
    editor::Map mapData(MapFile);

    return 0;
}
