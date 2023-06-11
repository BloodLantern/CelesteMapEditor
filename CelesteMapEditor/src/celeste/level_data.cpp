#include "level_data.hpp"

celeste::LevelData::LevelData(const BinaryPacker::Data &data)
{
    for (const auto& pair : data.attributes)
    {
        if (pair.first == "alt_music")
            altMusic = pair.second.Get<std::string>();
        else if (pair.first == "ambience")
            ambience = pair.second.Get<std::string>();
        else if (pair.first == "ambienceProgress")
            ambienceProgress = pair.second.Get<int>();
        else if (pair.first == "c")
            debugColorIndex = pair.second.Get<int>();
        else if (pair.first == "cameraOffsetX")
            cameraOffset.x = pair.second.Get<float>();
        else if (pair.first == "cameraOffsetY")
            cameraOffset.y = pair.second.Get<float>();
        else if (pair.first == "dark")
            dark = pair.second.Get<bool>();
        else if (pair.first == "delayAltMusicFade")
            delayAltMusic = pair.second.Get<bool>();
        else if (pair.first == "disableDownTransition")
            disableDownTransition = pair.second.Get<bool>();
        else if (pair.first == "enforceDashNumber")
            enforceDashNumber = pair.second.Get<int>();
        else if (pair.first == "height")
        {
            bounds.size.y = pair.second.Get<int>();
            if (bounds.size.y == 184)
            {
                bounds.size.y = 180;
            }
        }
        else if (pair.first == "music")
            music = pair.second.Get<std::string>();
        else if (pair.first == "musicLayer1")
            musicLayers[0] = pair.second.Get<bool>() ? 1.f : 0.f;
        else if (pair.first == "musicLayer2")
            musicLayers[1] = pair.second.Get<bool>() ? 1.f : 0.f;
        else if (pair.first == "musicLayer3")
            musicLayers[2] = pair.second.Get<bool>() ? 1.f : 0.f;
        else if (pair.first == "musicLayer4")
            musicLayers[3] = pair.second.Get<bool>() ? 1.f : 0.f;
        else if (pair.first == "musicProgress")
            musicProgress = pair.second.Get<int>();
        else if (pair.first == "name")
            name = pair.second.Get<std::string>().substr(4);
        else if (pair.first == "space")
            space = pair.second.Get<bool>();
        else if (pair.first == "underwater")
            underwater = pair.second.Get<bool>();
        else if (pair.first == "whisper")
            musicWhispers = pair.second.Get<bool>();
        else if (pair.first == "width")
            bounds.size.x = pair.second.Get<int>();
        else if (pair.first == "windPattern")
            windPattern = GetWindPattern(pair.second.Get<std::string>());
        else if (pair.first == "x")
            bounds.position.x = pair.second.Get<int>();
        else if (pair.first == "y")
            bounds.position.x = pair.second.Get<int>();
    }

    for (const BinaryPacker::Data* const child : data.children)
    {
        if (child->name == "entities")
        {
            if (!child->children.empty())
                for (const BinaryPacker::Data* const entity : child->children)
                {
                    if (entity->name == "player")
                        spawns.push_back(vec2((float) bounds.position.x + entity->attributes.at("x").Get<int>(), (float) bounds.position.y + entity->attributes.at("y").Get<int>()));
                    else if (entity->name == "strawberry" || entity->name == "snowberry")
                        strawberries++;
                    else if (entity->name == "shard")
                        hasGem = true;
                    else if (entity->name == "blackGem")
                        hasHeartGem = true;
                    else if (entity->name == "checkpoint")
                        hasCheckpoint = true;
                    
                    if (entity->name != "player")
                    {
                        EntityData playerData;
                        CreateEntityData(*entity, playerData);
                        entities.push_back(playerData);
                    }
                }
        }
        else if (child->name == "triggers")
        {
            if (!child->children.empty())
                for (const BinaryPacker::Data* const trigger : child->children)
                {
                    EntityData triggerData;
                    CreateEntityData(*trigger, triggerData);
                    triggers.push_back(triggerData);
                }
        }
        else if (child->name == "bgdecals")
        {
            if (!child->children.empty())
                for (const BinaryPacker::Data* const decal : child->children)
                    bgDecals.push_back(
                        DecalData
                        {
                            .texture = decal->attributes.at("texture").Get<std::string>(),
                            .position = vec2(decal->attributes.at("x").Get<float>(), decal->attributes.at("y").Get<float>()),
                            .scale = vec2(decal->attributes.at("scaleX").Get<float>(), decal->attributes.at("scaleY").Get<float>())
                        }
                    );
        }
        else if (child->name == "fgdecals")
        {
            if (!child->children.empty())
                for (const BinaryPacker::Data* const decal : child->children)
                    fgDecals.push_back(
                        DecalData
                        {
                            .texture = decal->attributes.at("texture").Get<std::string>(),
                            .position = vec2(decal->attributes.at("x").Get<float>(), decal->attributes.at("y").Get<float>()),
                            .scale = vec2(decal->attributes.at("scaleX").Get<float>(), decal->attributes.at("scaleY").Get<float>())
                        }
                    );
        }
        else if (child->name == "solids")
            solids = child->GetAttribute<std::string>("innerText", "");
        else if (child->name == "bg")
            bg = child->GetAttribute<std::string>("innerText", "");
        else if (child->name == "fgtiles")
            fgTiles = child->GetAttribute<std::string>("innerText", "");
        else if (child->name == "bgtiles")
            bgTiles = child->GetAttribute<std::string>("innerText", "");
        else if (child->name == "objtiles")
            objTiles = child->GetAttribute<std::string>("innerText", "");
    }
    dummy = spawns.size() == 0;
}

void celeste::LevelData::CreateEntityData(const BinaryPacker::Data &entity, EntityData &result)
{
    result = EntityData
    {
        .name = entity.name,
        .level = this
    };

    const size_t nodesSize = entity.children.size();
    if (nodesSize != 0)
        result.nodes = std::make_unique<vec2[]>(nodesSize);

    if (!entity.attributes.empty())
        for (auto& pair : entity.attributes)
        {
            if (pair.first == "id")
                result.id = pair.second.Get<int>();
            else if (pair.first == "x")
                result.position.x = pair.second.Get<float>();
            else if (pair.first == "y")
                result.position.y = pair.second.Get<float>();
            else if (pair.first == "width")
                result.size.x = pair.second.Get<int>();
            else if (pair.first == "height")
                result.size.y = pair.second.Get<int>();
            else if (pair.first == "originX")
                result.origin.x = pair.second.Get<float>();
            else if (pair.first == "originY")
                result.origin.y = pair.second.Get<float>();
            else
                result.values.emplace(pair.first, std::move(pair.second.value));
        }

    if (nodesSize != 0)
        for (int i = 0; i < nodesSize; i++)
            for (auto& pair : entity.children[i]->attributes)
            {
                if (pair.first == "x")
                    result.nodes[i].x = pair.second.Get<float>();
                else if (pair.first == "y")
                    result.nodes[i].y = pair.second.Get<float>();
            }
}
