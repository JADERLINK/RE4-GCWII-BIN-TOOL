using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RE4_GCWII_BIN_TOOL.ALL;
using RE4_GCWII_BIN_TOOL.EXTRACT;

namespace RE4_GCWII_BIN_TOOL.REPACK
{
    public class MtlConverter
    {
        public MtlConverter() {}

        public void Convert(IdxMtl idxmtl, out IdxMaterial idxMaterial)
        {
            idxMaterial = new IdxMaterial();
            idxMaterial.MaterialDic = new Dictionary<string, MaterialPart>();
            //-----

            foreach (var item in idxmtl.MtlDic)
            {
                MaterialPart mat = new MaterialPart();
                mat.custom_specular_map = 255;
                mat.generic_specular_map = 255;
                mat.opacity_map = 255;
                mat.bump_map = 255;

                if (item.Value.map_Bump != null)
                {
                    mat.material_flag |= 0x01; // bump flag

                    mat.bump_map = (byte)item.Value.map_Bump.TextureID;
                    mat.generic_specular_map = 0;
                    mat.intensity_specular_b = 255;
                    mat.intensity_specular_g = 255;
                    mat.intensity_specular_r = 255;
                    mat.specular_scale = 0x00;
                }

                if (item.Value.map_d != null)
                {
                    mat.material_flag |= 0x04; // opacity flag

                    mat.opacity_map = (byte)item.Value.map_d.TextureID;
                }

                if (item.Value.ref_specular_map != null)
                {
                    mat.material_flag |= 0x02; //generic specular flag

                    mat.intensity_specular_b = item.Value.Ks.GetR();
                    mat.intensity_specular_g = item.Value.Ks.GetG();
                    mat.intensity_specular_r = item.Value.Ks.GetB();
                    mat.specular_scale = item.Value.specular_scale;

                    if (item.Value.ref_specular_map.BaseFileName == "generic_specular")
                    {
                        mat.generic_specular_map = (byte)item.Value.ref_specular_map.TextureID;
                    }
                    else
                    {
                        mat.material_flag |= 0x10; // custom specular flag

                        mat.generic_specular_map = 0;
                        mat.custom_specular_map = (byte)item.Value.ref_specular_map.TextureID;
                    }
                }

                mat.diffuse_map =(byte)item.Value.map_Kd.TextureID;

                idxMaterial.MaterialDic.Add(item.Key, mat);
            }

        }

    }
}
