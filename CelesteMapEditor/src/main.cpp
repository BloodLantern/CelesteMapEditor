#include <iostream>
#include <fstream>
#include <string>
#include <vector>

#include "map_data.hpp"

constexpr const char* XML_FILE = "maps/0-Intro.bin";

int main()
{
    editor::MapData mapData(XML_FILE);

    /*std::ifstream f(XML_FILE, std::ios::binary);

    std::vector<unsigned char> buffer(std::istreambuf_iterator<char>(f), {});
    for (size_t i = 0; i < buffer.size(); i++)
    {
        std::cout << std::hex << (int)buffer[i] << ' ';
    }
    std::cout << std::endl;*/

    return 0;
}
