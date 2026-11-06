#!/usr/bin/env python3
"""Copy every non-system dylib an .app needs into Contents/Frameworks and
rewrite load commands to @executable_path/../Frameworks (dylibbundler-lite).
Usage: bundle_dylibs.py /path/Easel.app"""
import os, shutil, subprocess, sys

app = sys.argv[1]
exe_dir = os.path.join(app, "Contents", "MacOS")
fw = os.path.join(app, "Contents", "Frameworks")
os.makedirs(fw, exist_ok=True)
SYSTEM = ("/usr/lib/", "/System/")

def deps(path):
    out = subprocess.run(["otool", "-L", path], capture_output=True, text=True, check=True).stdout
    lines = out.splitlines()[1:]
    return [l.strip().split(" (")[0] for l in lines if l.strip()]

def rpaths(path):
    out = subprocess.run(["otool", "-l", path], capture_output=True, text=True, check=True).stdout.splitlines()
    rp = []
    for i, l in enumerate(out):
        if "cmd LC_RPATH" in l:
            for j in range(i, min(i + 4, len(out))):
                if out[j].strip().startswith("path "):
                    rp.append(out[j].strip().split()[1])
    return rp

def resolve(dep, owner, owner_orig):
    d = os.path.dirname(owner_orig)
    if dep.startswith("@loader_path/"):
        return os.path.realpath(os.path.join(d, dep[len("@loader_path/"):]))
    if dep.startswith("@executable_path/"):
        return None  # already app-relative
    if dep.startswith("@rpath/"):
        for rp in rpaths(owner) + ["/opt/homebrew/lib"]:
            rp = rp.replace("@loader_path", d)
            cand = os.path.join(rp, dep[len("@rpath/"):])
            if os.path.exists(cand):
                return os.path.realpath(cand)
        return None
    return os.path.realpath(dep) if os.path.exists(dep) else None

def tool(*args):
    r = subprocess.run(["install_name_tool", *args], capture_output=True, text=True)
    if r.returncode != 0:
        raise SystemExit("install_name_tool failed: " + " ".join(args) + "\n" + r.stderr)

done = set()         # bundled basenames
queue = []           # (path inside app, original location)
for f in os.listdir(exe_dir):
    p = os.path.join(exe_dir, f)
    if os.path.isfile(p) and (os.access(p, os.X_OK) or f.endswith(".dylib")):
        queue.append((p, p))
exe_main = os.path.join(exe_dir, "Easel")

while queue:
    target, orig = queue.pop()
    os.chmod(target, 0o755)
    in_fw = target.startswith(fw)
    for dep in deps(target):
        if dep.startswith(SYSTEM) or dep.startswith("@executable_path/"):
            continue
        name = os.path.basename(dep)
        if in_fw and name == os.path.basename(target):
            continue  # its own id line
        real = resolve(dep, target, orig)
        if not real:
            continue  # libndi beside the exe (dlopen/@rpath) - left alone
        dst = os.path.join(fw, name)
        if name not in done:
            done.add(name)
            if not os.path.exists(dst):
                shutil.copy2(real, dst)
                os.chmod(dst, 0o755)
                tool("-id", "@rpath/" + name, dst)
                queue.append((dst, real))
        if dep.startswith("@rpath/"):
            continue  # found through the exe's rpath to Frameworks
        # ONE spelling for every reference, or dyld loads a second copy
        # (two libomp runtimes = abort at launch)
        tool("-change", dep, "@rpath/" + name, target)

if "@executable_path/../Frameworks" not in rpaths(exe_main):
    tool("-add_rpath", "@executable_path/../Frameworks", exe_main)
for f in os.listdir(fw):   # libs loaded from libs also search their own folder
    p = os.path.join(fw, f)
    if "@loader_path" not in rpaths(p):
        r = subprocess.run(["install_name_tool", "-add_rpath", "@loader_path", p], capture_output=True, text=True)
print(f"bundled {len(done)} dylibs into {fw}")
