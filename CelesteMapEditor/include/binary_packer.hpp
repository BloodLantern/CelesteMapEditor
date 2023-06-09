#pragma once

#include <map>
#include <vector>
#include <string>

namespace celeste
{
    class BinaryPacker
    {
    public:
        enum DataType : unsigned char
        {
            BOOL,
            UINT8,
            INT16,
            INT32,
            FLOAT32,
            STRING,
            NONE = std::numeric_limits<unsigned char>::max()
        };

        struct DataPair
        {
            void* value;
            DataType type;

            void GetValue(void* out);
        };

        struct Data
        {
            std::string Package;
            std::string Name;
            std::map<std::string, DataPair> Attributes;
            std::vector<Data*> Children;
        };

        BinaryPacker() = delete;

        static Data Read(const std::string& filePath);

    private:
        static int Read7BitEncodedInt(std::istream& file);
        static std::string ReadString(std::istream& file);
        static Data ReadData(std::istream& file, const std::string* const metadata);
    };
}
