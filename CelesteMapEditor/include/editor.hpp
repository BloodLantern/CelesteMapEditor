#pragma once

#include <Mountain/game.hpp>

namespace editor
{
    class Editor : public mountain::Game
    {
    public:
        Editor();
        ~Editor();

        void Initialize() override;
        void Shutdown() override;

        void PreRender() override;
        void PostRender() override;

        void Update() override;
        void Render() override;

    private:
    };
}
