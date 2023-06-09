#include "map.hpp"

#include <iostream>

celeste::Map::Map(const std::string& filePath)
{
    Load(filePath);
}

celeste::Map::~Map()
{
    for (size_t i = 0; i < data.Children.size(); i++)
        delete data.Children[i];
    for (auto& pair : data.Attributes)
        delete pair.second.value;
}

void Print(celeste::BinaryPacker::Data* data, int depth)
{
    std::string tabs;
    for (int i = 0; i < depth; i++)
        tabs += '\t';

    std::cout << tabs << "Package: " << data->Package << std::endl;
    std::cout << tabs << "Name: " << data->Name << std::endl;

    std::cout << tabs << "Attributes (" << data->Attributes.size() << "):" << std::endl;
    for (auto& dataPair : data->Attributes)
    {
        celeste::BinaryPacker::DataPair& pair = dataPair.second;
        std::cout << tabs << '\t' << dataPair.first << ": ";

        switch (pair.type)
        {
            case celeste::BinaryPacker::BOOL:
                bool bValue;
                pair.GetValue(&bValue);
                std::cout << std::boolalpha << bValue << std::endl;
                break;

            case celeste::BinaryPacker::UINT8:
            case celeste::BinaryPacker::INT16:
            case celeste::BinaryPacker::INT32:
                int iValue;
                pair.GetValue(&iValue);
                std::cout << iValue << std::endl;
                break;

            case celeste::BinaryPacker::FLOAT32:
                float fValue;
                pair.GetValue(&fValue);
                std::cout << fValue << std::endl;
                break;

            case celeste::BinaryPacker::STRING:
                std::string sValue;
                pair.GetValue(&sValue);
                std::cout << sValue << std::endl;
                break;
        }
    }

    std::cout << tabs << "Children (" << data->Children.size() << "):" << std::endl;
    for (size_t i = 0; i < data->Children.size(); i++)
        Print(data->Children[i], depth + 1);

    std::cout << std::endl;
}

void celeste::Map::Load(const std::string& filePath)
{
    try
    {
        data = BinaryPacker::Read(filePath);
    }
    catch (const std::invalid_argument&)
    {
        return;
    }

    Print(&data, 0);
}
