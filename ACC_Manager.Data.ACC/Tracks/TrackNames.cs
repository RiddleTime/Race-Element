using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Tracks
{
    public class TrackNames
    {
        /// <summary>
        /// (folder/code name, Name )
        /// </summary>
        public static readonly Dictionary<string, string> Tracks = new Dictionary<string, string>() {
            {"Barcelona", "Circuit de Barcelona-Catalunya" },
            {"brands_hatch", "Brands Hatch Circuit" },
            {"cota", "Circuit of the Americas" },
            {"donington", "Donington Park" },
            {"Hungaroring", "Hungaroring" },
            {"Imola", "Imola (Autodromo Internazionale Enzo e Dino Ferrari)" },
            {"indianapolis", "Indanapolis Motor Speedway" },
            {"Kyalami", "Kyalami Grand Prix Circuit" },
            {"Laguna_Seca", "Weathertech Raceway Laguna Seca" },
            {"misano", "Misano World Circuit" },
            {"monza", "Monza Circuit" },
            {"mount_panorama", "Mount Panorama Circuit" },
            {"nurburgring", "Nürburgring" },
            {"oulton_park", "Oulton Park" },
            {"Paul_Ricard", "Circuit Paul Ricard" },
            {"Silverstone", "Silverstone" },
            {"snetterton", "Snetterton Circuit" },
            {"Spa", "Circuit De Spa-Francorchamps" },
            {"Suzuka", "Suzuka Circuit" },
            {"watkins_glen", "Watkins Glen International" },
            {"Zandvoort", "Circuit Zandvoort" },
            {"Zolder", "Circuit Zolder" },
        };
    }
}
