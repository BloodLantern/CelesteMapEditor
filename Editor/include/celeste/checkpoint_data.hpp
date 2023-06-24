#pragma once

#include <string>
#include <unordered_set>

#include "core_modes.hpp"
#include "player_inventory.hpp"

namespace celeste
{
	struct CheckpointData
	{
        std::string level;
        std::string name;
        bool dreaming;
        int strawberries;
        std::string colorGrade;
        PlayerInventory inventory;
        //AudioState audioState;
        std::unordered_set<std::string> flags;
        CoreMode coreMode;

        CheckpointData() = default;
	};
}
