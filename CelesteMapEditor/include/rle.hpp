#pragma once

#include <string>

namespace editor
{
    class RLE
    {
    public:
        static void Encode(const char* const str, unsigned char* result);
        static std::string Decode(const unsigned char* const data, size_t size);
    };
}
