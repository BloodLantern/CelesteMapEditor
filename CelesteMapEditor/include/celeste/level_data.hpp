#pragma once

#include <vector>
#include <Maths/vector2.hpp>

#include "entity_data.hpp"
#include "decal_data.hpp"
#include "wind_pattern.hpp"
#include "calc.hpp"

namespace celeste
{
    struct LevelData
    {
        std::string name;
        bool dummy;
        int strawberries;
        bool hasGem;
        bool hasHeartGem;
        bool hasCheckpoint;
        bool disableDownTransition;
        utils::Rectangle bounds;
        std::vector<EntityData> entities;
        std::vector<EntityData> triggers;
        std::vector<vec2> spawns;
        std::vector<DecalData> fgDecals;
        std::vector<DecalData> bgDecals;
        std::string solids;
        std::string bg;
        std::string fgTiles;
        std::string bgTiles;
        std::string objTiles;
        WindPattern windPattern;
        vec2 cameraOffset;
        bool dark;
        bool underwater;
        bool space;
        std::string music;
        std::string altMusic;
        std::string ambience;
        float musicLayers[4];
        int musicProgress = -1;
        int ambienceProgress = -1;
        bool musicWhispers;
        bool delayAltMusic;
        int enforceDashNumber;
        int editorColorIndex;
    };
}
