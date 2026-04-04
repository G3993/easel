#include "app/MappingProfile.h"

bool MappingProfile::init() {
    if (!cornerPin.init()) return false;
    if (!meshWarp.init(5, 5)) return false;
    if (!objMeshWarp.init()) return false;
    return true;
}
