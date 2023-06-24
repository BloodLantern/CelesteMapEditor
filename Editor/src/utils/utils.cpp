#include "utils.hpp"

utils::Rectangle::Rectangle(const vec2i position, const vec2i size)
    : position(position), size(size)
{
}

utils::Rectangle::Rectangle(const int x, const int y, const int width, const int height)
    : position(vec2i(x, y)), size(vec2i(width, height))
{
}
