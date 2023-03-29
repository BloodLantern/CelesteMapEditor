#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <typeinfo>

#include "map_data.hpp"
#include "binary_packer.hpp"

constexpr const char* const XML_FILE = "maps/0-Intro.bin";

int main()
{
    editor::MapData mapData(XML_FILE);

    return 0;
}
