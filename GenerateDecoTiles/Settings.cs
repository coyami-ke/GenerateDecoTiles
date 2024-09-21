using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace GenerateDecoTiles
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            string path = Path.Combine(modEntry.Path, "Settings.xml");
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                new XmlSerializer(base.GetType()).Serialize(streamWriter, this);
            }
        }
        public int firstParallax = 0;
        public int secondParallax = 0;
        public int seed = 0;
        public int count = 25;
    }
}
