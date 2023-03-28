#pragma once

#include <vector>
#include <Mountain/color.hpp>

#include "level_data.hpp"
#include "entity_data.hpp"

namespace editor
{
    class MapData;

    struct ModeProperties
    {
        const char* PoemID;
        const char* Path;
        int TotalStrawberries;
        int StartStrawberries;
        //EntityData StrawberriesByCheckpoint[][];
        //CheckpointData Checkpoints[];
        MapData* Data;
        //PlayerInventory Inventory;
        //AudioState AudioState;
        bool IgnoreLevelAudioLayerData;
    };

    class MapData
    {
    public:
        //AreaKey Area;
        //AreaData Data;
        ModeProperties ModeData;
        int DetectedStrawberries;
        bool DetectedRemixNotes;
        bool DetectedHeartGem;
        std::vector<LevelData> Levels = std::vector<LevelData>();
        std::vector<Rectangle> Filler = std::vector<Rectangle>();
        std::vector<EntityData> Strawberries = std::vector<EntityData>();
        std::vector<EntityData> Goldenberries = std::vector<EntityData>();
        mountain::Color BackgroundColor = mountain::ColorBlack;
        /*BinaryPacker.Element Foreground;
        BinaryPacker.Element Background;*/
        Rectangle Bounds;

        MapData(const char* const filePath);

    private:
        void Load(const char* const filePath);
    };
}
