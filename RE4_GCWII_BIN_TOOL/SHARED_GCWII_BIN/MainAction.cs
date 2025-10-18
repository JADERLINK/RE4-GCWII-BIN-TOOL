using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_GCWII_BIN
{
    public static class MainAction
    {
        public static void MainContinue(string[] args) 
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.WriteLine(SHARED_TOOLS.Shared.HeaderText());

            bool usingBatFile = false;
            int start = 0;
            if (args.Length > 0 && args[0].ToLowerInvariant() == "-bat")
            {
                usingBatFile = true;
                start = 1;
            }

            for (int i = start; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    try
                    {
                        FileInfo fileInfo1 = new FileInfo(args[i]);
                        string file1Extension = fileInfo1.Extension.ToUpperInvariant();
                        Console.WriteLine("File: " + fileInfo1.Name);
                        ContinueActions(fileInfo1, file1Extension);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + Environment.NewLine + ex);
                    }
                }
                else
                {
                    Console.WriteLine("File specified does not exist: " + args[i]);
                }

            }

            if (args.Length == 0)
            {
                Console.WriteLine("How to use: drag the file to the executable.");
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4-GCWII-BIN-TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Finished!!!");
                if (!usingBatFile)
                {
                    Console.WriteLine("Press any key to close the console.");
                    Console.ReadKey();
                }
            }
        }

        private static void ContinueActions(FileInfo fileInfo1, string file1Extension)
        {
            //diretorio, e nome do arquivo
            string baseDirectory = fileInfo1.DirectoryName;
            string baseName = Path.GetFileNameWithoutExtension(fileInfo1.Name);

            //stream
            Stream binFile = null;
            Stream idxmaterialFile = null;

            Stream idxggBinFile = null;
            Stream objFile = null;
            Stream smdFile = null;
            Stream mtlFile = null;

            Action CloseOpenedStreams = () => {
                binFile?.Close();
                idxmaterialFile?.Close();
                idxggBinFile?.Close();
                objFile?.Close();
                smdFile?.Close();
                mtlFile?.Close();
            };

            // verifica arquivos e posibiliades

            // arquivo 1
            switch (file1Extension)
            {
                case ".BIN":
                    binFile = fileInfo1.OpenRead();
                    break;
                case ".OBJ":
                    objFile = fileInfo1.OpenRead();
                    break;
                case ".SMD":
                    smdFile = fileInfo1.OpenRead();
                    break;
                case ".MTL":
                    mtlFile = fileInfo1.OpenRead();
                    break;
                case ".IDXGGBIN":
                    idxggBinFile = fileInfo1.OpenRead();
                    break;
                case ".IDXMATERIAL":
                    idxmaterialFile = fileInfo1.OpenRead();
                    break;
                default:
                    Console.WriteLine("The file format is invalid: " + fileInfo1.Name);
                    return;
            }

            //carregando arquivos adicionais
            switch (file1Extension)
            {
                case ".OBJ":
                case ".SMD":
                    // modo repack, tem que carregar o .IDXGGBIN

                    string idxbinFormat = ".idxggbin";
                    string idxbinFilePath = Path.Combine(baseDirectory, baseName + idxbinFormat);
                    if (File.Exists(idxbinFilePath))
                    {
                        Console.WriteLine("Load File: " + baseName + idxbinFormat);
                        idxggBinFile = new FileInfo(idxbinFilePath).OpenRead();
                    }
                    else
                    {
                        Console.WriteLine($"{idxbinFormat} file does not exist, it is necessary to repack the BIN;");
                        CloseOpenedStreams();
                        return;
                    }
                    break;
                default:
                    break;
            }

            //-----------
            //carrega os objetos arquivos.

            EXTRACT.GCWIIBIN BIN = null;
            EXTRACT.MorphBIN Morph = null;
            SHARED_TOOLS.ALL.IdxMaterial material = null;

            REPACK.IdxGgBin idxbin = null;

            ALL.IdxMtl idxMtl = null;

            if (binFile != null) //.BIN
            {
                BIN = EXTRACT.GcWiiBinDecoder.Decoder(binFile, 0, out _);
                material = ALL.IdxMaterialParser.Parser(BIN);
                Morph = EXTRACT.MorphBinDecoder.Decoder(binFile, 0, BIN.Header);
                binFile.Close();
            }

            if (idxggBinFile != null) //.IDXGGBIN
            {
                idxbin = REPACK.IdxGgBinLoad.Load(idxggBinFile);
                idxggBinFile.Close();
            }

            if ((file1Extension == ".OBJ" || file1Extension == ".SMD") && idxbin != null)
            {
                if (idxbin.UseIdxMaterial)
                {
                    // versão com idxmaterial
                    if (idxmaterialFile == null && mtlFile == null)
                    {
                        string mtlFilePath = Path.Combine(baseDirectory, baseName + ".idxmaterial");
                        if (File.Exists(mtlFilePath))
                        {
                            Console.WriteLine("Load File: " + baseName + ".idxmaterial");
                            idxmaterialFile = new FileInfo(mtlFilePath).OpenRead();
                        }
                        else
                        {
                            Console.WriteLine("IDXMATERIAL file does not exist, it is necessary to repack the BIN;");
                            CloseOpenedStreams();
                            return;
                        }
                    }
                }
                else
                {
                    // versão com mtl
                    if (idxmaterialFile == null && mtlFile == null)
                    {
                        string mtlFilePath = Path.Combine(baseDirectory, baseName + ".mtl");
                        if (File.Exists(mtlFilePath))
                        {
                            Console.WriteLine("Load File: " + baseName + ".mtl");
                            mtlFile = new FileInfo(mtlFilePath).OpenRead();
                        }
                        else
                        {
                            Console.WriteLine("MTL file does not exist, it is necessary to repack the BIN;");
                            CloseOpenedStreams();
                            return;
                        }
                    }

                }
            }

            if (idxmaterialFile != null) //.IDXMATERIAL
            {
                Console.WriteLine("Processing IDXMATERIAL");
                material = SHARED_TOOLS.ALL.IdxMaterialLoad.Load(idxmaterialFile);
                idxmaterialFile.Close();
            }

            if (mtlFile != null) //.MTL
            {
                Console.WriteLine("Processing MTL");
                REPACK.MtlLoad.Load(mtlFile, out idxMtl);
                REPACK.MtlConverter.Convert(idxMtl, out material);
                // o mtlFile é fechado no metodo acima.
            }

            // cria arquivos

            if (file1Extension == ".IDXGGBIN") // repack sem modelo 3d
            {
                material = new SHARED_TOOLS.ALL.IdxMaterial();
                material.MaterialDic = new Dictionary<string, SHARED_TOOLS.ALL.MaterialPart>();

                SHARED_TOOLS.REPACK.FinalBoneLine[] boneLines = REPACK.BinRepack.GetBoneLines(idxbin.Bones, Endianness.BigEndian);
                REPACK.Structures.FinalStructure final = new REPACK.Structures.FinalStructure();
                final.Groups = new REPACK.Structures.FinalMaterialGroup[0];
                final.Vertex_Color_Array = new (byte a, byte r, byte g, byte b)[0];
                final.Vertex_Normal_Array = new (short nx, short ny, short nz, ushort WeightIndex)[0];
                final.Vertex_Position_Array = new (short vx, short vy, short vz, ushort WeightIndex)[0];
                final.Vertex_UV_Array = new (short tu, short tv)[0];
                final.WeightMaps = new SHARED_TOOLS.REPACK.Structures.FinalWeightMap[0];

                if ((idxbin.EnableBonepairTag || idxbin.EnableAdjacentBoneTag) && idxbin.IsRe1Style == false)
                {
                    final.Vertex_Position_Array = new (short vx, short vy, short vz, ushort WeightIndex)[] { (0, 0, 0, 0) };
                    final.Vertex_Normal_Array = new (short vx, short vy, short vz, ushort WeightIndex)[] { (0, 0, 0, 0) };
                    final.Vertex_UV_Array = new (short tu, short tv)[] { (0, 0) };
                    final.WeightMaps = new SHARED_TOOLS.REPACK.Structures.FinalWeightMap[] { new SHARED_TOOLS.REPACK.Structures.FinalWeightMap(1, 0, 100, 0, 0, 0, 0)};
                    final.Groups = new REPACK.Structures.FinalMaterialGroup[1];
                    final.Groups[0] = new REPACK.Structures.FinalMaterialGroup();
                    final.Groups[0].materialName = "";
                    final.Groups[0].Mesh = new REPACK.Structures.FinalFace[1];
                    final.Groups[0].Mesh[0] = new REPACK.Structures.FinalFace();
                    final.Groups[0].Mesh[0].Type = 0x90;
                    final.Groups[0].Mesh[0].indexs = new REPACK.Structures.FinalFaceVextexIndex[3];
                    final.Groups[0].Mesh[0].indexs[0] = new REPACK.Structures.FinalFaceVextexIndex();
                    final.Groups[0].Mesh[0].indexs[1] = new REPACK.Structures.FinalFaceVextexIndex();
                    final.Groups[0].Mesh[0].indexs[2] = new REPACK.Structures.FinalFaceVextexIndex();
                }

                // cria arquivos
                Console.WriteLine("Creating file: " + baseName + ".BIN");
                string binFilePath = Path.Combine(baseDirectory, baseName + ".BIN");
                Stream binstream = File.Open(binFilePath, FileMode.Create);
                REPACK.BINmakeFile.MakeFile(binstream, 0, out _, final, boneLines, material,
                       idxbin.BonePairs, idxbin.UseAlternativeNormals, idxbin.UseWeightMap, idxbin.EnableBonepairTag,
                       idxbin.EnableAdjacentBoneTag, false, idxbin.IsRe1Style, 0);
                binstream.Close();
            }
            else if (file1Extension == ".BIN") // modo extract
            {
                EXTRACT.OutputFiles.CreateSMD(BIN, baseDirectory, baseName);
                EXTRACT.OutputFiles.CreateOBJ(BIN, baseDirectory, baseName);
                EXTRACT.OutputFiles.CreateIdxBin(BIN, baseDirectory, baseName);
                EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseName);

                var _idxMtl = ALL.IdxMtlParser.Parser(material, baseName);
                EXTRACT.OutputMaterial.CreateMTL(_idxMtl, baseDirectory, baseName);

                EXTRACT.OutputMorph.CreateMorphFiles(BIN, Morph, baseDirectory, baseName);
            }
            else if (file1Extension == ".OBJ") //repack with obj
            {
                SHARED_TOOLS.REPACK.FinalBoneLine[] boneLines = REPACK.BinRepack.GetBoneLines(idxbin.Bones, Endianness.BigEndian);
                REPACK.Structures.FinalStructure final = null;

                byte vertex_scale = 0;
                {
                    float FarthestVertex = 0; // valor que representa a maior distancia do modelo, tanto para X, Y ou Z
                    byte ObjFileUseBone = (byte)idxbin.ObjFileUseBone;
                    bool CompressVertices = true; // é sempre true
                    SHARED_TOOLS.REPACK.Structures.IntermediaryStructure intermediaryStructure;
                    REPACK.BinRepack.RepackOBJ(objFile, CompressVertices, ObjFileUseBone, out intermediaryStructure, false, ref FarthestVertex);
                    REPACK.Structures.IntermediaryLevel2 level2 = REPACK.BinRepack.MakeIntermediaryLevel2(intermediaryStructure,
                        idxbin.UseAlternativeNormals, idxbin.IsRe1Style, FarthestVertex, out vertex_scale);
                    final = REPACK.BinRepack.MakeFinalStructure(level2);
                }

                //checa limite de vertives
                if (final.Vertex_Position_Array.Length > ushort.MaxValue)
                {
                    Console.WriteLine("Error: Number of vertices greater than the limit: " + final.Vertex_Position_Array.Length + ";");
                    Console.WriteLine("The limit is: " + ushort.MaxValue + ";");
                    return;
                }

                // cria arquivos
                Console.WriteLine("Creating file: " + baseName + ".BIN");
                Console.WriteLine("Below is the order of the mesh with the name of the material used:");
                string binFilePath = Path.Combine(baseDirectory, baseName + ".BIN");
                Stream binstream = File.Open(binFilePath, FileMode.Create);
                REPACK.BINmakeFile.MakeFile(binstream, 0, out _, final, boneLines, material,
                    idxbin.BonePairs, idxbin.UseAlternativeNormals, idxbin.UseWeightMap, idxbin.EnableBonepairTag,
                    idxbin.EnableAdjacentBoneTag, idxbin.UseVertexColor, idxbin.IsRe1Style, vertex_scale);
                binstream.Close();

            }
            else if (file1Extension == ".SMD") //repack with smd
            {
                SHARED_TOOLS.REPACK.FinalBoneLine[] boneLines = null;
                REPACK.Structures.FinalStructure final = null;

                byte vertex_scale = 0;
                {
                    float FarthestVertex = 0; // valor que representa a maior distancia do modelo, tanto para X, Y ou Z
                    bool CompressVertices = true; // é sempre true
                    SHARED_TOOLS.REPACK.Structures.IntermediaryStructure intermediaryStructure;
                    REPACK.BinRepack.RepackSMD(smdFile, CompressVertices, out intermediaryStructure, out boneLines, ref FarthestVertex);
                    REPACK.Structures.IntermediaryLevel2 level2 = REPACK.BinRepack.MakeIntermediaryLevel2(intermediaryStructure,
                         idxbin.UseAlternativeNormals, idxbin.IsRe1Style, FarthestVertex, out vertex_scale);
                    final = REPACK.BinRepack.MakeFinalStructure(level2);
                }

                //checa limite de vertives
                if (final.Vertex_Position_Array.Length > ushort.MaxValue)
                {
                    Console.WriteLine("Error: Number of vertices greater than the limit: " + final.Vertex_Position_Array.Length + ";");
                    Console.WriteLine("The limit is: " + ushort.MaxValue + ";");
                    return;
                }

                // checa limite da combinações de pesos (WeightMap)
                if (final.WeightMaps.Length > byte.MaxValue)
                {
                    Console.WriteLine("Error: Number of WeightMap combinations greater than limit: " + final.WeightMaps.Length + ";");
                    Console.WriteLine("The limit is: " + byte.MaxValue + ";");
                    return;
                }

                // cria arquivos
                Console.WriteLine("Creating file: " + baseName + ".BIN");
                Console.WriteLine("Below is the order of the mesh with the name of the material used:");
                string binFilePath = Path.Combine(baseDirectory, baseName + ".BIN");
                Stream binstream = File.Open(binFilePath, FileMode.Create);
                REPACK.BINmakeFile.MakeFile(binstream, 0, out _, final, boneLines, material,
                    idxbin.BonePairs, idxbin.UseAlternativeNormals, idxbin.UseWeightMap, idxbin.EnableBonepairTag,
                    idxbin.EnableAdjacentBoneTag, false, idxbin.IsRe1Style, vertex_scale);
                binstream.Close();
            }
            else if (idxMtl != null) // cria idxMaterial derivado do mtl
            {
                EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseName + ".Repack");
            }
            else if (idxmaterialFile != null) // cria mtl derivado do idxMaterial
            {
                var _idxMtl = ALL.IdxMtlParser.Parser(material, baseName);
                EXTRACT.OutputMaterial.CreateMTL(_idxMtl, baseDirectory, baseName + ".Repack");
            }
        }

    }
}
