#include "area_key.hpp"

const celeste::AreaKey celeste::AreaKey::None = -1;
const celeste::AreaKey celeste::AreaKey::Default = 0;

celeste::AreaKey::AreaKey(const int id, const AreaMode mode)
    : id(id), mode(mode)
{
}

celeste::AreaKey::operator std::string()
{
    std::string result = std::to_string(id);
    switch (mode)
    {
        case AreaMode::BSide:
            result += "H";
            break;

        case AreaMode::CSide:
            result += "HH";
            break;
    }
    return result;
}

bool celeste::operator==(AreaKey left, AreaKey right)
{
    return left.id == right.id && left.mode == right.mode;
}

bool celeste::operator!=(AreaKey left, AreaKey right)
{
    return !(left == right);
}
