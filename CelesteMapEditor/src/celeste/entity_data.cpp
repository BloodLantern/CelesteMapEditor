#include "entity_data.hpp"

celeste::EntityData& celeste::EntityData::operator=(EntityData&& other)
{
    id = other.id;
    name = other.name;
    level = other.level;
    position = other.position;
    origin = other.origin;
    size = other.size;
    nodes = std::move(other.nodes);
    for (auto& value : other.values)
        values[value.first] = std::move(value.second);
}
