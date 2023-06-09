#pragma once

#include <vector>
#include <Mountain/color.hpp>

#include "level_data.hpp"
#include "entity_data.hpp"
#include "binary_packer.hpp"
#include "mode_properties.hpp"
#include "area_key.hpp"
#include "area_data.hpp"

namespace celeste
{
    class Map
    {
    public:
        AreaKey area;
        AreaData data;
        ModeProperties modeData;
        int detectedStrawberries;
        bool detectedRemixNotes;
        bool detectedHeartGem;
        std::vector<LevelData> levels;
        std::vector<Rectangle> filler;
        std::vector<EntityData> strawberries;
        std::vector<EntityData> goldenberries;
        mountain::Color backgroundColor = mountain::Color::Black;
        BinaryPacker::Data foreground;
        BinaryPacker::Data background;
        Rectangle bounds;
        BinaryPacker::Data data;

        Map(const std::string& filePath);
        ~Map();

    private:
        void Load(const std::string& filePath);
    };
}
