#include "map.hpp"

#include <iostream>

celeste::Map::Map(const std::filesystem::path& path)
    : filePath(path)
{
    Load(path);
}

celeste::Map::~Map()
{
}

void celeste::Map::Reload()
{
    Load(filePath);
}

void Print(celeste::BinaryPacker::Data* data, int depth)
{
    std::string tabs;
    for (int i = 0; i < depth; i++)
        tabs += '\t';

    std::cout << tabs << "Package: " << data->package << std::endl;
    std::cout << tabs << "Name: " << data->name << std::endl;

    std::cout << tabs << "Attributes (" << data->attributes.size() << "):" << std::endl;
    for (auto& dataPair : data->attributes)
    {
        celeste::BinaryPacker::DataPair& pair = dataPair.second;
        std::cout << tabs << '\t' << dataPair.first << ": ";

        switch (pair.type)
        {
            case celeste::BinaryPacker::DataType::Bool:
                bool bValue;
                pair.GetValue(&bValue);
                std::cout << std::boolalpha << bValue << std::endl;
                break;

            case celeste::BinaryPacker::DataType::UInt8:
            case celeste::BinaryPacker::DataType::Int16:
            case celeste::BinaryPacker::DataType::Int32:
                int iValue;
                pair.GetValue(&iValue);
                std::cout << iValue << std::endl;
                break;

            case celeste::BinaryPacker::DataType::Float32:
                float fValue;
                pair.GetValue(&fValue);
                std::cout << fValue << std::endl;
                break;

            case celeste::BinaryPacker::DataType::String:
                std::string sValue;
                pair.GetValue(&sValue);
                std::cout << sValue << std::endl;
                break;
        }
    }

    std::cout << tabs << "Children (" << data->children.size() << "):" << std::endl;
    for (size_t i = 0; i < data->children.size(); i++)
        Print(data->children[i], depth + 1);

    std::cout << std::endl;
}

void celeste::Map::Load(const std::filesystem::path& path)
{
    BinaryPacker::Data* mapData = new BinaryPacker::Data;
    
    if (!BinaryPacker::Read(path, *mapData))
    {
        delete mapData;
        return;
    }

    Print(mapData, 0);

    delete mapData;
}
