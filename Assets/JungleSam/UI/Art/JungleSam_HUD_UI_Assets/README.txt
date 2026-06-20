Jungle Sam HUD UI Assets

Folders:
- Cleaned: alpha-cut PNGs with practical filenames for Unity import. These were generated from AI images and basic background removal was applied.
- Raw: original generated PNGs, kept as backup.

Recommended Unity import:
- Panel/button/frame/crosshair/icons: Texture Type = Sprite (2D and UI), Mip Maps OFF, Compression = High Quality/None.
- Panel/background/button sprites: set Sprite Mode = Single. Use Image Type = Sliced after setting borders in Sprite Editor.
- UI_Bars_Sheet: Sprite Mode = Multiple, slice manually into four horizontal bars.
- UI_Icons_Combat_Sheet: Sprite Mode = Multiple, Grid by Cell Count 3 x 2.
- UI_Icons_Story_Sheet: Sprite Mode = Multiple, Grid by Cell Count 4 x 2.
- UI_Decor_Sheet: Sprite Mode = Multiple, slice manually or use Automatic.
- UI_Overlay_Scanlines: use as full-screen Image with low alpha, Raycast Target OFF.

Suggested borders:
- UI_Panel_Background: 80 px
- UI_Panel_Frame: 70 px
- UI_Button_Dark / UI_Button_Selected: 90 px left/right, 40 px top/bottom
