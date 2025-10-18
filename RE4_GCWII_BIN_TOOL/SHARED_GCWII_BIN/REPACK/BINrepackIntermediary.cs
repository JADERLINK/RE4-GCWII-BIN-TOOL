using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.REPACK.Structures;
using SHARED_TOOLS.ALL;

namespace SHARED_GCWII_BIN.REPACK
{
    public static partial class BinRepack
    {
        private static IntermediaryStructure MakeIntermediaryStructure(StartStructure startStructure)
        {
            IntermediaryStructure intermediary = new IntermediaryStructure();

            foreach (var item in startStructure.FacesByMaterial)
            {
                IntermediaryMesh mesh = new IntermediaryMesh();

                for (int i = 0; i < item.Value.Faces.Count; i++)
                {
                    IntermediaryFace face = new IntermediaryFace();

                    for (int iv = 0; iv < item.Value.Faces[i].Count; iv++)
                    {
                        IntermediaryVertex vertex = new IntermediaryVertex();

                        vertex.PosX = item.Value.Faces[i][iv].Position.X * CONSTs.GLOBAL_POSITION_SCALE;
                        vertex.PosY = item.Value.Faces[i][iv].Position.Y * CONSTs.GLOBAL_POSITION_SCALE;
                        vertex.PosZ = item.Value.Faces[i][iv].Position.Z * CONSTs.GLOBAL_POSITION_SCALE;

                        vertex.NormalX = item.Value.Faces[i][iv].Normal.X;
                        vertex.NormalY = item.Value.Faces[i][iv].Normal.Y;
                        vertex.NormalZ = item.Value.Faces[i][iv].Normal.Z;

                        vertex.TextureU = item.Value.Faces[i][iv].Texture.U;
                        vertex.TextureV = item.Value.Faces[i][iv].Texture.V;

                        vertex.ColorR = item.Value.Faces[i][iv].Color.R;
                        vertex.ColorG = item.Value.Faces[i][iv].Color.G;
                        vertex.ColorB = item.Value.Faces[i][iv].Color.B;
                        vertex.ColorA = item.Value.Faces[i][iv].Color.A;

                        vertex.WeightMap = item.Value.Faces[i][iv].WeightMap;

                        face.Vertexs.Add(vertex);
                    }

                    mesh.Faces.Add(face);
                }

                mesh.MaterialName = item.Key.ToUpperInvariant();
                intermediary.Groups.Add(mesh.MaterialName, mesh);
            }

            return intermediary;
        }

    }
}
