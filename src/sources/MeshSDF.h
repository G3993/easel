#pragma once
// MeshSDF — bake a triangle mesh (.obj / .gltf / .glb) into a signed distance
// volume so a particle sim can form, hold and morph between 3D shapes.
//
// The mesh is fitted into a res³ cube (longest axis = 80 % of the cube,
// centred), its surface is sampled into points, a 3D jump-flood pass finds
// the nearest surface sample for every voxel, and the sign comes from the
// sample's triangle normal (pseudo-normal; good enough for closed or nearly
// closed meshes). Distances are in bake cells, negative inside.
#include <string>
#include <vector>

struct MeshSDFResult {
    int res = 0;
    int triangles = 0;
    std::vector<float> sdf;   // res*res*res, x fastest
};

bool bakeMeshSDF(const std::string& path, int res, MeshSDFResult& out, std::string& err);
