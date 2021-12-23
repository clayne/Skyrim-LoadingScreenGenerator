using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nifly;
using System.IO;
using LoadScreenGen.Settings;

namespace LoadScreenGen {

    public static class MeshGen {
        const double sourceUpperWidth = 45.5;
        const double sourceLowerWidth = 1.1;
        const double sourceHeightOffset = 1.0;
        const double sourceHeight = 29.0;
        const double sourceOffsetX = 2.5;
        const double sourceOffsetY = 0.65;
        const double sourceRatio = 1.6;

        static double heightFactor = 0;
        static double widthFactor = 0;

        public static void FitToDisplayRatio(double displayRatio, double imageRatio, BorderOption borderOption) {
            // In the first part, the factors are adjusted, so the model fills the entire screen.
            // A width of 1.0 means the entire width of the image is visible on the screen, so width stays at 1.
            // For wider screens (ratioFactor > 1.0), the height is reduced.
            // Likewise for slimmer screens (ratioFactor < 1.0), the height is increased.
            double ratioFactor = displayRatio / sourceRatio;
            double width = 1.0;
            double height = 1.0 / ratioFactor;

            // Now the model fills the entire screen.
            // In order to keep the aspect ratio of the image, the model must be modified.
            // Here, the model only becomes smaller, in order to add black bars.

            if(borderOption != BorderOption.Stretch) {
                if(displayRatio > imageRatio) {
                    if(borderOption == BorderOption.FixedWidth) {
                        height *= displayRatio / imageRatio;
                    } else if(borderOption == BorderOption.FixedHeight) {
                        width = width * imageRatio / displayRatio;
                    } else if(borderOption == BorderOption.Crop) {
                        height = height * displayRatio / imageRatio;
                    } else if(borderOption == BorderOption.Normal) {
                        width = width * imageRatio / displayRatio;
                    }
                } else if(displayRatio < imageRatio) {
                    if(borderOption == BorderOption.FixedWidth) {
                        height = height * displayRatio / imageRatio;
                    } else if(borderOption == BorderOption.FixedHeight) {
                        width = width * imageRatio / displayRatio;
                    } else if(borderOption == BorderOption.Crop) {
                        width = width * imageRatio / displayRatio;
                    } else if(borderOption == BorderOption.Normal) {
                        height = height * displayRatio / imageRatio;
                    }
                }
            }

            // Write result.
            widthFactor = width;
            heightFactor = height;
        }

        public static void CreateMeshes(List<Image> imageList, string targetDirectory, string textureDirectory, string templatePath, double displayRatio, BorderOption borderOption) {
            var templateNif = new NifFile();
            templateNif.Load(templatePath);
            int i = 0;
            foreach(var image in imageList) {
                var imagePath = image.skyrimPath;
                var newNif = new NifFile(templateNif);

                NiShape shape = newNif.GetShapes()[0];
                if(shape != null) {
                    newNif.SetTextureSlot(shape, Path.Combine(textureDirectory, imagePath + ".dds"));
                    FitToDisplayRatio(displayRatio, image.width * 1.0 / image.height, borderOption);

                    var verts = newNif.GetVertsForShape(shape);
                    
                    // Top Left
                    verts[0].x = (float)(sourceOffsetX - sourceUpperWidth * widthFactor);
                    verts[0].y = (float)(sourceOffsetY + sourceHeight * heightFactor - sourceHeightOffset * heightFactor);

                    // Bottom Left
                    verts[1].x = (float)(sourceOffsetX - sourceUpperWidth * widthFactor - sourceLowerWidth * widthFactor * heightFactor);
                    verts[1].y = (float)(sourceOffsetY - sourceHeight * heightFactor - sourceHeightOffset * heightFactor);

                    // Bottom Right
                    verts[2].x = (float)(sourceOffsetX + sourceUpperWidth * widthFactor + sourceLowerWidth * widthFactor * heightFactor);
                    verts[2].y = (float)(sourceOffsetY - sourceHeight * heightFactor - sourceHeightOffset * heightFactor);

                    // Top Right
                    verts[3].x = (float)(sourceOffsetX + sourceUpperWidth * widthFactor);
                    verts[3].y = (float)(sourceOffsetY + sourceHeight * heightFactor - sourceHeightOffset * heightFactor);

                    newNif.SetVertsForShape(shape, verts);
                }
                var savePath = Path.Combine(targetDirectory, image.skyrimPath + ".nif");
                newNif.Save(savePath);
                shape?.Dispose();
                newNif.Dispose();
                i++;
            }
            templateNif.Dispose();
        }
    }
}
