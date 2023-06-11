#pragma once

#include <vector>
#include <filesystem>

#include "color.hpp"
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
        std::vector<utils::Rectangle> filler;
        std::vector<EntityData> strawberries;
        std::vector<EntityData> goldenberries;
        editor::Color backgroundColor = editor::Color::Black;
        BinaryPacker::Data foreground;
        BinaryPacker::Data background;
        utils::Rectangle bounds;

        std::filesystem::path filePath;

        Map(const std::filesystem::path& path);
        ~Map();

        void Reload();

    private:
        void Load(const std::filesystem::path& path);
    };
}
