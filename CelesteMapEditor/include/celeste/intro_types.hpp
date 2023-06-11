#pragma once

namespace celeste
{
    enum class IntroTypes : unsigned char
    {
        Transition,
        Respawn,
        WalkInRight,
        WalkInLeft,
        Jump,
        WakeUp,
        Fall,
        TempleMirrorVoid,
        None,
        ThinkForABit,
    };
}
