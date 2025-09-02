using SHARED_TOOLS.ALL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RE4_GCWII_BIN_TOOL.EXTRACT
{
    public static class OutputMorph
    {
        private static float get_scale_from_vertex_scale(byte vertex_scale)
        {
            return (float)Math.Pow(2, vertex_scale);
        }

        public static void CreateMorphFiles(GCWIIBIN bin, MorphBIN morph, string baseDirectory, string baseFileName) 
        {
            if (morph != null)
            {
                for (int i = 0; i < morph.MorphGroups.Length; i++)
                {
                    string name = baseFileName + "_morph_" + i.ToString("D2");
                    CreateMorphOBJ(bin, morph.MorphGroups[i], baseDirectory, name, baseFileName);
                }

                CreateMorphVTA(bin, morph, baseDirectory, baseFileName);
            }
        }

        private static void CreateMorphOBJ(GCWIIBIN bin, MorphGroup morphGroup, string baseDirectory, string baseFileName, string mtlName)
        {
            var obj = new FileInfo(Path.Combine(baseDirectory, baseFileName + ".obj")).CreateText();

            obj.WriteLine(SHARED_TOOLS.Shared.HeaderText());

            obj.WriteLine("mtllib " + mtlName + ".mtl");

            float extraScale = CONSTs.GLOBAL_POSITION_SCALE * get_scale_from_vertex_scale(bin.Header.vertex_scale);

            Dictionary<int, (short x, short y, short z)> morph = new Dictionary<int, (short x, short y, short z)>();
            foreach (var item in morphGroup.Morph_Vertex)
            {
                if ( ! morph.ContainsKey(item.VertexID))
                {
                    morph.Add(item.VertexID, (item.posX, item.posY, item.posZ));
                }
            }

            for (int i = 0; i < bin.Vertex_Position_Array.Length; i++)
            {
                int e_x = (morph.ContainsKey(i) ? morph[i].x : 0);
                int e_y = (morph.ContainsKey(i) ? morph[i].y : 0);
                int e_z = (morph.ContainsKey(i) ? morph[i].z : 0);

                float vx = (bin.Vertex_Position_Array[i].vx + e_x) / extraScale;
                float vy = (bin.Vertex_Position_Array[i].vy + e_y) / extraScale;
                float vz = (bin.Vertex_Position_Array[i].vz + e_z) / extraScale;

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
                float tu = bin.Vertex_UV_Array[i].tu / (float)byte.MaxValue;
                float tv = ((bin.Vertex_UV_Array[i].tv / (float)byte.MaxValue) - 1) * -1;
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

        private static void CreateMorphVTA(GCWIIBIN bin, MorphBIN morph, string baseDirectory, string baseFileName)
        {
            TextWriter text = new FileInfo(Path.Combine(baseDirectory, baseFileName + ".vta")).CreateText();
            text.WriteLine("version 1");
            text.WriteLine("nodes");

            //Bones Fix
            (uint BoneID, short BoneParent)[] FixedBones = new (uint BoneID, short BoneParent)[bin.Bones.Length];

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

                FixedBones[i] = (OutBoneID, BoneParent);
            }

            for (int i = 0; i < FixedBones.Length; i++)
            {
                text.WriteLine(FixedBones[i].BoneID + " \"BONE_" + FixedBones[i].BoneID.ToString("D3") + "\" " + FixedBones[i].BoneParent);
            }

            text.WriteLine("end");

            text.WriteLine("skeleton");
            text.WriteLine("time 0");

            for (int m = 0; m < morph.MorphGroups.Length; m++)
            {
                text.WriteLine("time " + (m + 1));
            }

            text.WriteLine("end");

            text.WriteLine("vertexanimation");

            float extraScale = CONSTs.GLOBAL_POSITION_SCALE * get_scale_from_vertex_scale(bin.Header.vertex_scale);

            {
                int indexcounter = 0;

                text.WriteLine("time 0");

                for (int g = 0; g < bin.Materials.Length; g++)
                {
                    for (int l = 0; l < bin.Materials[g].face_index_array.Length; l++)
                    {

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

                            string res = indexcounter
                            + " " + vx.ToFloatString()
                            + " " + vz.ToFloatString()
                            + " " + vy.ToFloatString()
                            + " " + nx.ToFloatString()
                            + " " + nz.ToFloatString()
                            + " " + ny.ToFloatString();

                            text.WriteLine(res);

                            indexcounter++;
                        }
                    }

                }
            }

            for (int m = 0; m < morph.MorphGroups.Length; m++)
            {
                Dictionary<int, (short x, short y, short z)> morphDic = new Dictionary<int, (short x, short y, short z)>();
                foreach (var item in morph.MorphGroups[m].Morph_Vertex)
                {
                    if (!morphDic.ContainsKey(item.VertexID))
                    {
                        morphDic.Add(item.VertexID, (item.posX, item.posY, item.posZ));
                    }
                }

                text.WriteLine("time " + (m + 1));

                int indexcounter = 0;

                for (int g = 0; g < bin.Materials.Length; g++)
                {
                    for (int l = 0; l < bin.Materials[g].face_index_array.Length; l++)
                    {
                        FaceIndex[] indexs = new FaceIndex[3];
                        indexs[0] = bin.Materials[g].face_index_array[l].i1;
                        indexs[1] = bin.Materials[g].face_index_array[l].i2;
                        indexs[2] = bin.Materials[g].face_index_array[l].i3;

                        for (int i = 0; i < indexs.Length; i++)
                        {
                            float e_x = (morphDic.ContainsKey(indexs[i].indexVertex) ? morphDic[indexs[i].indexVertex].x : 0);
                            float e_y = (morphDic.ContainsKey(indexs[i].indexVertex) ? morphDic[indexs[i].indexVertex].y : 0);
                            float e_z = (morphDic.ContainsKey(indexs[i].indexVertex) ? morphDic[indexs[i].indexVertex].z : 0);

                            float vx = (bin.Vertex_Position_Array[indexs[i].indexVertex].vx + e_x) / extraScale;
                            float vy = (bin.Vertex_Position_Array[indexs[i].indexVertex].vy + e_y) / extraScale;
                            float vz = (bin.Vertex_Position_Array[indexs[i].indexVertex].vz + e_z) / extraScale * -1;

                            float nx = bin.Vertex_Normal_Array[indexs[i].indexNormal].nx;
                            float ny = bin.Vertex_Normal_Array[indexs[i].indexNormal].ny;
                            float nz = bin.Vertex_Normal_Array[indexs[i].indexNormal].nz;

                            float NORMAL_FIX = (float)Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
                            NORMAL_FIX = (NORMAL_FIX == 0) ? 1 : NORMAL_FIX;
                            nx /= NORMAL_FIX;
                            ny /= NORMAL_FIX;
                            nz /= NORMAL_FIX * -1;

                            if (morphDic.ContainsKey(indexs[i].indexVertex))
                            {
                                string res = indexcounter
                                + " " + vx.ToFloatString()
                                + " " + vz.ToFloatString()
                                + " " + vy.ToFloatString()
                                + " " + nx.ToFloatString()
                                + " " + nz.ToFloatString()
                                + " " + ny.ToFloatString();
                                text.WriteLine(res);
                            }

                            indexcounter++;
                        }
                    }

                }

            }

            text.WriteLine("end");

            text.Write(SHARED_TOOLS.Shared.HeaderTextSmd());
            text.Close();
        }


    }
}
