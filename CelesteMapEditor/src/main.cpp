#include "map.hpp"
#include "binary_packer.hpp"

constexpr const char* const XML_FILE = "maps/0-Intro.bin";

int main()
{
    editor::Map mapData(XML_FILE);

    return 0;
}
