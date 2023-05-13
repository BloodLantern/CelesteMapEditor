#include "map.hpp"
#include "editor.hpp"

constexpr const char* const XML_FILE = "maps/0-Intro.bin";

int main()
{
    editor::Editor e;
    e.Initialize();
    e.MainLoop();
    e.Shutdown();
    //editor::Map mapData(XML_FILE);

    return 0;
}
