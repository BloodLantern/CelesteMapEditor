#pragma once

#include <vector>
#include <Mountain/color.hpp>

#include "level.hpp"
#include "entity.hpp"
#include "binary_packer.hpp"

namespace editor
{
    class Map;

    struct ModeProperties
    {
        const char* PoemID;
        const char* Path;
        int TotalStrawberries;
        int StartStrawberries;
        //EntityData StrawberriesByCheckpoint[][];
        //CheckpointData Checkpoints[];
        Map* Data;
        //PlayerInventory Inventory;
        //AudioState AudioState;
        bool IgnoreLevelAudioLayerData;
    };

    class Map
    {
    public:
        //AreaKey Area;
        //AreaData Data;
        ModeProperties ModeData;
        int DetectedStrawberries;
        bool DetectedRemixNotes;
        bool DetectedHeartGem;
        std::vector<Level> Levels = std::vector<Level>();
        std::vector<Rectangle> Filler = std::vector<Rectangle>();
        std::vector<Entity> Strawberries = std::vector<Entity>();
        std::vector<Entity> Goldenberries = std::vector<Entity>();
        mountain::Color BackgroundColor = mountain::ColorBlack;
        /*BinaryPacker.Element Foreground;
        BinaryPacker.Element Background;*/
        Rectangle Bounds;
        BinaryPacker::Data Data;

        Map(const char* const filePath);
        ~Map();

    private:
        void Load(const char* const filePath);
    };
}
