# One Mesh by ACE STUDIO X

## Documentation Introduction

**One Mesh** is a powerful Unity asset by ACE STUDIO X that simplifies the process of combining skinned meshes and materials. It supports URP and HDRP rendering pipelines and provides a user-friendly interface for seamless integration into your projects.

### Key Features

- Combine skinned meshes and static meshes.
- Combine materials for skinned meshes.
- Standard shader, URP Lit, and HDRP Lit are supported.
- Save combined assets for reuse.
- Optimize performance by reducing draw calls.
- Blend shape support for combining materials.

## Installation

### Importing the Asset

1. Download the One Mesh asset from the Unity Asset Store.
2. Open your Unity project.
3. Install through the Package Manager.

## Getting Started

### Quick Start Guide

1. Import the One Mesh asset into your Unity project.
2. Open the One Mesh tool from the Unity menu: `Tools > One Mesh > One Mesh 2.1`.
3. Add your target objects and configure the settings.
4. Click the "Combine Skinned Meshes" or "Combine Materials" button to combine your assets.

### Basic Usage

- **Combining Skinned Meshes**: Select the target objects and specify the root bone.
- **Combining Materials for URP**: Select the skinned mesh renderers and configure the texture settings.
- **Combining Materials for HDRP**: Select the skinned mesh renderers and configure the texture settings.

## Detailed Usage

For detailed instructions, please check out our YouTube One Mesh tutorial videos (https://youtu.be/jyydD5QsPYc).

## Support

### Getting Help

If you need assistance, you can reach us at:
- **Email**: Adsjk09@gmail.com

### Feedback and Bug Reporting

We welcome your feedback and encourage you to report any bugs. Please contact us using the information above.

## Known Limitations for the 2.1 Version

1. **Complex Models**:
   - **Issue**:In some cases, when combining skinned meshes with the 'Combine Blendshapes' function enabled, the resulting combined skinned mesh may appear in incorrect positions. 
   - **Solution**: Adjust the transforms of the affected skinned mesh game objects before combining. For instance, modify the rotations (X, Y, Z) of the game object and then combine it to see if the result improves. While generally enabling the 'Combine Blendshapes' function can enhance the overall quality of the combined skinned mesh, but it may also cause incorrect positions or animations in some cases. 

2. **Cloth Physics Components and Materials with Different Render Settings**:
   - **Issue**: Some components like Cloth physics won't function correctly after combining the skinned mesh. Hair, fur, or similar materials typically use fade or cutout settings for shaders. These materials should not be combined with others, as they may not render properly.
   - **Solution**: If your skinned mesh renderer includes a Cloth component, do not combine it with other parts to ensure the Cloth component functions as intended.

3. **Custom Shader Support**:
   - **Issue**: The material combine function supports only Main Textures, Metallic Maps, Normal Maps, and Occlusion Maps.
   - **Solution**: Combined atlases for custom shaders are limited to the textures mentioned above.


## License Documentation

**One Mesh by ACE STUDIO X**

**Copyright (c) 2024 ACE STUDIO X. All rights reserved.**

This package is under the Unity Asset Store End User License Agreement. For more details, please refer to the [Asset Store Terms](https://unity.com/legal/as-terms).