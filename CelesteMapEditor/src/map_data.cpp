#include "map_data.hpp"

#include <iostream>

#include "binary_packer.hpp"

editor::MapData::MapData(const char* const filePath)
{
    Load(filePath);
}

void Print(editor::BinaryPacker::Data* data, int depth)
{
    std::string tabs;
    for (int i = 0; i < depth; i++)
        tabs += '\t';

    std::cout << tabs << "Package: " << data->Package << std::endl;
    std::cout << tabs << "Name: " << data->Name << std::endl;

    std::cout << tabs << "Attributes (" << data->Attributes.size() << "):" << std::endl;
    for (std::map<std::string, editor::BinaryPacker::DataPair>::iterator it = data->Attributes.begin(); it != data->Attributes.end(); it++)
    {
        editor::BinaryPacker::DataPair& pair = it->second;
        std::cout << tabs << '\t' << it->first << ": ";

        switch (pair.type)
        {
            case editor::BinaryPacker::BOOL:
                bool bValue;
                pair.GetValue(&bValue);
                std::cout << std::boolalpha << bValue << std::endl;
                break;

            case editor::BinaryPacker::UINT8:
            case editor::BinaryPacker::INT16:
            case editor::BinaryPacker::INT32:
                int iValue;
                pair.GetValue(&iValue);
                std::cout << iValue << std::endl;
                break;

            case editor::BinaryPacker::FLOAT32:
                float fValue;
                pair.GetValue(&fValue);
                std::cout << fValue << std::endl;
                break;

            case editor::BinaryPacker::STRING:
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

void editor::MapData::Load(const char* const filePath)
{
    BinaryPacker::Data data;
    try
    {
        data = BinaryPacker::Read(filePath);
    }
    catch (const std::invalid_argument&)
    {
        return;
    }

    Print(&data, 0);
    
    for (size_t i = 0; i < data.Children.size(); i++)
        delete data.Children[i];
    for (std::map<std::string, editor::BinaryPacker::DataPair>::iterator it = data.Attributes.begin(); it != data.Attributes.end(); it++)
        delete it->second.value;
}
