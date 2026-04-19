#pragma once

enum class BlendMode {
    Normal = 0,
    Multiply,
    Screen,
    Overlay,
    Add,
    Subtract,
    Difference,
    SoftLight,
    HardLight,
    ColorDodge,
    ColorBurn,
    Exclusion,
    COUNT
};

inline const char* blendModeName(BlendMode mode) {
    switch (mode) {
        case BlendMode::Normal:     return "Normal";
        case BlendMode::Multiply:   return "Multiply";
        case BlendMode::Screen:     return "Screen";
        case BlendMode::Overlay:    return "Overlay";
        case BlendMode::Add:        return "Add";
        case BlendMode::Subtract:   return "Subtract";
        case BlendMode::Difference: return "Difference";
        case BlendMode::SoftLight:  return "Soft Light";
        case BlendMode::HardLight:  return "Hard Light";
        case BlendMode::ColorDodge: return "Color Dodge";
        case BlendMode::ColorBurn:  return "Color Burn";
        case BlendMode::Exclusion:  return "Exclusion";
        default:                    return "Unknown";
    }
}
