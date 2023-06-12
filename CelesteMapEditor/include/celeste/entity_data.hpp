#pragma once

#include <string>
#include <unordered_map>
#include <sstream>
#include <vector>
#include <memory>

#include <vector2.hpp>
#include <vector2i.hpp>

namespace celeste
{
    struct LevelData;

    class EntityData
    {
    public:
        int id;
        std::string name;
        LevelData* level = nullptr;
        vec2 position;
        vec2 origin;
        vec2i size;
        std::vector<vec2> nodes;
        std::unordered_map<std::string, std::shared_ptr<void>> values;

        template<typename T>
        T GetAttribute(const std::string& attributeName, const T& defaultValue) const
        {
            if (!values.contains(attributeName))
                return defaultValue;
                
            const T* const value = reinterpret_cast<const T*>(values.at(attributeName).get());
            if (value)
                return *value;
            
            T result;
            std::istringstream(*reinterpret_cast<std::string*>(values.at(attributeName).get())) >> result;
            return result;
        }
    };
}
