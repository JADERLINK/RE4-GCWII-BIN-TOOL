using System;

namespace SHARED_TOOLS
{
    public static class Shared
    {
        private const string VERSION = "V.1.0.1 (2025-09-02)";

        public static string HeaderText()
        {
            return "# github.com/JADERLINK/RE4-GCWII-BIN-TOOL" + Environment.NewLine +
                   "# youtube.com/@JADERLINK" + Environment.NewLine +
                   "# RE4_GCWII_BIN_TOOL by: JADERLINK" + Environment.NewLine +
                   "# Thanks to \"mariokart64n\"" + Environment.NewLine +
                   "# Material information by \"Albert\"" + Environment.NewLine +
                  $"# Version {VERSION}";
        }

        public static string HeaderTextSmd()
        {
            return "// RE4_GCWII_BIN_TOOL" + Environment.NewLine +
                   "// by: JADERLINK" + Environment.NewLine +
                   "// youtube.com/@JADERLINK" + Environment.NewLine +
                  $"// Version {VERSION}";
        }
    }
}
