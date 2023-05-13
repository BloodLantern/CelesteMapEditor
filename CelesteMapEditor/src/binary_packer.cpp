#include "binary_packer.hpp"

#include <fstream>
#include <iostream>

#include <Mountain/run_length_encoding.hpp>

#define READ_DATA(fileStream, address) fileStream.read((char*) &address, sizeof(address))

editor::BinaryPacker::Data editor::BinaryPacker::Read(const char* const filePath)
{
    std::ifstream file(filePath, std::ios::binary | std::ios::in);

    if (!file.good())
        throw std::invalid_argument("Failed to open file");

    // Skip the 'CELESTE MAP' header
    ReadString(file);


    std::string name = ReadString(file);
    short metadataLength;
    READ_DATA(file, metadataLength);
    std::string* metadata = new std::string[metadataLength];
    for (int i = 0; i < metadataLength; i++)
        metadata[i] = ReadString(file);

    Data result = ReadData(file, metadata);

    delete[] metadata;
    file.close();

    result.Package = name;
    return result;
}

int editor::BinaryPacker::Read7BitEncodedInt(std::istream &file)
{
    int value = 0;
    bool parsed = false;
    for (int step = 0; !parsed; step++) {
        unsigned char part = (unsigned char) file.get();
        parsed = (((int) part >> 7) == 0);
        int partCutter = part & 0x7F;
        part = (unsigned char) partCutter;
        value += (int) part << (step*7);
    }

    return value;
}

std::string editor::BinaryPacker::ReadString(std::istream &file)
{
    int length = Read7BitEncodedInt(file);
    if (length == 0)
        return "";

    std::string result(length + 1, '\0');
    for (int i = 0; i < length; ++i)
        result[i] = (char) file.get();
    return result;
}

editor::BinaryPacker::Data editor::BinaryPacker::ReadData(std::istream& file, const std::string* const metadata)
{
    Data result;

    short nameOffset;
    READ_DATA(file, nameOffset);
    result.Name = metadata[nameOffset];

    unsigned char valueCount = (unsigned char) file.get();
    for (unsigned char index = 0; index < valueCount; index++)
    {
        short keyOffset;
        READ_DATA(file, keyOffset);
        std::string key = metadata[keyOffset];

        unsigned char valueType = (unsigned char) file.get();
        DataPair data;
        data.type = (valueType == STRING + 1 || valueType == STRING + 2) ? STRING : (DataType) valueType;
        switch (valueType)
        {
            case BOOL:
                data.value = new bool((bool)(unsigned char) file.get());
                break;

            // Byte / unsigned char
            case UINT8:
                data.value = new int((unsigned char) file.get());
                break;

            // Short
            case INT16:
                short s;
                READ_DATA(file, s);
                data.value = new int((int) s);
                break;

            // Int
            case INT32:
                int i;
                READ_DATA(file, i);
                data.value = new int(i);
                break;

            // Float
            case FLOAT32:
                float f;
                READ_DATA(file, f);
                data.value = new float(f);
                break;

            // Metadata string
            case STRING:
                READ_DATA(file, s);
                data.value = new std::string(metadata[s]);
                break;

            // Raw string
            case STRING + 1:
                data.value = new std::string(ReadString(file));
                break;

            // RLE-encoded string
            case STRING + 2:
                short count;
                READ_DATA(file, count);
                char* bytes = new char[count];
                file.read((char*) bytes, count);
                data.value = new std::string(mountain::RunLengthEncoding::Decode(std::string(bytes)));
                delete[] bytes;
                break;
        }

        result.Attributes.emplace(key, data);
    }

    short childCount;
    READ_DATA(file, childCount);
    for (short i = 0; i < childCount; i++)
        result.Children.push_back(new Data(ReadData(file, metadata)));

    return result;
}

void editor::BinaryPacker::DataPair::GetValue(void *out)
{
    switch (type)
    {
        case BOOL:
            *(bool*)out = *(bool*)value;
            break;

        case UINT8:
        case INT16:
        case INT32:
            *(int*)out = *(int*)value;
            break;

        case FLOAT32:
            *(float*)out = *(float*)value;
            break;

        case STRING:
            *(std::string*)out = *(std::string*)value;
            break;
    }
}
