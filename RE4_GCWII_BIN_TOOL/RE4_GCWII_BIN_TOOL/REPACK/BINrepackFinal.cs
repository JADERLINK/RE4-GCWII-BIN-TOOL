using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RE4_GCWII_BIN_TOOL.REPACK.Structures;

namespace RE4_GCWII_BIN_TOOL.REPACK
{
    public static partial class BinRepack
    {
        public static FinalStructure MakeFinalStructure(IntermediaryLevel2 intermediaryStructure) 
        {
            FinalStructure final = new FinalStructure();

            List<(short vx, short vy, short vz, ushort WeightIndex)> Vertex_Position_Array = new List<(short vx, short vy, short vz, ushort WeightIndex)>();
            List<(short nx, short ny, short nz, ushort WeightIndex)> Vertex_Normal_Array = new List<(short nx, short ny, short nz, ushort WeightIndex)>();
            List<(short tu, short tv)> Vertex_UV_Array = new List<(short tu, short tv)>();
            List<(byte a, byte r, byte g, byte b)> Vertex_Color_Array = new List<(byte a, byte r, byte g, byte b)>();

            List<FinalWeightMap> WeightMaps = new List<FinalWeightMap>();

            List<FinalMaterialGroup> Groups = new List<FinalMaterialGroup>();

            foreach (var item in intermediaryStructure.Groups)
            {
                FinalMaterialGroup group = new FinalMaterialGroup();
                group.materialName = item.Key;

                group.Mesh = new FinalFace[item.Value.Faces.Count];

                for (int i = 0; i < item.Value.Faces.Count; i++)
                {
                    FinalFace face = new FinalFace();
                    face.Type = item.Value.Faces[i].Type;

                    List<FinalFaceVextexIndex> vertexIndices = new List<FinalFaceVextexIndex>();

                    for (int iv = 0; iv < item.Value.Faces[i].Vertexs.Count; iv++)
                    {
                        var vertex = item.Value.Faces[i].Vertexs[iv];

                        FinalWeightMap weightMap = vertex.GetFinalWeightMap();

                        if (!WeightMaps.Contains(weightMap))
                        {
                            WeightMaps.Add(weightMap);
                        }

                        ushort weightMapIndex = (ushort)WeightMaps.IndexOf(weightMap);

                        Vertex_Position_Array.Add((vertex.PosX, vertex.PosY, vertex.PosZ, weightMapIndex));
                        Vertex_Normal_Array.Add((vertex.NormalX, vertex.NormalY, vertex.NormalZ, weightMapIndex));

                        var UV = (vertex.TextureU, vertex.TextureV);

                        if (!Vertex_UV_Array.Contains(UV))
                        {
                            Vertex_UV_Array.Add(UV);
                        }

                        ushort UVIndex = (ushort)Vertex_UV_Array.IndexOf(UV);

                        var COLOR = (vertex.ColorA, vertex.ColorR, vertex.ColorG, vertex.ColorB);

                        if (!Vertex_Color_Array.Contains(COLOR))
                        {
                            Vertex_Color_Array.Add(COLOR);
                        }

                        ushort COLORIndex = (ushort)Vertex_Color_Array.IndexOf(COLOR);

                        FinalFaceVextexIndex ii = new FinalFaceVextexIndex();
                        ii.indexVertex = (ushort)(Vertex_Position_Array.Count - 1);
                        ii.indexNormal = (ushort)(Vertex_Normal_Array.Count - 1);
                        ii.indexUV = UVIndex;
                        ii.indexColor = COLORIndex;
                        vertexIndices.Add(ii);
                    }

                    face.indexs = vertexIndices.ToArray();
                    group.Mesh[i] = face;
                }

                Groups.Add(group);
            }
            

            final.Groups = Groups.ToArray();
            final.WeightMaps = WeightMaps.ToArray();
            final.Vertex_Color_Array = Vertex_Color_Array.ToArray();
            final.Vertex_Normal_Array = Vertex_Normal_Array.ToArray();
            final.Vertex_Position_Array = Vertex_Position_Array.ToArray();
            final.Vertex_UV_Array = Vertex_UV_Array.ToArray();
            return final;
        }



    }
}
