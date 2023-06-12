#pragma once

#include <unordered_map>
#include <vector>
#include <filesystem>
#include <fstream>
#include <memory>
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

        struct DataValue
        {
            std::shared_ptr<void> value;

            template<typename T>
            inline T Get() const { return *GetPtr<T>(); }
            template<typename T>
            inline T* GetPtr() const { return reinterpret_cast<T*>(value.get()); }
        };

        class Data
        {
        public:
            std::string package;
            std::string name;
            std::unordered_map<std::string, DataValue> attributes;
            std::vector<Data*> children;

            ~Data();

            inline bool HasAttribute(const std::string& attributeName) const
            {
                return attributes.contains(attributeName);
            }

            template<typename T>
            inline T GetAttribute(const std::string& attributeName, const T& defaultValue) const
            {
                if (attributes.empty() || !attributes.contains(attributeName))
                    return defaultValue;
                return attributes.at(attributeName).Get<T>();
            }
        };

        BinaryPacker() = delete;

        static bool Read(const std::filesystem::path& filePath, Data& result);

    private:
        static int Read7BitEncodedInt(std::istream& file);
        static void ReadString(std::ifstream& file, std::string& result);
        static void SkipString(std::ifstream& file);
        static void ReadData(std::ifstream& file, const std::string* const metadata, Data& result);
    };
}
