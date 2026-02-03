# Bonelab Hostile NPC Mod

## Build Instructions

1. Create a **Class Library** project targeting the same runtime your MelonLoader setup uses (commonly `net472`).
2. Add references to the following assemblies from your BONELAB + MelonLoader install:
   - `MelonLoader.dll`
   - `BoneLib.dll`
   - `Il2CppInterop.Runtime.dll`
   - `UnityEngine.CoreModule.dll`
   - `Il2CppSLZ.Interaction.dll`
3. Add the source files from `src/` to the project.
4. Build the project to produce the mod DLL.
5. Copy the resulting DLL and your asset bundle (`ford(pinkspiderhoodiev2).bundle`) into the BONELAB `Mods/` directory.

## Asset Bundle Reference

The mod expects an AssetBundle named `ford(pinkspiderhoodiev2).bundle` containing a prefab named `Ford (PinkSpiderHoodie) UPDATED Variant`.
