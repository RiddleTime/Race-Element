using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Tracks
{
    public class TrackNames
    {
        public class TrackData
        {
            public Guid Guid { get; internal set; }
            public string FullName { get; internal set; }
            public int TrackLength { get; internal set; }
        }

        /// <summary>
        /// (folder/code name, Name )
        /// </summary>
        public static readonly Dictionary<string, TrackData> Tracks = new Dictionary<string, TrackData>() {
            {"Barcelona", new TrackData(){FullName ="Circuit de Barcelona-Catalunya", Guid = new Guid("c47d348d-4cac-4377-90ff-be3613bc6519"), TrackLength = 4655 } },
            {"brands_hatch",  new TrackData(){FullName ="Brands Hatch Circuit", Guid = new Guid("7827823f-6df1-4295-bc24-93bc83d71855"), TrackLength = 3908 } },
            {"cota",  new TrackData(){FullName ="Circuit of the Americas" , Guid = new Guid("f45eac53-7a77-4fe5-812f-064b30ac22df"), TrackLength = 5513 } },
            {"donington",  new TrackData(){FullName ="Donington Park" , Guid = new Guid("e93925e9-16c8-442a-bea3-ae449d2e04be"), TrackLength = 4020} },
            {"Hungaroring",  new TrackData(){FullName ="Hungaroring" , Guid = new Guid("f59e6015-077b-40e0-a822-71104f253ea2"), TrackLength = 4381} },
            {"Imola",  new TrackData(){FullName ="Imola (Autodromo Internazionale Enzo e Dino Ferrari)", Guid = new Guid("678eefc3-a5f0-4b2a-a1cc-03ac62650ede"), TrackLength = 4959 } },
            {"indianapolis",  new TrackData(){FullName ="Indianapolis Motor Speedway" , Guid = new Guid("d3c246d2-edba-429e-af59-6e25357d59d4"), TrackLength = 4167 } },
            {"Kyalami",  new TrackData(){FullName ="Kyalami Grand Prix Circuit" , Guid = new Guid("65e00cd4-6c39-4cb4-acf9-f8977cd56ba1"), TrackLength = 4522 } },
            {"Laguna_Seca",  new TrackData(){FullName ="Weathertech Raceway Laguna Seca" , Guid = new Guid("0c8d198c-608f-4beb-96f5-5eafb5d3ba6b"), TrackLength = 3602 } },
            {"misano",  new TrackData(){FullName ="Misano World Circuit" , Guid = new Guid("e8ce417b-5f5c-4921-9b6b-9367e703d3f8"), TrackLength = 4226 } },
            {"monza",  new TrackData(){FullName ="Monza Circuit" , Guid = new Guid("5091ac88-c7c3-4cf1-ac46-e974bc7b73d5"), TrackLength = 5793 } },
            {"mount_panorama",  new TrackData(){FullName ="Mount Panorama Circuit" , Guid = new Guid("c054299c-0e16-4094-8e76-a9a4da399268"), TrackLength = 6213 } },
            {"nurburgring",  new TrackData(){FullName ="Nürburgring" , Guid = new Guid("20200ee1-89c1-4580-86f1-3ded3018e9e3"), TrackLength = 5137 } },
            {"oulton_park",  new TrackData(){FullName ="Oulton Park" , Guid = new Guid("72794bc2-841c-40e1-8587-3e41f9228ea8"), TrackLength = 4307 } },
            {"Paul_Ricard",  new TrackData(){FullName ="Circuit Paul Ricard" , Guid = new Guid("c46a0299-b5d0-421f-90a7-0c5223deaa63"), TrackLength = 5770 } },
            {"Silverstone",  new TrackData(){FullName ="Silverstone" , Guid = new Guid("8636837e-e916-4d4b-8f29-625cf6bf4695"), TrackLength = 5891 } },
            {"snetterton",  new TrackData(){FullName ="Snetterton Circuit" , Guid = new Guid("9248d360-e1ba-45be-bdec-dc939fb3959b"), TrackLength = 4779 } },
            {"Spa",  new TrackData(){FullName ="Circuit De Spa-Francorchamps", Guid = new Guid("a56b5381-6c59-4380-8a32-679c8734a9a9"), TrackLength = 7004 } },
            {"Suzuka",  new TrackData(){FullName ="Suzuka Circuit", Guid = new Guid("1c9b35a5-dacb-40e2-adc9-134d75f75c3f"), TrackLength = 5807 } },
            {"watkins_glen",  new TrackData(){FullName ="Watkins Glen International", Guid = new Guid("6c4d5fe4-105d-47b0-8699-49c10a92c591"), TrackLength = 5552 } },
            {"Zandvoort",  new TrackData(){FullName ="Circuit Zandvoort", Guid = new Guid("e7a091a3-b2c1-4903-8768-591a937858ea"), TrackLength = 4252 } },
            {"Zolder",  new TrackData(){FullName ="Circuit Zolder", Guid = new Guid("eaca1a4d-aa7e-4c31-bfc5-6035bfa30395"), TrackLength = 4011 } },
        };


    }
}
