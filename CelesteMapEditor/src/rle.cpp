#include "rle.hpp"

void editor::RLE::Encode(const char *const str, unsigned char* result)
{
    // TODO: Implement RLE encoding function
}

std::string editor::RLE::Decode(const unsigned char *const data, size_t size)
{
    std::string result;

    for (size_t i = 0; i < size; i += 2)
        for (unsigned char repeat = 0; repeat < data[i]; repeat++)
            result += data[i + 1];

    return result;
}
