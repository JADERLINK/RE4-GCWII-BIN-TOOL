using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RE4_GCWII_BIN_TOOL.REPACK.Structures
{
    public class IntermediaryLevel2
    {
        public Dictionary<string, IntermediaryLevel2Mesh> Groups { get; set; }

        public IntermediaryLevel2()
        {
            Groups = new Dictionary<string, IntermediaryLevel2Mesh>();
        }
    }

    public class IntermediaryLevel2Mesh
    {
        public string MaterialName { get; set; }

        public List<IntermediaryLevel2Face> Faces { get; set; }

        public IntermediaryLevel2Mesh()
        {
            Faces = new List<IntermediaryLevel2Face>();
        }
    }


    public class IntermediaryLevel2Face
    {
        public byte Type;
        public ushort Count;

        public List<IntermediaryVertex2> Vertexs { get; set; }

        public IntermediaryLevel2Face()
        {
            Vertexs = new List<IntermediaryVertex2>();
        }
    }


    public class IntermediaryVertex2
    {
        public short PosX { get; set; }
        public short PosY { get; set; }
        public short PosZ { get; set; }

        public short NormalX { get; set; }
        public short NormalY { get; set; }
        public short NormalZ { get; set; }

        public short TextureU { get; set; }
        public short TextureV { get; set; }

        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public byte ColorA { get; set; }
        public byte Links { get; set; }
        public byte BoneID1 { get; set; }
        public byte Weight1 { get; set; }

        public byte BoneID2 { get; set; }
        public byte Weight2 { get; set; }

        public byte BoneID3 { get; set; }
        public byte Weight3 { get; set; }

        public FinalWeightMap GetFinalWeightMap()
        {
            FinalWeightMap weightMap = new FinalWeightMap();
            weightMap.Links = Links;
            weightMap.BoneID1 = BoneID1;
            weightMap.BoneID2 = BoneID2;
            weightMap.BoneID3 = BoneID3;
            weightMap.Weight1 = Weight1;
            weightMap.Weight2 = Weight2;
            weightMap.Weight3 = Weight3;
            return weightMap;
        }
    }
}
