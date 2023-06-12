#include "map.hpp"

#include "logger.hpp"

celeste::Map::Map(const std::filesystem::path& path)
    : filePath(path)
{
    Load(path);
}

celeste::Map::~Map()
{
}

void celeste::Map::Reload()
{
    Load(filePath);
}

void celeste::Map::Load(const std::filesystem::path& path)
{
    BinaryPacker::Data* mapData = new BinaryPacker::Data;
    
    if (!BinaryPacker::Read(path, *mapData))
    {
        delete mapData;
        return;
    }

    /*if (mapData->package != modeData.path)
    {
        editor::Logger::LogError("Corrupted map file: " + path.string());
        delete mapData;
        return;
    }*/

    for (const BinaryPacker::Data* const dataPair : mapData->children)
    {
        if (dataPair->name == "levels")
        {
            for (const BinaryPacker::Data* const level : dataPair->children)
            {
                celeste::LevelData levelData(*level);
                
                detectedStrawberries += levelData.strawberries;
                if (levelData.hasGem)
                    detectedRemixNotes = true;
                if (levelData.hasHeartGem)
                    detectedHeartGem = true;

                levels.push_back(levelData);
            }
        }
        else if (dataPair->name == "filler")
        {
            if (!dataPair->children.empty())
                for (const BinaryPacker::Data* const filler : dataPair->children)
                    fillers.push_back(
                        utils::Rectangle(
                            filler->attributes.at("x").Get<int>(),
                            filler->attributes.at("y").Get<int>(),
                            filler->attributes.at("w").Get<int>(),
                            filler->attributes.at("h").Get<int>()
                        )
                    );
        }
        else if (dataPair->name == "Style")
        {
            if (dataPair->HasAttribute("name"))
                backgroundColor = editor::Color(dataPair->GetAttribute<int>("color", 0));

            if (!dataPair->children.empty())
                for (const BinaryPacker::Data* const styleground : dataPair->children)
                {
                    if (styleground->name == "Backgrounds")
                        background = *styleground;
                    else if (styleground->name == "Foregrounds")
                        foreground = *styleground;
                }
        }
    }

    for (const LevelData& levelData : levels)
        for (const EntityData& entityData : levelData.entities)
        {
            if (entityData.name == "strawberry")
                strawberries.push_back(entityData);
            else if (entityData.name == "goldenBerry")
                goldenberries.push_back(entityData);
        }

    int left = std::numeric_limits<int>::max(),
        top = std::numeric_limits<int>::max(),
        right = std::numeric_limits<int>::min(),
        bottom = std::numeric_limits<int>::min();
    
    for (const LevelData& levelData : levels)
    {
        if (levelData.bounds.Left() < left)
            left = levelData.bounds.Left();
        if (levelData.bounds.Top() < top)
            top = levelData.bounds.Top();
        if (levelData.bounds.Right() < right)
            right = levelData.bounds.Right();
        if (levelData.bounds.Bottom() < bottom)
            bottom = levelData.bounds.Bottom();
    }
    
    for (const utils::Rectangle& filler : fillers)
    {
        if (filler.Left() < left)
            left = filler.Left();
        if (filler.Top() < top)
            top = filler.Top();
        if (filler.Right() < right)
            right = filler.Right();
        if (filler.Bottom() < bottom)
            bottom = filler.Bottom();
    }

    const int offset = 64;
    bounds = utils::Rectangle(left - offset, top - offset, right - left + offset * 2, bottom - top + offset * 2);

    modeData.totalStrawberries = 0;
    modeData.startStrawberries = 0;
    modeData.strawberriesByCheckpoint.resize(modeData.checkpoints.size());

    for (auto& checkpoint : modeData.checkpoints)
        checkpoint.strawberries = 0;

    for (const EntityData& strawberry : strawberries)
        if (!strawberry.GetAttribute<bool>("moon", false))
        {
            const int checkpointId = strawberry.GetAttribute<int>("checkpointId", -1);

            if (checkpointId == -1)
            {
                editor::Logger::LogWarning("Strawberry with id %d has no checkpointId", strawberry.id);
                continue;
            }

            modeData.strawberriesByCheckpoint[checkpointId].push_back(strawberry);

            if (checkpointId == 0)
                modeData.startStrawberries++;
            else if (!modeData.checkpoints.empty())
                modeData.checkpoints[checkpointId - 1].strawberries++;

            modeData.totalStrawberries++;
        }

    delete mapData;
}
