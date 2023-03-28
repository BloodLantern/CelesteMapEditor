#pragma once

#include <vector>
#include <Maths/vector2.hpp>

#include "entity_data.hpp"
#include "decal_data.hpp"

struct Rectangle
{
    Vector2 position;
    Vector2 size;
};

namespace editor
{
    class LevelData
    {
    public:
        const char* Name;
        bool Dummy;
        int Strawberries;
        bool HasGem;
        bool HasHeartGem;
        bool HasCheckpoint;
        bool DisableDownTransition;
        Rectangle Bounds;
        std::vector<EntityData> Entities;
        std::vector<EntityData> Triggers;
        std::vector<Vector2> Spawns;
        std::vector<DecalData> FgDecals;
        std::vector<DecalData> BgDecals;
        const char* Solids;
        const char* Bg;
        const char* FgTiles;
        const char* BgTiles;
        const char* ObjTiles;
        //WindController.Patterns WindPattern;
        Vector2 CameraOffset;
        bool Dark;
        bool Underwater;
        bool Space;
        const char* Music;
        const char* AltMusic;
        const char* Ambience;
        float MusicLayers[4];
        int MusicProgress = -1;
        int AmbienceProgress = -1;
        bool MusicWhispers;
        bool DelayAltMusic;
        int EnforceDashNumber;
        int EditorColorIndex;
    };
}
