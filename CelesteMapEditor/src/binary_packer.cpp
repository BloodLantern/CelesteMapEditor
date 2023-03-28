#include "binary_packer.hpp"

#include <fstream>
#include <iostream>

#include "rle.hpp"

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
    {
        metadata[i] = ReadString(file);
        for (int j = 0; j < metadata[i].size(); j++)
            std::cout << metadata[i][j];
        std::cout << std::endl;
    }

    Data result = ReadData(file, metadata);
    delete[] metadata;
    result.Package = name;
    return result;
}

std::string editor::BinaryPacker::ReadString(std::istream& file)
{
    unsigned char length = (unsigned char) file.get();
    if (length == 0)
        return "";

    std::string result(length + 1, '\0');
    for (unsigned char i = 0; i < length; ++i)
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

        unsigned char value = (unsigned char) file.get();

        void* data;
        switch (value)
        {
            case 0:
                data = new bool((bool)(char) file.get());
                break;

            case 1:
                data = new int(file.get());
                break;

            case 2:
                short s;
                READ_DATA(file, s);
                data = new int((int) s);
                break;

            case 3:
                int i;
                READ_DATA(file, i);
                data = new int(i);
                break;

            case 4:
                float f;
                READ_DATA(file, f);
                data = new float(f);
                break;

            case 5:
                READ_DATA(file, s);
                data = new std::string(metadata[s]);
                break;

            case 6:
                data = new std::string(ReadString(file));
                break;

            case 7:
                short count;
                READ_DATA(file, count);
                unsigned char* bytes = new unsigned char[count];
                file.read((char*) bytes, count);
                data = new std::string(RLE::Decode(bytes, count));
                delete[] bytes;
                break;
        }

        result.Attributes.emplace(key, data);
    }

    short childCount;
    std::streampos filePos = file.tellg();
    READ_DATA(file, childCount);

    for (short i = 0; i < childCount; i++)
        result.Children.push_back(new Data(ReadData(file, metadata)));

    return result;
}
