#include "texture.hpp"

#include <stb_image.h>
#include <GLFW/glfw3.h>
#include <iostream>

mtn::Texture::Texture(const char *const filepath)
{
}

mtn::Texture::~Texture()
{
    if (data)
        glDeleteTextures(1, &id);
}
