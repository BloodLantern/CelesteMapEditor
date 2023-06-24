#pragma once

#include "map.hpp"

namespace editor
{
    class Editor
    {
    public:
        static constexpr Editor& GetInstance() { return sInstance; }

        celeste::Map* map = nullptr;

        Editor() = default;

    private:
        static Editor sInstance;
    };
}
