**************************************
*            Voxel Play 2            *
*     by Ramiro Oliva (Kronnect)     * 
*            README FILE             *
**************************************


What's Voxel Play?
--------------------

Voxel Play is a voxelized environment for your game. It aims to provide a complete solution for terrain, sky, water, UI, inventory and character interaction.


How to use this asset
---------------------
Firstly, you should run the Demo scenes to get an idea of the overall functionality.
Then, please take a look at the online documentation to learn how to use all the features that Voxel Play can offer.

Documentation/API reference
---------------------------
The user manual is available online:
https://kronnect.freshdesk.com/support/home

You can find internal development notes in the Documentation folder.


Support
-------
Please read the documentation and browse/play with the demo scene and sample source code included before contacting us for support :-)

* E-mail support: contact@kronnect.com
* Support forum: https://kronnect.com/support
* Twitter: @Kronnect


Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Voxel Play will be eventually available on the Asset Store.


Version history
---------------

Version 10.5.3
- Added Bright Point Lights Max Distance option to Voxel Play Environment inspector
- Added more verbose messages during initialization
- [Fix] Fixed a missing foe prefab reference in demo scene 3

Version 10.5.2
- Added SendMessageOptions.DontRequireReceiver to SendMessage commands when loading/saving a scene to prevent console warnings
- Added "Can Climb" option to first person controller
- Added "Manage Voxel Rotation" to character controller
- [Fix] Save/load game fixes

Version 10.5.1
- API: added "fallbackVoxelDefinition" to load savegame methods (replaces a missing voxel definition from the savegame with an alternate voxel definition)
- Added an inspector error message if Enable URP Support is activated but Universal RP package is not present or configured
- Added support to origin shift to foes in demo scene flat terrain
- [Fix] Fixed character controller position not being applied correctly when loading a saved game
- [Fix] Fixed origin shift regression with first person character controller
- [Fix] Fixed dynamic voxel textures not reflecting all textures when rotating a 6-textured cube

Version 10.5
- Improvements to water placement/destruction in build mode
- Improvements to realistic water appearance on side faces
- [Fix] Fixed custom voxels visibility not being preserved when updating a chunk

Version 10.4
- API: added VoxelGetRotation methods
- Constructor: added tiny delay when returning to focus to prevent accidental clicks
- Constructor: improvements to "Save As New..." option
- [Fix] Constructor: voxel rotations are lost when using the Displace command
- [Fix] Constructor: voxels at z position=0 were not saved correctly
- [Fix] Fixed footfall sounds update failing when character is not grounded

Version 10.3.1
- API: added ChunkReset() method
- [Fix] Fixed water blocks rendering in black in URP when camera background is set to solid color

Version 10.3
- Custom voxels: added "Compute Lighting" option (experimental). This option bakes surrounding lighting and AO into the mesh vertex colors at runtime.
- Internal improvements related to multiple player instances
- DefaultCaveGenerator: added minLength / maxLength properties (length random range for tunnels)
- Improvements to terrain generator and caves
- Improved torch lighting falloff in linear color space
- [Fix] OnGameLoaded event not fired when calling LoadGameFromByteArray
- [Fix] Fixed transparent blocks rendering in black in URP when camera background is set to solid color
- [Fix] Fixed /teleport console command bug
- [Fix] Fixed an error when visible lights exceed 32
- [Fix] Fixed chunk rendering issue when pool is exhausted
- [Fix] Fixed texture bleeding for opaque side textures with solid colors

Version 10.2
- Added debug info when loading connected textures
- Added buoyance effect to particles when underwater (in practice, they fall slower underwater)
- Improved Connected Texture editor visuals
- Added helps section to Voxel Play Environment inspector
- Added menu links to online documentation, youtube tutorials and support forum
- API: improved transition between dynamic voxel to regular voxel using VoxelCancelDynamic method
- API: virtualized methods of character controllers for easier customization
- [Fix] Damage particles now use the textureSample field in voxel definition if present
- [Fix] Voxels were highlighted when highlighting is disabled when using the third person controller

Version 10.1
- Change: chunk.isAboveSurface now defaults to true
- Optimization of the voxel thumbnail generation. New "Drop Voxel Texture Resolution". See: https://kronnect.freshdesk.com/support/solutions/articles/42000001884-world-definition-fields
- UI: removed console message when crouching
- [Fix] First person character controller fixes
- [Fix] Fixes related to the water level transition
- [Fix] Fixed model colors imported with Qubicle when rendering in linear color space

Version 10.0 4/Aug/2021
- Support for URP native lights including point and spot lights with shadows
- Improved underwater effect (fog, caustics) and air to water transition
- Added /fps command to console to toggle fps display on/off
- [Fix] Fixed rogue white pixels on the edges of some voxels visible underground in very dark areas
- [Fix] Fixed an issue with collider rebuild which could led to player falling down
