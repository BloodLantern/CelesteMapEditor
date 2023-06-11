#include "binary_packer.hpp"

#include <fstream>
#include <iostream>

#include "run_length_encoding.hpp"

#include "logger.hpp"

bool celeste::BinaryPacker::Read(const std::filesystem::path& filePath, Data& result)
{
    std::ifstream file(filePath, std::ios::binary | std::ios::in);

    if (!file.is_open() || !file.good())
    {
        editor::Logger::LogError("Failed to open file: " + filePath.string());
        return false;
    }

    // Skip the 'CELESTE MAP' header
    SkipString(file);

    std::string name;
    ReadString(file, name);
    short metadataLength;
    file.read((char*) &metadataLength, sizeof(metadataLength));
    std::string* metadata = new std::string[metadataLength];
    for (int i = 0; i < metadataLength; i++)
        ReadString(file, metadata[i]);

    ReadData(file, metadata, result);

    delete[] metadata;
    file.close();

    result.package = name;

    return true;
}

int celeste::BinaryPacker::Read7BitEncodedInt(std::istream &file)
{
    int value = 0;
    bool parsed = false;
    for (int step = 0; !parsed; step++)
    {
        unsigned char part = (unsigned char) file.get();
        parsed = (((int) part >> 7) == 0);
        int partCutter = part & 0x7F;
        part = (unsigned char) partCutter;
        value += (int) part << (step*7);
    }

    return value;
}

void celeste::BinaryPacker::ReadString(std::istream &file, std::string& result)
{
    int length = Read7BitEncodedInt(file);
    if (length == 0)
        return;

    result = std::string(length + 1, '\0');
    for (int i = 0; i < length; ++i)
        result[i] = (char) file.get();
}

void celeste::BinaryPacker::SkipString(std::istream &file)
{
    int length = Read7BitEncodedInt(file);
    if (length == 0)
        return;

    file.seekg(length, std::ios::cur);
}

void celeste::BinaryPacker::ReadData(std::istream& file, const std::string* const metadata, Data& result)
{
    short nameOffset;
    file.read((char*) &nameOffset, sizeof(nameOffset));
    result.name = metadata[nameOffset];

    unsigned char valueCount = (unsigned char) file.get();
    for (unsigned char index = 0; index < valueCount; index++)
    {
        short keyOffset;
        file.read((char*) &keyOffset, sizeof(keyOffset));
        std::string key = metadata[keyOffset];

        unsigned char valueType = (unsigned char) file.get();
        DataPair data;
        data.type = (valueType == DataType::String + 1 || valueType == DataType::String + 2) ? DataType::String : (DataType) valueType;
        switch (valueType)
        {
            case DataType::Bool:
                data.value = new bool((bool) file.get());
                break;

            // Byte / unsigned char
            case DataType::UInt8:
                data.value = new unsigned char((unsigned char) file.get());
                break;

            // Short
            case DataType::Int16:
            {
                short s;
                file.read((char*) &s, sizeof(s));
                data.value = new short((short) s);
                break;
            }

            // Int
            case DataType::Int32:
            {
                int i;
                file.read((char*) &i, sizeof(i));
                data.value = new int(i);
                break;
            }

            // Float
            case DataType::Float32:
            {
                float f;
                file.read((char*) &f, sizeof(f));
                data.value = new float(f);
                break;
            }

            // Metadata string
            case DataType::String:
            {
                short s;
                file.read((char*) &s, sizeof(s));
                data.value = new std::string(metadata[s]);
                break;
            }

            // Raw string
            case DataType::String + 1:
            {
                std::string* str = new std::string;
                ReadString(file, *str);
                data.value = str;
                break;
            }

            // RLE-encoded string
            case DataType::String + 2:
            {
                short count;
                file.read((char*) &count, sizeof(count));
                char* bytes = new char[count];
                file.read((char*) bytes, count);
                data.value = new std::string(utils::RunLengthEncoding::Decode(std::string(bytes)));
                delete[] bytes;
                break;
            }
        }

        result.attributes[key] = data;
    }

    short childCount;
    file.read((char*) &childCount, sizeof(childCount));
    for (short i = 0; i < childCount; i++)
    {
        Data* child = new Data;
        ReadData(file, metadata, *child);
        result.children.push_back(child);
    }
}

void celeste::BinaryPacker::DataPair::GetValue(void *out)
{
    switch (type)
    {
        case DataType::Bool:
            *(bool*) out = *(bool*) value;
            break;

        case DataType::UInt8:
            *(unsigned char*) out = *(unsigned char*) value;
            break;

        case DataType::Int16:
            *(short*) out = *(short*) value;
            break;

        case DataType::Int32:
            *(int*) out = *(int*) value;
            break;

        case DataType::Float32:
            *(float*) out = *(float*) value;
            break;

        case DataType::String:
            *(std::string*) out = *(std::string*) value;
            break;
    }
}

celeste::BinaryPacker::Data::~Data()
{
    for (auto& pair : attributes)
        delete pair.second.value;
    for (Data* child : children)
        delete child;
}
