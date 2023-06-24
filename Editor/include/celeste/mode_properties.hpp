#pragma once

#include "player_inventory.hpp"
#include "checkpoint_data.hpp"

namespace celeste
{
    class Map;

    struct ModeProperties
    {
        std::string poemId;
        std::string path;
        int totalStrawberries;
        int startStrawberries;
        std::vector<std::vector<EntityData>> strawberriesByCheckpoint;
        std::vector<CheckpointData> checkpoints;
        Map* data;
        PlayerInventory inventory;
        //AudioState audioState;
        bool ignoreLevelAudioLayerData;
    };
}
