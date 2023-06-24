#pragma once

#include <string>

namespace celeste
{
    enum class WindPattern : unsigned char
    {
        None,
        Left,
        Right,
        LeftStrong,
        RightStrong,
        LeftOnOff,
        RightOnOff,
        LeftOnOffFast,
        RightOnOffFast,
        Alternating,
        LeftGemsOnly,
        RightCrazy,
        Down,
        Up,
        Space,
    };

    WindPattern GetWindPattern(const std::string& windPattern);
}
