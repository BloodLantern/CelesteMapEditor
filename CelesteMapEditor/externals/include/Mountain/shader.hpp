#pragma once

#include <string>
#include <filesystem>

#include <GLEW/glew.h>

#include <vector2.hpp>
#include <vector3.hpp>
#include <vector4.hpp>
#include <matrix2x2.hpp>
#include <matrix3x3.hpp>
#include <matrix4x4.hpp>

#include "color.hpp"

namespace mtn
{
    class Shader
    {
    public:
        Shader() = default;
        Shader(const std::filesystem::path& folder, const std::string& name);
        Shader(const std::filesystem::path& folder, const std::string& vertexName, const std::string& fragmentName);
        ~Shader() { glDeleteProgram(mProgram); }

        bool Load(const std::filesystem::path& vertexFile, const std::filesystem::path& fragmentFile);
        bool LoadVertex(const std::filesystem::path& filepath);
        bool LoadFragment(const std::filesystem::path& filepath);
        bool Link();

        void Use();
        void Unuse();

        inline void SetUniform(const std::string& name, const bool value);
        void SetUniform(const std::string& name, const int value);
        void SetUniform(const std::string& name, const float value);
        void SetUniform(const std::string& name, const Vector2 value);
        void SetUniform(const std::string& name, const Vector3& value);
        void SetUniform(const std::string& name, const Vector4& value);
        void SetUniform(const std::string& name, const Colorf& value);
        void SetUniform(const std::string& name, const Matrix2x2& value);
        void SetUniform(const std::string& name, const Matrix3x3& value);
        void SetUniform(const std::string& name, const Matrix4x4& value);

    private:
        enum class ShaderType : unsigned char
        {
            Vertex,
            Fragment
        };

        unsigned int mVertex = 0, mFragment = 0, mProgram = 0;
        std::string mSource;
        bool mBound = false;

        bool LoadShader(const std::filesystem::path& filepath, unsigned int& shader, const ShaderType type);

        // Load shader source
        bool Load(const std::filesystem::path& filepath);
        void Unload();
        void Unload(unsigned int& shader);
        
        inline unsigned int GetUniform(const std::string& name) const { return glGetUniformLocation(mProgram, name.c_str()); }
    };
}
