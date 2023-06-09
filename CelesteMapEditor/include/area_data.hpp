#pragma once

#include <string>
#include <Mountain/color.hpp>

#include "core_modes.hpp"
#include "mode_properties.hpp"
#include "intro_types.hpp"

namespace celeste
{
    struct AreaData
    {
        std::string name;
        std::string icon;
        bool dreaming;
        std::string completeScreenName;
        ModeProperties* mode;
        int cassetteCheckpointIndex = -1;
        mountain::Color titleBaseColor = mountain::Color::White;
        mountain::Color titleAccentColor = mountain::Color::Gray;
        mountain::Color titleTextColor = mountain::Color::White;
        IntroTypes introType;
        std::string colorGrade;
        std::string wipe;
        float darknessAlpha = 0.05f;
        float bloomBase;
        float bloomStrength = 1;
        std::string jumpthru = "wood";
        std::string spike = "default";
        std::string crumbleBlock = "default";
        std::string woodPlatform = "default";
        mountain::Color cassetteNoteColor = mountain::Color::White;
        /*mountain::Color* cobwebColor = mountain::Color[1]
        {
            mountain::Color(0x696A6AFF);
        };*/
        std::string CassetteSong = "event:/music/cassette/01_forsaken_city";
        CoreMode coreMode;

        AreaData() = default;
    };
}
