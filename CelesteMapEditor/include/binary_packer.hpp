#pragma once

#include <map>
#include <vector>
#include <string>

namespace editor
{
    class BinaryPacker
    {
    public:
        struct Data
        {
            std::string Package;
            std::string Name;
            std::map<std::string, void*> Attributes;
            std::vector<Data*> Children;
        };

        BinaryPacker() = delete;

        static Data Read(const char* const filePath);

    private:
        static std::string ReadString(std::istream& file);
        static Data ReadData(std::istream& file, const std::string* const metadata);
    };
}
