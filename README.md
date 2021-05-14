# PixelArtCamera

> A very simple Unity camera tool that can be used to render pixel art with pixel perfect precision at any resolution.

![alt text](/Cover%20Image.png)

## Asset Store

You can find this tool on the Unity Asset Store [here](http://u3d.as/2xP8)

## How it works

This tool works by rendering the game at the target resolution and then upscale it by the biggest possible integer factor.
This way, the pixel art will always remain pixel perfect with no artifacts.

The source code for the camera script is quite simple and heavily commented, so feel free to dig into it and modify it to suit your needs.

## Pixel Perfect Text

This tool requires TextMeshPro (TMP) in order to achieve pixel perfect text.
In the example scene it is also showed how to set up a TMP font asset.

## UI

Unity UI is not yet supported, since usually games that requires pixel perfect precision should not need auto-scaling ui systems like the one provided by Unity.
These games should be able to build their own ui with just sprites and text meshes.
