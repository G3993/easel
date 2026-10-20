#pragma once
#include "sources/ContentSource.h"
#include "sources/AudioBinding.h"
#include "scene/SceneParams.h"
#include <map>
#include <string>

// The zone's 3D scene on the SHOW surface. Not a picture: a ContentSource
// adapter with no texture that exposes the scene's bindable params (camera,
// sun, point lights) and its AudioBinding map, so the knob engine
// (showParamsFor / showBindingsFor / autoMapShaderToLCXL) treats "3D Scene ·
// <zone>" like any shader or fluid extra. The binding values are applied by
// SceneParams::applyAudioBindings in the per-zone loop, MIDI included.
class SceneKnobSource : public ContentSource {
public:
    explicit SceneKnobSource(SceneParams* s) : scene(s) {}
    SceneParams* scene = nullptr;

    GLuint textureId() const override { return 0; }
    int width() const override  { return 0; }
    int height() const override { return 0; }
    std::string typeName() const override { return "3D Scene"; }
    std::string sourcePath() const override { return "__scene__"; }
    std::map<std::string, AudioBinding>& audioBindings() { return scene->audioBindings; }
};
