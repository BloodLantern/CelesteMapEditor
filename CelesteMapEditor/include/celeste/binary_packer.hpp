#pragma once

#include <unordered_map>
#include <vector>
#include <filesystem>
#include <string>

namespace celeste
{
    class BinaryPacker
    {
    public:
        enum DataType : unsigned char
        {
            Bool,
            UInt8,
            Int16,
            Int32,
            Float32,
            String,
            None = std::numeric_limits<unsigned char>::max()
        };

        struct DataPair
        {
            void* value = nullptr;
            DataType type;

            void GetValue(void* out);
        };

        struct Data
        {
            std::string package;
            std::string name;
            std::unordered_map<std::string, DataPair> attributes;
            std::vector<Data*> children;

            ~Data();
        };

        BinaryPacker() = delete;

        static bool Read(const std::filesystem::path& filePath, Data& result);

    private:
        static int Read7BitEncodedInt(std::istream& file);
        static void ReadString(std::istream& file, std::string& result);
        static void SkipString(std::istream& file);
        static void ReadData(std::istream& file, const std::string* const metadata, Data& result);
    };
}
