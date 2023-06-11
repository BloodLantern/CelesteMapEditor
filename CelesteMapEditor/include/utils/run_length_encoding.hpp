#pragma once

#include <string>

namespace utils
{
    class RunLengthEncoding
    {
    public:
        static std::string Encode(const std::string& str);
        static std::string Decode(const std::string& data);
    };
}