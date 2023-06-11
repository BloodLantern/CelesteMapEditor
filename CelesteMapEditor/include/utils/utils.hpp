#pragma once

#include <vector2i.hpp>

namespace utils
{
    struct Rectangle
    {
        vec2i position;
        vec2i size;

        Rectangle() = default;
        Rectangle(const vec2i position, const vec2i size);
        Rectangle(const int x, const int y, const int width, const int height);

        inline int Left() const { return position.x; }
        inline int Top() const { return position.y; }
        inline int Right() const { return Left() + size.x; }
        inline int Bottom() const { return Top() + size.y; }
    };
}
