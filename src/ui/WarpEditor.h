#pragma once
#include "ui/ViewportPanel.h"
#include <vector>
#include <memory>

struct MappingProfile;

class WarpEditor {
public:
    void render(MappingProfile& mapping, bool& maskEditMode,
                std::vector<std::unique_ptr<MappingProfile>>* allMappings = nullptr,
                int activeMappingIndex = 0);

    bool wantsLoadOBJ() const { return m_wantsLoadOBJ; }
    void clearLoadOBJ() { m_wantsLoadOBJ = false; }

private:
    bool m_wantsLoadOBJ = false;
    bool m_renaming = false;
    char m_renameBuf[128] = {};
};
