# OpenSAE Symbol Art Editor
OpenSAE (Open Symbol Art Editor) is an editor and generator for the pseudo-vector symbol art graphics supported by the game Phantasy Star Online 2/PSO2 NGS.
![OpenSAE screenshot](https://github.com/kodemum/OpenSAE/assets/72192505/e51ca17c-c309-4c71-acd2-c15467f17915)

## Features
* Reading/writing game .SAR file format as well as the .SAML format used by the other available editors.
* Full-fledged editor supporting groups, rotation, duplications, bulk color adjustment and more.
* Conversion of any bitmap image into symbol arts using image segmentation analysis.
* Browser for viewing all symbol arts in a directory.
* Backup manager for easily saving specific symbol arts from a game directory.

## Bitmap image conversion
OpenSAE supports converting any specified bitmap image (JPG, PNG, etc.) into a symbol art using a range of conversion options to get the best result.
![OpenSAE bitmap converter](https://github.com/kodemum/OpenSAE/assets/72192505/3c29e639-d483-4bad-bc46-c9f0210893e6)
*NOTE*: Not all bitmaps can be converted with good results depending on the level of detail they contain; some settings work better than others for specific images.

## Technical information
* OpenSAE is written in C#/.NET Core 6.0.
* The user interface is written using WPF.
* The installer is MSI based using WiX for Windows Installer and creates shortcuts and file associations, but the app will run as a portable app too.

## Third-partly libraries
* Geometrize (bitmap image segmentation): https://github.com/Tw1ddle/geometrize
* ImageSharp (bitmap image processing): https://github.com/SixLabors/ImageSharp
* WiX Toolset (installer): https://wixtoolset.org
* PixiEditor.ColorPicker: https://github.com/PixiEditor/ColorPicker
* Microsoft.WindowsAPICodePack.Shell
* CommunityToolkit.Mvvm
