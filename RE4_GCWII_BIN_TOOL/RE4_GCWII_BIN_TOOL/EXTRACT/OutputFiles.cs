using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RE4_GCWII_BIN_TOOL.ALL;
using SimpleEndianBinaryIO;

namespace RE4_GCWII_BIN_TOOL.EXTRACT
{
    public static class OutputFiles
    {
        private static float get_scale_from_vertex_scale(byte vertex_scale) 
        {
            return (float)Math.Pow(2, vertex_scale);
        }

        //Studiomdl Data
        public static void CreateSMD(GCWIIBIN bin, string baseDirectory, string baseFileName)
        {
            TextWriter text = new FileInfo(Path.Combine(baseDirectory, baseFileName + ".smd")).CreateText();
            text.WriteLine("version 1");
            text.WriteLine("nodes");

            //Bones Fix
            (uint BoneID, short BoneParent, float p1, float p2, float p3)[] FixedBones = new (uint BoneID, short BoneParent, float p1, float p2, float p3)[bin.Bones.Length];

            // Bone ID, number of times found
            Dictionary<byte, int> BoneCheck = new Dictionary<byte, int>();
            for (int i = bin.Bones.Length - 1; i >= 0; i--)
            {
                byte InBoneID = bin.Bones[i].BoneID;
                uint OutBoneID = InBoneID;
                if (BoneCheck.ContainsKey(InBoneID))
                {
                    OutBoneID += (uint)(0x100u * BoneCheck[InBoneID]);
                    BoneCheck[InBoneID]++;
                }
                else
                {
                    BoneCheck.Add(InBoneID, 1);
                }

                short BoneParent = bin.Bones[i].BoneParent;
                if (BoneParent == 0xFF)
                {
                    BoneParent = -1;
                }
                
                float p1 = bin.Bones[i].PositionX / CONSTs.GLOBAL_POSITION_SCALE;
                float p2 = bin.Bones[i].PositionZ * -1 / CONSTs.GLOBAL_POSITION_SCALE;
                float p3 = bin.Bones[i].PositionY / CONSTs.GLOBAL_POSITION_SCALE;

                FixedBones[i] = (OutBoneID, BoneParent, p1, p2, p3);
            }

            for (int i = 0; i < FixedBones.Length; i++)
            {
                text.WriteLine(FixedBones[i].BoneID + " \"BONE_" + FixedBones[i].BoneID.ToString("D3") + "\" " + FixedBones[i].BoneParent);
            }

            text.WriteLine("end");

            text.WriteLine("skeleton");
            text.WriteLine("time 0");

            for (int i = 0; i < FixedBones.Length; i++)
            {
                text.WriteLine(FixedBones[i].BoneID + "  " +
                               FixedBones[i].p1.ToFloatString() + " " +
                               FixedBones[i].p2.ToFloatString() + " " +
                               FixedBones[i].p3.ToFloatString() + "  0.0 0.0 0.0");
            }

            text.WriteLine("end");

            text.WriteLine("triangles");

            float extraScale = CONSTs.GLOBAL_POSITION_SCALE * get_scale_from_vertex_scale(bin.Header.vertex_scale);

            for (int g = 0; g < bin.Materials.Length; g++)
            {
                for (int l = 0; l < bin.Materials[g].face_index_array.Length; l++)
                {
                    text.WriteLine(CONSTs.MATERIAL + g.ToString("D3"));

                    FaceIndex[] indexs = new FaceIndex[3];
                    indexs[0] = bin.Materials[g].face_index_array[l].i1;
                    indexs[1] = bin.Materials[g].face_index_array[l].i2;
                    indexs[2] = bin.Materials[g].face_index_array[l].i3;

                    for (int i = 0; i < indexs.Length; i++)
                    {
                        float vx = bin.Vertex_Position_Array[indexs[i].indexVertex].vx / extraScale;
                        float vy = bin.Vertex_Position_Array[indexs[i].indexVertex].vy / extraScale;
                        float vz = bin.Vertex_Position_Array[indexs[i].indexVertex].vz / extraScale * -1;

                        float nx = bin.Vertex_Normal_Array[indexs[i].indexNormal].nx;
                        float ny = bin.Vertex_Normal_Array[indexs[i].indexNormal].ny;
                        float nz = bin.Vertex_Normal_Array[indexs[i].indexNormal].nz;

                        float NORMAL_FIX = (float)Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
                        NORMAL_FIX = (NORMAL_FIX == 0) ? 1 : NORMAL_FIX;
                        nx /= NORMAL_FIX;
                        ny /= NORMAL_FIX;
                        nz /= NORMAL_FIX * -1;

                        float tu;
                        float tv;

                        if (bin.Header.ReturnHasEnableModernStyle())
                        {
                            tu = bin.Vertex_UV_Array[indexs[i].indexUV].tu / (float)byte.MaxValue;
                            tv = ((bin.Vertex_UV_Array[indexs[i].indexUV].tv / (float)byte.MaxValue) - 1) * -1;
                        }
                        else 
                        {
                            tu = bin.Vertex_UV_Array[indexs[i].indexUV].tu / (float)short.MaxValue;
                            tv = ((bin.Vertex_UV_Array[indexs[i].indexUV].tv / (float)short.MaxValue) - 1) * -1;
                        }          

                        string res = "0"
                        + " " + vx.ToFloatString()
                        + " " + vz.ToFloatString()
                        + " " + vy.ToFloatString()
                        + " " + nx.ToFloatString()
                        + " " + nz.ToFloatString()
                        + " " + ny.ToFloatString()
                        + " " + tu.ToFloatString()
                        + " " + tv.ToFloatString();

                        if (bin.WeightMaps != null && bin.WeightMaps.Length != 0)
                        {
                            ushort indexw = bin.Vertex_Position_Array[indexs[i].indexVertex].weightmap_index;

                            int links = bin.WeightMaps[indexw].count;

                            res += " " + links;

                            if (links >= 1)
                            {
                                res += " " + bin.WeightMaps[indexw].boneId1 + " " + (bin.WeightMaps[indexw].weight1 / 100f).ToFloatString();
                            }

                            if (links >= 2)
                            {
                                res += " " + bin.WeightMaps[indexw].boneId2 + " " + (bin.WeightMaps[indexw].weight2 / 100f).ToFloatString();
                            }

                            if (links >= 3)
                            {
                                res += " " + bin.WeightMaps[indexw].boneId3 + " " + (bin.WeightMaps[indexw].weight3 / 100f).ToFloatString();
                            }

                        }
                        else
                        {
                            res += " 0";
                        }

                        text.WriteLine(res);
                    }
                }

            }


            text.WriteLine("end");
            text.Write(Shared.HeaderTextSmd());
            text.Close();
        }

        public static void CreateOBJ(GCWIIBIN bin, string baseDirectory, string baseFileName)
        {
            var obj = new FileInfo(Path.Combine(baseDirectory, baseFileName + ".obj")).CreateText();

            obj.WriteLine(Shared.HeaderText());

            obj.WriteLine("mtllib " + baseFileName + ".mtl");

            float extraScale = CONSTs.GLOBAL_POSITION_SCALE * get_scale_from_vertex_scale(bin.Header.vertex_scale);

            for (int i = 0; i < bin.Vertex_Position_Array.Length; i++)
            {
                float vx = bin.Vertex_Position_Array[i].vx / extraScale;
                float vy = bin.Vertex_Position_Array[i].vy / extraScale;
                float vz = bin.Vertex_Position_Array[i].vz / extraScale;

                string v = "v " + vx.ToFloatString() + " " + vy.ToFloatString() + " " + vz.ToFloatString();

                obj.WriteLine(v);
            }

            for (int i = 0; i < bin.Vertex_Normal_Array.Length; i++)
            {
                float nx = bin.Vertex_Normal_Array[i].nx;
                float ny = bin.Vertex_Normal_Array[i].ny;
                float nz = bin.Vertex_Normal_Array[i].nz;

                float NORMAL_FIX = (float)Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
                NORMAL_FIX = (NORMAL_FIX == 0) ? 1 : NORMAL_FIX;
                nx /= NORMAL_FIX;
                ny /= NORMAL_FIX;
                nz /= NORMAL_FIX;

                obj.WriteLine("vn " + nx.ToFloatString() + " " + ny.ToFloatString() + " " + nz.ToFloatString());
            }

            for (int i = 0; i < bin.Vertex_UV_Array.Length; i++)
            {
                float tu;
                float tv;

                if (bin.Header.ReturnHasEnableModernStyle())
                {
                    tu = bin.Vertex_UV_Array[i].tu / (float)byte.MaxValue;
                    tv = ((bin.Vertex_UV_Array[i].tv / (float)byte.MaxValue) - 1) * -1;
                }
                else
                {
                    tu = bin.Vertex_UV_Array[i].tu / (float)short.MaxValue;
                    tv = ((bin.Vertex_UV_Array[i].tv / (float)short.MaxValue) - 1) * -1;
                }
               
                obj.WriteLine("vt " + tu.ToFloatString() + " " + tv.ToFloatString());
            }


            for (int g = 0; g < bin.Materials.Length; g++)
            {
                obj.WriteLine("g " + CONSTs.MATERIAL + g.ToString("D3"));
                obj.WriteLine("usemtl " + CONSTs.MATERIAL + g.ToString("D3"));

                for (int i = 0; i < bin.Materials[g].face_index_array.Length; i++)
                {
                    string av = (bin.Materials[g].face_index_array[i].i1.indexVertex + 1).ToString();
                    string bv = (bin.Materials[g].face_index_array[i].i2.indexVertex + 1).ToString();
                    string cv = (bin.Materials[g].face_index_array[i].i3.indexVertex + 1).ToString();

                    string an = (bin.Materials[g].face_index_array[i].i1.indexNormal + 1).ToString();
                    string bn = (bin.Materials[g].face_index_array[i].i2.indexNormal + 1).ToString();
                    string cn = (bin.Materials[g].face_index_array[i].i3.indexNormal + 1).ToString();

                    string at = (bin.Materials[g].face_index_array[i].i1.indexUV + 1).ToString();
                    string bt = (bin.Materials[g].face_index_array[i].i2.indexUV + 1).ToString();
                    string ct = (bin.Materials[g].face_index_array[i].i3.indexUV + 1).ToString();

                    obj.WriteLine("f " + av + "/" + at + "/" + an
                                 + " " + bv + "/" + bt + "/" + bn
                                 + " " + cv + "/" + ct + "/" + cn);
                }

            }

            obj.Close();
        }

        public static void CreateIdxBin(GCWIIBIN bin, string baseDirectory, string baseFileName)
        {

            var idx = new FileInfo(Path.Combine(baseDirectory, baseFileName + ".idxggbin")).CreateText();
            idx.WriteLine(Shared.HeaderText());
            idx.WriteLine();
            idx.WriteLine();

            idx.WriteLine("UseIdxMaterial:False");
            if (bin.Header.ReturnHasEnableModernStyle())
            {
                idx.WriteLine("UseAlternativeNormals:" + bin.Header.ReturnsHasNormalsAlternativeTag());
                idx.WriteLine("UseWeightMap:" + (bin.Header.weightmap_count != 0));
                idx.WriteLine("EnableAdjacentBoneTag:" + bin.Header.ReturnsHasEnableAdjacentBoneTag());
                idx.WriteLine("EnableBonepairTag:" + bin.Header.ReturnsHasEnableBonepairTag());
                //idx.WriteLine("UseVertexColor:False");
            }
            else
            {
                idx.WriteLine("IsRe1Style:True");
            }

            idx.WriteLine("ObjFileUseBone:0");

            idx.WriteLine();
            idx.WriteLine();
            idx.WriteLine("## Bones");
            for (int i = 0; i < bin.Bones.Length; i++)
            {
                float p1 = bin.Bones[i].PositionX / CONSTs.GLOBAL_POSITION_SCALE;
                float p2 = bin.Bones[i].PositionZ * -1 / CONSTs.GLOBAL_POSITION_SCALE;
                float p3 = bin.Bones[i].PositionY / CONSTs.GLOBAL_POSITION_SCALE;

                idx.WriteLine("BoneLine:" +
                    bin.Bones[i].BoneID.ToString().PadLeft(4) + " " +
                    (bin.Bones[i].BoneParent != 0xFF ? bin.Bones[i].BoneParent : -1).ToString().PadLeft(4) + "   " +
                    p1.ToFloatString() + "  " +
                    p2.ToFloatString() + "  " +
                    p3.ToFloatString()
                    );
            }


            if (bin.BonePairs != null && bin.BonePairs.Length != 0)
            {
                idx.WriteLine();
                idx.WriteLine();
                idx.WriteLine("## BonePairs:");

                for (int i = 0; i < bin.BonePairs.Length; i++)
                {
                    idx.WriteLine("BonePair:" +
                       bin.BonePairs[i].Bone1.ToString().PadLeft(4) + " " +
                       bin.BonePairs[i].Bone2.ToString().PadLeft(4) + " " +
                       bin.BonePairs[i].Bone3.ToString().PadLeft(4) + " " +
                       bin.BonePairs[i].Bone4.ToString().PadLeft(4)
                       );
                }
            }

            idx.Close();
        }

    }
}
