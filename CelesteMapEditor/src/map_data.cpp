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
    for (std::map<std::string, void*>::iterator it = data->Attributes.begin(); it != data->Attributes.end(); it++)
        std::cout << tabs << "\tKey: " << it->first << ", Value: " << it->second << std::endl;
    std::cout << tabs << "Children (" << data->Children.size() << "):" << std::endl;
    for (size_t i = 0; i < data->Children.size(); i++)
        Print(data->Children[i], depth + 1);
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
    for (std::map<std::string, void*>::iterator it = data.Attributes.begin(); it != data.Attributes.end(); it++)
        delete it->second;
}
