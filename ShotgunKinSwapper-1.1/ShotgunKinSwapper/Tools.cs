using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace ShotgunKinSwapper {

    public class Tools {

        public static void ApplyCustomTexture(tk2dSpriteCollectionData targetSpriteCollection, Texture2D newTexture = null, List<Texture2D> spriteList = null, Shader overrideShader = null) {
            if (targetSpriteCollection != null) {
                if (newTexture != null) {
                    Material[] materials = targetSpriteCollection.materials;
                    Material[] newMaterials = new Material[materials.Length];
                    if (materials != null) {
                        for (int i = 0; i < materials.Length; i++) {
                            newMaterials[i] = materials[i].Copy(newTexture);
                            if (overrideShader) { newMaterials[i].shader = overrideShader; }
                        }
                        targetSpriteCollection.materials = newMaterials;
                        foreach (Material material2 in targetSpriteCollection.materials) {
                            foreach (tk2dSpriteDefinition spriteDefinition in targetSpriteCollection.spriteDefinitions) {
                                if (material2 != null && spriteDefinition.material.name.Equals(material2.name)) {
                                    spriteDefinition.material = material2;
                                    spriteDefinition.materialInst = new Material(material2);
                                    if (overrideShader) {
                                        spriteDefinition.material.shader = overrideShader;
                                        spriteDefinition.materialInst.shader = overrideShader;
                                    }
                                }
                            }
                        }
                    }
                    return;
                } else if (spriteList != null) {
                    RuntimeAtlasPage runtimeAtlasPage = new RuntimeAtlasPage(0, 0, TextureFormat.RGBA32, 2);
                    foreach (Texture2D texture in spriteList) {
                        float Width = (texture.width / 16f);
                        float Height = (texture.height / 16f);
                        tk2dSpriteDefinition spriteData = targetSpriteCollection.GetSpriteDefinition(texture.name);
                        if (spriteData != null) {
                            if (spriteData.boundsDataCenter != Vector3.zero) {
                                RuntimeAtlasSegment runtimeAtlasSegment = runtimeAtlasPage.Pack(texture, false);
                                spriteData.materialInst.mainTexture = runtimeAtlasSegment.texture;
                                spriteData.uvs = runtimeAtlasSegment.uvs;
                                spriteData.extractRegion = true;
                                spriteData.position0 = Vector3.zero;
                                spriteData.position1 = new Vector3(Width, 0, 0);
                                spriteData.position2 = new Vector3(0, Height, 0);
                                spriteData.position3 = new Vector3(Width, Height, 0);
                                spriteData.boundsDataCenter = new Vector2((Width / 2f), (Height / 2f));
                                spriteData.untrimmedBoundsDataCenter = spriteData.boundsDataCenter;
                                spriteData.boundsDataExtents = new Vector2(Width, Height);
                                spriteData.untrimmedBoundsDataExtents = spriteData.boundsDataExtents;
                            } else {
                                ETGMod.ReplaceTexture(spriteData, texture);
                            }
                        }
                    }
                    runtimeAtlasPage.Apply();
                    return;
                } else {
                    return;
                }
            }
        }

        public static Texture2D BytesToTexture(byte[] bytes, string resourceName) {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture2D, bytes);
            texture2D.filterMode = FilterMode.Point;
            texture2D.name = resourceName;
            texture2D.Apply();
            return texture2D;
        }

        public static Texture2D GetTextureFromResource(string texturePath, int Width = 1, int Height = 1) {
            string file = texturePath;
            file = file.Replace("/", ".");
            file = file.Replace("\\", ".");
            byte[] bytes = ExtractEmbeddedResource($"{(ShotgunKinSwapper.ModName)}." + file);
            // byte[] bytes = ExtractEmbeddedResource(file);
            if (bytes == null) {
                ETGModConsole.Log("No bytes found in " + file);
                return null;
            }
            Texture2D texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture, bytes);
            texture.filterMode = FilterMode.Point;

            string name = file.Substring(0, file.LastIndexOf('.'));
            if (name.LastIndexOf('.') >= 0) { name = name.Substring(name.LastIndexOf('.') + 1); }
            texture.name = name;
            return texture;
        }

        public static byte[] ExtractEmbeddedResource(string filename) {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string[] strings = executingAssembly.GetManifestResourceNames();
            // foreach (string name in strings) { ETGModConsole.Log(name, true); }
            byte[] result;
            using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(filename)) {
                bool flag = manifestResourceStream == null;
                if (flag) {
                    result = null;
                } else {
                    byte[] array = new byte[manifestResourceStream.Length];
                    manifestResourceStream.Read(array, 0, array.Length);
                    result = array;
                }
            }
            return result;
        }
    }

    public static class ShotgunKinExtensions {

        public static Material Copy(this Material orig, Texture2D textureOverride = null, Shader shaderOverride = null) {
            Material m_NewMaterial = new Material(orig.shader) {
                name = orig.name,
                shaderKeywords = orig.shaderKeywords,
                globalIlluminationFlags = orig.globalIlluminationFlags,
                enableInstancing = orig.enableInstancing,
                doubleSidedGI = orig.doubleSidedGI,
                mainTextureOffset = orig.mainTextureOffset,
                mainTextureScale = orig.mainTextureScale,
                renderQueue = orig.renderQueue,
                color = orig.color,
                hideFlags = orig.hideFlags
            };
            if (textureOverride != null) {
                m_NewMaterial.mainTexture = textureOverride;
            } else {
                m_NewMaterial.mainTexture = orig.mainTexture;
            }

            if (shaderOverride != null) {
                m_NewMaterial.shader = shaderOverride;
            } else {
                m_NewMaterial.shader = orig.shader;
            }
            return m_NewMaterial;
        }

        public static tk2dSpriteDefinition Copy(this tk2dSpriteDefinition orig) {
            tk2dSpriteDefinition m_newSpriteCollection = new tk2dSpriteDefinition();

            m_newSpriteCollection.boundsDataCenter = orig.boundsDataCenter;
            m_newSpriteCollection.boundsDataExtents = orig.boundsDataExtents;
            m_newSpriteCollection.colliderConvex = orig.colliderConvex;
            m_newSpriteCollection.colliderSmoothSphereCollisions = orig.colliderSmoothSphereCollisions;
            m_newSpriteCollection.colliderType = orig.colliderType;
            m_newSpriteCollection.colliderVertices = orig.colliderVertices;
            m_newSpriteCollection.collisionLayer = orig.collisionLayer;
            m_newSpriteCollection.complexGeometry = orig.complexGeometry;
            m_newSpriteCollection.extractRegion = orig.extractRegion;
            m_newSpriteCollection.flipped = orig.flipped;
            m_newSpriteCollection.indices = orig.indices;
            if (orig.material != null) { m_newSpriteCollection.material = new Material(orig.material); }
            m_newSpriteCollection.materialId = orig.materialId;
            if (orig.materialInst != null) { m_newSpriteCollection.materialInst = new Material(orig.materialInst); }
            m_newSpriteCollection.metadata = orig.metadata;
            m_newSpriteCollection.name = orig.name;
            m_newSpriteCollection.normals = orig.normals;
            m_newSpriteCollection.physicsEngine = orig.physicsEngine;
            m_newSpriteCollection.position0 = orig.position0;
            m_newSpriteCollection.position1 = orig.position1;
            m_newSpriteCollection.position2 = orig.position2;
            m_newSpriteCollection.position3 = orig.position3;
            m_newSpriteCollection.regionH = orig.regionH;
            m_newSpriteCollection.regionW = orig.regionW;
            m_newSpriteCollection.regionX = orig.regionX;
            m_newSpriteCollection.regionY = orig.regionY;
            m_newSpriteCollection.tangents = orig.tangents;
            m_newSpriteCollection.texelSize = orig.texelSize;
            m_newSpriteCollection.untrimmedBoundsDataCenter = orig.untrimmedBoundsDataCenter;
            m_newSpriteCollection.untrimmedBoundsDataExtents = orig.untrimmedBoundsDataExtents;
            m_newSpriteCollection.uvs = orig.uvs;

            return m_newSpriteCollection;
        }
    }
}

