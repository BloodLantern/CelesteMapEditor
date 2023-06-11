#include "color.hpp"

#include <cmath>

#define HUE_ANGLE 43

const editor::Color  editor::Color::White    = Color(0xFF, 0xFF, 0xFF);
const editor::Color  editor::Color::Gray     = Color(0x7F, 0x7F, 0x7F);
const editor::Color  editor::Color::Black    = Color(0x00, 0x00, 0x00);
const editor::Color  editor::Color::Red      = Color(0xFF, 0x00, 0x00);
const editor::Color  editor::Color::Green    = Color(0x00, 0xFF, 0x00);
const editor::Color  editor::Color::Blue     = Color(0x00, 0x00, 0xFF);
const editor::Color  editor::Color::Yellow   = Color(0xFF, 0xFF, 0x00);
const editor::Color  editor::Color::Cyan     = Color(0x00, 0xFF, 0xFF);
const editor::Color  editor::Color::Magenta  = Color(0xFF, 0x00, 0xFF);

const editor::Colorf editor::Colorf::White   = Colorf(1.0f, 1.0f, 1.0f);
const editor::Colorf editor::Colorf::Gray    = Colorf(0.5f, 0.5f, 0.5f);
const editor::Colorf editor::Colorf::Black   = Colorf(0.0f, 0.0f, 0.0f);
const editor::Colorf editor::Colorf::Red     = Colorf(1.0f, 0.0f, 0.0f);
const editor::Colorf editor::Colorf::Green   = Colorf(0.0f, 1.0f, 0.0f);
const editor::Colorf editor::Colorf::Blue    = Colorf(0.0f, 0.0f, 1.0f);
const editor::Colorf editor::Colorf::Yellow  = Colorf(1.0f, 1.0f, 0.0f);
const editor::Colorf editor::Colorf::Cyan    = Colorf(0.0f, 1.0f, 1.0f);
const editor::Colorf editor::Colorf::Magenta = Colorf(1.0f, 0.0f, 1.0f);

editor::Color::operator editor::ColorHSV() const
{
    ColorHSV hsv = { 0, 0, 0, a };
    unsigned char minVal = std::min(std::min(r, g), b);
    unsigned char maxVal = std::max(std::max(r, g), b);
    hsv.v = maxVal;
    unsigned char delta = maxVal - minVal;
    if (delta == 0)
    {
        hsv.h = 0;
        hsv.s = 0;
    }
    else
    {
        hsv.s = 0xFF * delta / maxVal;
        if (r == maxVal)
            hsv.h = 0 + HUE_ANGLE * (g - b) / delta;
        else if (g == maxVal)
            hsv.h = 85 + HUE_ANGLE * (b - r) / delta;
        else
            hsv.h = 171 + HUE_ANGLE * (r - g) / delta;
    }
    return hsv;
}

editor::ColorHSV::operator editor::Color() const
{
    unsigned char hi = h / HUE_ANGLE;
    unsigned char f = h % HUE_ANGLE * 6;
    unsigned char p = (v * (0xFF - s) + 0x7F) / 0xFF;
    unsigned char q = (v * (0xFF - (s * f + 0x7F) / 0xFF) + 0x7F) / 0xFF;
    unsigned char t = (v * (0xFF - (s * (0xFF - f) + 0x7F) / 0xFF) + 0x7F) / 0xFF;

    switch (hi)
    {
    case 0:
        return Color(v, t, p, a);
    case 1:
        return Color(q, v, p, a);
    case 2:
        return Color(p, v, t, a);
    case 3:
        return Color(p, q, v, a);
    case 4:
        return Color(t, p, v, a);
    default:
        return Color(v, p, q, a);
    }
}

editor::Color editor::operator+(const Color c1, const Color c2)
{
    return Color(
        c1.r + c2.r > 0xFF ? 0xFF : c1.r + c2.r,
        c1.g + c2.g > 0xFF ? 0xFF : c1.g + c2.g,
        c1.b + c2.b > 0xFF ? 0xFF : c1.b + c2.b,
        c1.a + c2.a > 0xFF ? 0xFF : c1.a + c2.a
    );
}

editor::Color editor::operator*(const Color c1, const Color c2)
{
    return Color(
        c1.r * c2.r > 0xFF ? 0xFF : c1.r * c2.r,
        c1.g * c2.g > 0xFF ? 0xFF : c1.g * c2.g,
        c1.b * c2.b > 0xFF ? 0xFF : c1.b * c2.b,
        c1.a * c2.a > 0xFF ? 0xFF : c1.a * c2.a
    );
}

editor::Colorf::operator editor::Color() const
{
    return Color((unsigned char) r * 255, (unsigned char) g * 255, (unsigned char) b * 255, (unsigned char) a * 255);
}

editor::Color::operator editor::Colorf() const
{
    return Colorf(r / 255.f, g / 255.f, b / 255.f, a / 255.f);
}

editor::Colorf::operator Vector4() const
{
    return Vector4(r, g, b, a);
}
