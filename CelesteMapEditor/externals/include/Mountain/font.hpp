#pragma once

#include <string>
#include <map>

#include <vector2i.hpp>

#include <ft2build.h>
#include FT_FREETYPE_H

namespace mtn
{
    class Font
    {
        friend class Draw;

        struct Character
        {
            unsigned int textureID; // ID handle of the glyph texture
            Vector2i size;          // Size of glyph
            Vector2i bearing;       // Offset from baseline to left/top of glyph
            unsigned int advance;   // Offset to advance to next glyph
        };

    public:
        Font() = default;
        Font(std::string name, const int size = 12) { Load(name, size); }
        ~Font() { Unload(); }

        void Load(std::string name, const int size = 12);
        void Unload();

        Character& GetCharacter(const char c) { return mCharacters[c]; }

    private:
        std::map<char, Character> mCharacters;
    };
}
