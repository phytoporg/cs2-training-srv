using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenariosPlugin
{
    public class Map
    {
        public enum MapType
        {
            Invalid,
            Mirage
        }

        public static string MapTypeToString(MapType mapType)
        {
            switch (mapType) {
                case MapType.Invalid:
                    return "";
                case MapType.Mirage:
                    return "de_mirage";
                default:
                    return "";
            }
        }
    }
}
