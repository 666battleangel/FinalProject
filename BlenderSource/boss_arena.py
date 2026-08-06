"""
boss_arena.py
=============
Procedurally builds a low-poly "final boss battlefield" for Unity, inspired by a
glowing cavern: a circular arena floor ringed by jagged rock spires, hanging
stalactites, emissive crystal clusters, a central altar with a floating orb, and
a large glowing portal in the background. Recolored to a purple-grey scheme.

HOW TO RUN
----------
1. Open Blender (3.6 / 4.x), switch to the Scripting workspace.
2. Open this file (or paste it) and press "Run Script".
   -- or from a terminal:  blender --python boss_arena.py
3. Everything is parented under an empty called "BossArena".
4. Flip EXPORT_FBX to True (and set FBX_PATH) to write a Unity-ready .fbx.

DESIGN NOTES
------------
* Low poly: primitives use few segments; every mesh is flat-shaded.
* One material per look, shared across objects -> few Unity materials/draw calls.
* 1 Blender unit == 1 meter, so it drops into Unity at a sensible scale.
* Tweak the CONFIG block below to reshape the whole arena from a few numbers.
"""

import bpy
import bmesh
import math
import random
from mathutils import Vector

# --------------------------------------------------------------------------- #
# CONFIG                                                                       #
# --------------------------------------------------------------------------- #
SEED            = 7           # change for a different layout, same style
ARENA_RADIUS    = 8.0         # radius of the walkable floor
SPIRE_COUNT     = 11          # jagged rock pillars around the rim
STALACTITE_COUNT = 9          # hanging spikes above the arena
CRYSTAL_CLUSTERS = 6          # emissive crystal groups
ADD_LIGHTING    = True        # add a purple key/fill + camera for preview only
EXPORT_FBX      = False       # set True to auto-export for Unity
FBX_PATH        = "//boss_arena.fbx"   # '//' = next to the .blend file

# Purple-grey palette (linear-ish sRGB values in 0..1)
PAL = {
    "rock_dark":   (0.070, 0.060, 0.090),
    "rock_mid":    (0.150, 0.130, 0.185),
    "rock_light":  (0.260, 0.230, 0.320),
    "floor":       (0.120, 0.105, 0.150),
    "floor_rim":   (0.220, 0.190, 0.270),
    "crystal":     (0.420, 0.250, 0.720),
    "glow_purple": (0.560, 0.280, 0.960),
    "glow_soft":   (0.400, 0.300, 0.680),
    "portal":      (0.680, 0.520, 1.000),
}

rng = random.Random(SEED)


# --------------------------------------------------------------------------- #
# HELPERS                                                                      #
# --------------------------------------------------------------------------- #
def clean_scene():
    """Remove everything so re-running is idempotent."""
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in (bpy.data.meshes, bpy.data.materials, bpy.data.lights,
                  bpy.data.cameras):
        for item in list(block):
            if item.users == 0:
                block.remove(item)


def make_material(name, color, roughness=0.9, metallic=0.0,
                  emission_color=None, emission_strength=0.0):
    """Create (or reuse) a simple Principled material, optionally emissive."""
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    if emission_color is not None:
        # Blender 4.x renamed "Emission" -> "Emission Color"
        for key in ("Emission Color", "Emission"):
            if key in bsdf.inputs:
                bsdf.inputs[key].default_value = (*emission_color, 1.0)
                break
        if "Emission Strength" in bsdf.inputs:
            bsdf.inputs["Emission Strength"].default_value = emission_strength
    return mat


def shade_flat(obj):
    """Flat shading via mesh data (avoids operator-context pitfalls)."""
    for poly in obj.data.polygons:
        poly.use_smooth = False


def finalize(obj, mat, parent):
    """Assign material, flat-shade, and parent under the root empty."""
    obj.data.materials.clear()
    obj.data.materials.append(mat)
    shade_flat(obj)
    obj.parent = parent
    return obj


def jitter_verts(obj, amount, only_up=False, top_bias=False):
    """Push vertices around to break up clean primitives into rock."""
    local_rng = random.Random(rng.random())
    zs = [v.co.z for v in obj.data.vertices]
    zmin, zmax = min(zs), max(zs)
    span = (zmax - zmin) or 1.0
    for v in obj.data.vertices:
        # top_bias: displace upper verts more (craggy tips, flat-ish base)
        w = ((v.co.z - zmin) / span) if top_bias else 1.0
        v.co.x += local_rng.uniform(-amount, amount) * w
        v.co.y += local_rng.uniform(-amount, amount) * w
        if not only_up:
            v.co.z += local_rng.uniform(-amount, amount) * w


# --------------------------------------------------------------------------- #
# BUILDERS                                                                     #
# --------------------------------------------------------------------------- #
def build_root():
    root = bpy.data.objects.new("BossArena", None)
    root.empty_display_size = 1.0
    root.empty_display_type = "PLAIN_AXES"
    bpy.context.collection.objects.link(root)
    return root


def build_floor(root):
    mat_floor = make_material("M_Floor", PAL["floor"], roughness=0.85)
    mat_rim   = make_material("M_FloorRim", PAL["floor_rim"], roughness=0.7)
    mat_rune  = make_material("M_Rune", PAL["glow_purple"],
                              emission_color=PAL["glow_purple"],
                              emission_strength=3.0)

    # main disc
    bpy.ops.mesh.primitive_cylinder_add(vertices=14, radius=ARENA_RADIUS,
                                         depth=0.6, location=(0, 0, -0.3))
    disc = bpy.context.active_object
    disc.name = "Arena_Floor"
    finalize(disc, mat_floor, root)

    # raised rim ring
    bpy.ops.mesh.primitive_cylinder_add(vertices=14, radius=ARENA_RADIUS + 0.6,
                                         depth=0.5, location=(0, 0, 0.05))
    rim_outer = bpy.context.active_object
    bpy.ops.mesh.primitive_cylinder_add(vertices=14, radius=ARENA_RADIUS,
                                         depth=0.7, location=(0, 0, 0.05))
    rim_inner = bpy.context.active_object
    # boolean the inner out of the outer to make a ring
    mod = rim_outer.modifiers.new("ring", "BOOLEAN")
    mod.operation = "DIFFERENCE"
    mod.object = rim_inner
    bpy.context.view_layer.objects.active = rim_outer
    bpy.ops.object.modifier_apply(modifier="ring")
    bpy.data.objects.remove(rim_inner, do_unlink=True)
    rim_outer.name = "Arena_Rim"
    finalize(rim_outer, mat_rim, root)

    # glowing rune ring inlaid in the floor
    bpy.ops.mesh.primitive_torus_add(major_radius=ARENA_RADIUS * 0.62,
                                     minor_radius=0.12,
                                     major_segments=32, minor_segments=6,
                                     location=(0, 0, 0.02))
    rune = bpy.context.active_object
    rune.name = "Arena_RuneRing"
    finalize(rune, mat_rune, root)


def build_altar(root):
    mat_stone = make_material("M_AltarStone", PAL["rock_light"], roughness=0.6)
    mat_orb   = make_material("M_Orb", PAL["portal"],
                              emission_color=PAL["glow_purple"],
                              emission_strength=6.0)

    # stepped base
    heights = [(2.4, 0.5, 0.25), (1.7, 0.5, 0.75), (1.1, 0.6, 1.25)]
    for i, (r, d, z) in enumerate(heights):
        bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=r, depth=d,
                                            location=(0, 0, z))
        step = bpy.context.active_object
        step.rotation_euler.z = math.radians(22.5 * i)
        step.name = f"Altar_Step_{i}"
        finalize(step, mat_stone, root)

    # floating boss orb (low-poly icosphere)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=1.0,
                                          location=(0, 0, 3.1))
    orb = bpy.context.active_object
    orb.name = "Altar_Orb"
    finalize(orb, mat_orb, root)

    # jagged shards orbiting the orb
    for i in range(5):
        ang = math.radians(72 * i)
        loc = (math.cos(ang) * 1.7, math.sin(ang) * 1.7, 3.1 + rng.uniform(-0.4, 0.4))
        bpy.ops.mesh.primitive_cone_add(vertices=4, radius1=0.18, radius2=0.0,
                                        depth=0.9, location=loc)
        shard = bpy.context.active_object
        shard.rotation_euler = (math.radians(rng.uniform(-60, 60)),
                                math.radians(rng.uniform(-60, 60)), ang)
        shard.name = f"Altar_Shard_{i}"
        finalize(shard, mat_orb, root)


def build_spires(root):
    mat_dark = make_material("M_RockDark", PAL["rock_dark"], roughness=0.95)
    mat_mid  = make_material("M_RockMid", PAL["rock_mid"], roughness=0.9)
    ring_r = ARENA_RADIUS + 3.5

    for i in range(SPIRE_COUNT):
        ang = (2 * math.pi / SPIRE_COUNT) * i + rng.uniform(-0.12, 0.12)
        r = ring_r + rng.uniform(-1.2, 1.8)
        x, y = math.cos(ang) * r, math.sin(ang) * r
        height = rng.uniform(6.0, 14.0)
        base_r = rng.uniform(1.4, 2.6)

        bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=base_r,
                                        radius2=base_r * 0.15, depth=height,
                                        location=(x, y, height / 2 - 0.5))
        spire = bpy.context.active_object
        jitter_verts(spire, base_r * 0.28, top_bias=True)
        # lean slightly outward from the arena centre
        spire.rotation_euler = (
            math.radians(rng.uniform(-8, 8)),
            math.radians(rng.uniform(-8, 8)),
            rng.uniform(0, math.tau),
        )
        spire.name = f"Spire_{i}"
        finalize(spire, mat_dark if i % 2 else mat_mid, root)


def build_backdrop(root):
    """A few big rock masses behind the arena = cavern wall."""
    mat_dark = make_material("M_RockDark", PAL["rock_dark"], roughness=0.95)
    for i in range(4):
        ang = math.radians(200 + i * 35)
        r = ARENA_RADIUS + 9
        x, y = math.cos(ang) * r, math.sin(ang) * r
        sz = rng.uniform(6, 10)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=sz,
                                              location=(x, y, sz * 0.5))
        mass = bpy.context.active_object
        jitter_verts(mass, sz * 0.22)
        mass.scale = (1.0, 1.0, rng.uniform(1.4, 2.1))
        mass.name = f"Backdrop_{i}"
        finalize(mass, mat_dark, root)


def build_stalactites(root):
    mat_mid = make_material("M_RockMid", PAL["rock_mid"], roughness=0.9)
    for i in range(STALACTITE_COUNT):
        ang = rng.uniform(0, math.tau)
        r = rng.uniform(2.0, ARENA_RADIUS + 2)
        x, y = math.cos(ang) * r, math.sin(ang) * r
        length = rng.uniform(3.0, 7.0)
        top_r = rng.uniform(0.7, 1.4)
        top_z = rng.uniform(13.0, 17.0)
        # wide at top (radius1 base=bottom point), narrow point at bottom:
        bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.05, radius2=top_r,
                                        depth=length,
                                        location=(x, y, top_z - length / 2))
        stal = bpy.context.active_object
        jitter_verts(stal, top_r * 0.2)
        stal.name = f"Stalactite_{i}"
        finalize(stal, mat_mid, root)


def build_crystals(root):
    mat_crystal = make_material("M_Crystal", PAL["crystal"],
                                roughness=0.25, metallic=0.0,
                                emission_color=PAL["glow_purple"],
                                emission_strength=4.0)
    ring_r = ARENA_RADIUS + 1.0
    for c in range(CRYSTAL_CLUSTERS):
        ang = (2 * math.pi / CRYSTAL_CLUSTERS) * c + rng.uniform(-0.25, 0.25)
        cx, cy = math.cos(ang) * ring_r, math.sin(ang) * ring_r
        count = rng.randint(3, 6)
        for k in range(count):
            ox, oy = rng.uniform(-1.0, 1.0), rng.uniform(-1.0, 1.0)
            h = rng.uniform(1.4, 3.4)
            rad = rng.uniform(0.18, 0.45)
            bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=rad,
                                            radius2=rad * 0.35, depth=h,
                                            location=(cx + ox, cy + oy, h / 2))
            cr = bpy.context.active_object
            cr.rotation_euler = (math.radians(rng.uniform(-22, 22)),
                                 math.radians(rng.uniform(-22, 22)),
                                 rng.uniform(0, math.tau))
            cr.name = f"Crystal_{c}_{k}"
            finalize(cr, mat_crystal, root)


def build_portal(root):
    """Large glowing disc + ring in the background (the cavern's light source)."""
    mat_portal = make_material("M_Portal", PAL["portal"],
                               emission_color=PAL["portal"],
                               emission_strength=8.0)
    mat_ring   = make_material("M_PortalRing", PAL["rock_light"], roughness=0.6)

    px, py, pz = -6.0, ARENA_RADIUS + 14, 12.0
    bpy.ops.mesh.primitive_circle_add(vertices=24, radius=5.0, fill_type="NGON",
                                     location=(px, py, pz),
                                     rotation=(math.radians(90), 0, 0))
    disc = bpy.context.active_object
    disc.name = "Portal_Disc"
    finalize(disc, mat_portal, root)

    bpy.ops.mesh.primitive_torus_add(major_radius=5.2, minor_radius=0.5,
                                     major_segments=24, minor_segments=6,
                                     location=(px, py, pz),
                                     rotation=(math.radians(90), 0, 0))
    ring = bpy.context.active_object
    ring.name = "Portal_Ring"
    finalize(ring, mat_ring, root)


# --------------------------------------------------------------------------- #
# PREVIEW LIGHTING / CAMERA  (not needed for Unity, handy in Blender)         #
# --------------------------------------------------------------------------- #
def add_preview_lighting():
    key = bpy.data.lights.new("Key", "SUN")
    key.energy = 1.6
    key.color = (0.72, 0.60, 1.0)
    key_obj = bpy.data.objects.new("KeyLight", key)
    key_obj.rotation_euler = (math.radians(55), math.radians(10), math.radians(45))
    bpy.context.collection.objects.link(key_obj)

    fill = bpy.data.lights.new("Fill", "AREA")
    fill.energy = 400
    fill.color = (0.4, 0.35, 0.7)
    fill.size = 20
    fill_obj = bpy.data.objects.new("FillLight", fill)
    fill_obj.location = (0, -20, 10)
    fill_obj.rotation_euler = (math.radians(60), 0, 0)
    bpy.context.collection.objects.link(fill_obj)

    cam = bpy.data.cameras.new("PreviewCam")
    cam_obj = bpy.data.objects.new("PreviewCam", cam)
    cam_obj.location = (0, -26, 9)
    cam_obj.rotation_euler = (math.radians(78), 0, 0)
    bpy.context.collection.objects.link(cam_obj)
    bpy.context.scene.camera = cam_obj

    world = bpy.context.scene.world
    if world and world.use_nodes:
        bg = world.node_tree.nodes.get("Background")
        if bg:
            bg.inputs[0].default_value = (0.03, 0.025, 0.05, 1.0)
            bg.inputs[1].default_value = 0.4


# --------------------------------------------------------------------------- #
# EXPORT                                                                       #
# --------------------------------------------------------------------------- #
def export_for_unity(root, filepath):
    # apply transforms so Unity gets clean, identity transforms
    bpy.ops.object.select_all(action="DESELECT")
    meshes = [o for o in bpy.data.objects if o.type == "MESH"]
    for o in meshes:
        o.select_set(True)
    if meshes:
        bpy.context.view_layer.objects.active = meshes[0]
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for o in meshes:
        o.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=bpy.path.abspath(filepath),
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        object_types={"MESH", "EMPTY"},
        mesh_smooth_type="FACE",     # keep the low-poly flat shading
        bake_space_transform=True,   # Y-up for Unity
        axis_forward="-Z", axis_up="Y",
        use_mesh_modifiers=True,
    )
    print(f"[boss_arena] Exported FBX -> {bpy.path.abspath(filepath)}")


# --------------------------------------------------------------------------- #
# MAIN                                                                         #
# --------------------------------------------------------------------------- #
def main():
    clean_scene()
    root = build_root()

    build_floor(root)
    build_altar(root)
    build_spires(root)
    build_backdrop(root)
    build_stalactites(root)
    build_crystals(root)
    build_portal(root)

    if ADD_LIGHTING:
        add_preview_lighting()

    if EXPORT_FBX:
        export_for_unity(root, FBX_PATH)

    print("[boss_arena] Done. Everything is parented under 'BossArena'.")


if __name__ == "__main__":
    main()
