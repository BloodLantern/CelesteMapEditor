#include "wind_pattern.hpp"

#include "logger.hpp"

celeste::WindPattern celeste::GetWindPattern(const std::string &windPattern)
{
    if (windPattern == "None")
        return WindPattern::None;
    if (windPattern == "Left")
        return WindPattern::Left;
    if (windPattern == "Right")
        return WindPattern::Right;
    if (windPattern == "LeftStrong")
        return WindPattern::LeftStrong;
    if (windPattern == "RightStrong")
        return WindPattern::RightStrong;
    if (windPattern == "LeftOnOff")
        return WindPattern::LeftOnOff;
    if (windPattern == "RightOnOff")
        return WindPattern::RightOnOff;
    if (windPattern == "LeftOnOffFast")
        return WindPattern::LeftOnOffFast;
    if (windPattern == "RightOnOffFast")
        return WindPattern::RightOnOffFast;
    if (windPattern == "Alternating")
        return WindPattern::Alternating;
    if (windPattern == "LeftGemsOnly")
        return WindPattern::LeftGemsOnly;
    if (windPattern == "RightCrazy")
        return WindPattern::RightCrazy;
    if (windPattern == "Down")
        return WindPattern::Down;
    if (windPattern == "Up")
        return WindPattern::Up;
    if (windPattern == "Space")
        return WindPattern::Space;
    editor::Logger::LogWarning("Invalid WindPattern string: %s", windPattern.c_str());
    return WindPattern::None;
}