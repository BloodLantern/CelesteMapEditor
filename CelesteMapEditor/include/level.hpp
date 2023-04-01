#pragma once

#include <vector>
#include <Maths/vector2.hpp>

#include "entity.hpp"
#include "decal.hpp"
#include "calc.hpp"

namespace editor
{
    class Level
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
        std::vector<Entity> Entities;
        std::vector<Entity> Triggers;
        std::vector<Vector2> Spawns;
        std::vector<Decal> FgDecals;
        std::vector<Decal> BgDecals;
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
