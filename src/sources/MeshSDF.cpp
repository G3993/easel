#include "sources/MeshSDF.h"
#include "tiny_obj_loader.h"
#include <nlohmann/json.hpp>
#include "stb_image.h"
#include "stb_image_write.h"
#define TINYGLTF_NO_INCLUDE_JSON
#define TINYGLTF_NO_INCLUDE_STB_IMAGE
#define TINYGLTF_NO_INCLUDE_STB_IMAGE_WRITE
#include "tiny_gltf.h"
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/quaternion.hpp>
#include <glm/gtc/type_ptr.hpp>
#include <algorithm>
#include <array>
#include <cmath>
#include <filesystem>
#include <iostream>
#include <limits>
#include <random>

namespace {

inline float d2(const glm::vec3& a, const glm::vec3& b) { glm::vec3 d = a - b; return glm::dot(d, d); }

using Tri = std::array<glm::vec3, 3>;

bool loadObjTris(const std::string& path, std::vector<Tri>& tris, std::string& err) {
    tinyobj::ObjReader reader;
    tinyobj::ObjReaderConfig cfg; cfg.triangulate = true;
    if (!reader.ParseFromFile(path, cfg)) { err = reader.Error(); return false; }
    const auto& a = reader.GetAttrib();
    for (const auto& sh : reader.GetShapes()) {
        size_t off = 0;
        for (size_t f = 0; f < sh.mesh.num_face_vertices.size(); f++) {
            int fv = sh.mesh.num_face_vertices[f];
            if (fv == 3) {
                Tri t;
                for (int v = 0; v < 3; v++) {
                    const auto& idx = sh.mesh.indices[off + v];
                    t[v] = glm::vec3(a.vertices[3 * idx.vertex_index + 0], a.vertices[3 * idx.vertex_index + 1], a.vertices[3 * idx.vertex_index + 2]);
                }
                tris.push_back(t);
            }
            off += fv;
        }
    }
    return !tris.empty();
}

template <typename T>
static unsigned readIndex(const unsigned char* p, int comp) {
    (void)comp; return (unsigned)*reinterpret_cast<const T*>(p);
}

void gltfNode(const tinygltf::Model& m, int ni, const glm::mat4& parent, std::vector<Tri>& tris) {
    if (ni < 0 || ni >= (int)m.nodes.size()) return;
    const auto& n = m.nodes[ni];
    glm::mat4 local(1.0f);
    if (n.matrix.size() == 16) {
        float mm[16]; for (int i = 0; i < 16; i++) mm[i] = (float)n.matrix[i];
        local = glm::make_mat4(mm);
    } else {
        glm::mat4 T(1.0f), R(1.0f), S(1.0f);
        if (n.translation.size() == 3) T = glm::translate(glm::mat4(1.0f), glm::vec3((float)n.translation[0], (float)n.translation[1], (float)n.translation[2]));
        if (n.rotation.size() == 4) R = glm::mat4_cast(glm::quat((float)n.rotation[3], (float)n.rotation[0], (float)n.rotation[1], (float)n.rotation[2]));
        if (n.scale.size() == 3) S = glm::scale(glm::mat4(1.0f), glm::vec3((float)n.scale[0], (float)n.scale[1], (float)n.scale[2]));
        local = T * R * S;
    }
    glm::mat4 world = parent * local;
    if (n.mesh >= 0 && n.mesh < (int)m.meshes.size()) {
        for (const auto& prim : m.meshes[n.mesh].primitives) {
            if (prim.mode != TINYGLTF_MODE_TRIANGLES && prim.mode != -1) continue;
            auto it = prim.attributes.find("POSITION");
            if (it == prim.attributes.end()) continue;
            const auto& acc = m.accessors[it->second];
            if (acc.componentType != TINYGLTF_COMPONENT_TYPE_FLOAT || acc.type != TINYGLTF_TYPE_VEC3) continue;
            const auto& bv = m.bufferViews[acc.bufferView];
            const auto& buf = m.buffers[bv.buffer];
            size_t stride = bv.byteStride ? bv.byteStride : sizeof(float) * 3;
            const unsigned char* base = buf.data.data() + bv.byteOffset + acc.byteOffset;
            std::vector<glm::vec3> pos(acc.count);
            for (size_t i = 0; i < acc.count; i++) {
                const float* f = reinterpret_cast<const float*>(base + i * stride);
                pos[i] = glm::vec3(world * glm::vec4(f[0], f[1], f[2], 1.0f));
            }
            std::vector<unsigned> idx;
            if (prim.indices >= 0) {
                const auto& ia = m.accessors[prim.indices];
                const auto& ibv = m.bufferViews[ia.bufferView];
                const auto& ibuf = m.buffers[ibv.buffer];
                const unsigned char* ib = ibuf.data.data() + ibv.byteOffset + ia.byteOffset;
                int cs = (ia.componentType == TINYGLTF_COMPONENT_TYPE_UNSIGNED_BYTE) ? 1 : (ia.componentType == TINYGLTF_COMPONENT_TYPE_UNSIGNED_SHORT) ? 2 : 4;
                size_t is = ibv.byteStride ? ibv.byteStride : cs;
                idx.resize(ia.count);
                for (size_t i = 0; i < ia.count; i++) {
                    const unsigned char* p = ib + i * is;
                    idx[i] = (cs == 1) ? *p : (cs == 2) ? *reinterpret_cast<const uint16_t*>(p) : *reinterpret_cast<const uint32_t*>(p);
                }
            } else {
                idx.resize(pos.size());
                for (size_t i = 0; i < pos.size(); i++) idx[i] = (unsigned)i;
            }
            for (size_t i = 0; i + 2 < idx.size(); i += 3) {
                if (idx[i] >= pos.size() || idx[i + 1] >= pos.size() || idx[i + 2] >= pos.size()) continue;
                tris.push_back({pos[idx[i]], pos[idx[i + 1]], pos[idx[i + 2]]});
            }
        }
    }
    for (int c : n.children) gltfNode(m, c, world, tris);
}

bool loadGltfTris(const std::string& path, std::vector<Tri>& tris, std::string& err) {
    tinygltf::Model model; tinygltf::TinyGLTF loader; std::string warn;
    std::string ext = std::filesystem::path(path).extension().string();
    std::transform(ext.begin(), ext.end(), ext.begin(), ::tolower);
    bool ok = (ext == ".glb") ? loader.LoadBinaryFromFile(&model, &err, &warn, path)
                              : loader.LoadASCIIFromFile(&model, &err, &warn, path);
    if (!ok) return false;
    int si = model.defaultScene >= 0 ? model.defaultScene : 0;
    if (si < (int)model.scenes.size()) {
        for (int root : model.scenes[si].nodes) gltfNode(model, root, glm::mat4(1.0f), tris);
    } else {
        for (int i = 0; i < (int)model.nodes.size(); i++) gltfNode(model, i, glm::mat4(1.0f), tris);
    }
    return !tris.empty();
}

} // namespace

bool bakeMeshSDF(const std::string& path, int res, MeshSDFResult& out, std::string& err) {
    std::vector<Tri> tris;
    std::string ext = std::filesystem::path(path).extension().string();
    std::transform(ext.begin(), ext.end(), ext.begin(), ::tolower);
    bool ok = (ext == ".gltf" || ext == ".glb") ? loadGltfTris(path, tris, err) : loadObjTris(path, tris, err);
    if (!ok) { if (err.empty()) err = "no triangles in " + path; return false; }
    res = std::max(16, std::min(128, res));

    // fit into the cube: longest axis = 80 % of res, centred
    glm::vec3 bmin(std::numeric_limits<float>::max()), bmax(std::numeric_limits<float>::lowest());
    for (const auto& t : tris) for (const auto& v : t) { bmin = glm::min(bmin, v); bmax = glm::max(bmax, v); }
    glm::vec3 c = (bmin + bmax) * 0.5f;
    float ext3 = std::max(bmax.x - bmin.x, std::max(bmax.y - bmin.y, bmax.z - bmin.z));
    if (ext3 < 1e-8f) { err = "degenerate mesh"; return false; }
    float s = 0.8f * (float)res / ext3;
    // model up (+y) → sim up (+z): the sim grid uses z as up, y toward the back
    auto toGrid = [&](const glm::vec3& v) {
        glm::vec3 q = (v - c) * s;
        return glm::vec3(q.x, -q.z, q.y) + glm::vec3((float)res * 0.5f);
    };

    // surface samples: ~4 per cell² of area, at least one per triangle
    std::vector<glm::vec3> pts, nrm;
    std::mt19937 rng(1234);
    std::uniform_real_distribution<float> U(0.0f, 1.0f);
    double totalArea = 0.0;
    for (auto& t : tris) { for (auto& v : t) v = toGrid(v); totalArea += 0.5 * glm::length(glm::cross(t[1] - t[0], t[2] - t[0])); }
    float perArea = 4.0f;
    size_t budget = (size_t)std::min(600000.0, std::max(20000.0, totalArea * perArea));
    perArea = (float)(budget / std::max(totalArea, 1e-6));
    for (const auto& t : tris) {
        glm::vec3 n = glm::cross(t[1] - t[0], t[2] - t[0]);
        float area = 0.5f * glm::length(n);
        if (area < 1e-9f) continue;
        n /= (2.0f * area);
        int k = std::max(1, (int)std::lround(area * perArea));
        for (int i = 0; i < k; i++) {
            float a = U(rng), b = U(rng);
            if (a + b > 1.0f) { a = 1.0f - a; b = 1.0f - b; }
            pts.push_back(t[0] + (t[1] - t[0]) * a + (t[2] - t[0]) * b);
            nrm.push_back(n);
        }
    }
    if (pts.empty()) { err = "no surface samples"; return false; }

    // jump flood: nearest sample per voxel
    const int N = res * res * res;
    std::vector<int> seed(N, -1);
    auto at = [&](int x, int y, int z) { return x + res * (y + res * z); };
    auto vc = [&](int i) { int x = i % res, y = (i / res) % res, z = i / (res * res); return glm::vec3(x + 0.5f, y + 0.5f, z + 0.5f); };
    for (size_t i = 0; i < pts.size(); i++) {
        int x = std::clamp((int)pts[i].x, 0, res - 1), y = std::clamp((int)pts[i].y, 0, res - 1), z = std::clamp((int)pts[i].z, 0, res - 1);
        int v = at(x, y, z);
        if (seed[v] < 0 || glm::distance(vc(v), pts[i]) < glm::distance(vc(v), pts[seed[v]])) seed[v] = (int)i;
    }
    std::vector<int> next(N);
    for (int step = res / 2; step >= 1; step /= 2) {
        for (int z = 0; z < res; z++) for (int y = 0; y < res; y++) for (int x = 0; x < res; x++) {
            int v = at(x, y, z);
            glm::vec3 pc = vc(v);
            int best = seed[v];
            float bd = (best >= 0) ? d2(pc, pts[best]) : std::numeric_limits<float>::max();
            for (int dz = -1; dz <= 1; dz++) for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) {
                if (!dx && !dy && !dz) continue;
                int nx = x + dx * step, ny = y + dy * step, nz = z + dz * step;
                if (nx < 0 || ny < 0 || nz < 0 || nx >= res || ny >= res || nz >= res) continue;
                int sidx = seed[at(nx, ny, nz)];
                if (sidx < 0) continue;
                float d = d2(pc, pts[sidx]);
                if (d < bd) { bd = d; best = sidx; }
            }
            next[v] = best;
        }
        seed.swap(next);
    }
    out.res = res; out.triangles = (int)tris.size();
    out.sdf.assign(N, (float)res);
    for (int v = 0; v < N; v++) {
        int si = seed[v];
        if (si < 0) continue;
        glm::vec3 pc = vc(v);
        glm::vec3 d = pc - pts[si];
        float dist = glm::length(d);
        float sign = (glm::dot(d, nrm[si]) < 0.0f) ? -1.0f : 1.0f;
        out.sdf[v] = dist * sign;
    }
    std::cerr << "[MeshSDF] baked " << path << ": " << tris.size() << " tris, " << pts.size() << " samples, " << res << "^3" << std::endl;
    return true;
}
