using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.ALL;
using SHARED_GCWII_BIN.EXTRACT;

namespace SHARED_GCWII_BIN.ALL
{
    public static class IdxMaterialParser
    {
        public static IdxMaterial Parser(GCWIIBIN uhdBIN) 
        {
            IdxMaterial idx = new IdxMaterial();
            idx.MaterialDic = new Dictionary<string, MaterialPart>();

            for (int i = 0; i < uhdBIN.Materials.Length; i++)
            {
                idx.MaterialDic.Add(CONSTs.MATERIAL + i.ToString("D3"), uhdBIN.Materials[i].material);
            }

            return idx;
        }

    }
}
