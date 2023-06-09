#pragma once

#include <string>
#include <unordered_map>
#include <vector2i.hpp>

#include "level_data.hpp"

namespace celeste
{
    class EntityData
    {
        int id;
        std::string name;
        LevelData level;
        vec2 position;
        vec2 origin;
        vec2i size;
        vec2* nodes;
        std::unordered_map<std::string, void*> values;
    };
}

