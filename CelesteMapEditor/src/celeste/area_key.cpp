#include "area_key.hpp"

const celeste::AreaKey celeste::AreaKey::None = -1;
const celeste::AreaKey celeste::AreaKey::Default = 0;

celeste::AreaKey::AreaKey(const int id, const AreaMode mode, const std::string campaign)
    : id(id), mode(mode), campaign(campaign)
{
}

celeste::AreaKey::operator std::string()
{
    std::string result = campaign + ":" + std::to_string(id);
    switch (mode)
    {
        case AreaMode::Normal:
            result += "A";
            break;

        case AreaMode::BSide:
            result += "B";
            break;

        case AreaMode::CSide:
            result += "C";
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
