using ADOFAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GenerateDecoTiles
{
    public static class CustomLevelHelper
    {
        public static LevelEvent CreateDecoration(LevelEventType eventType)
        {
            LevelEvent levelEvent = new LevelEvent(-1, eventType);
            Vector3 position = Camera.main.transform.position;
            if (scnEditor.instance.selectedFloors.Count == 1)
            {
                levelEvent["relativeTo"] = DecPlacementType.Tile;
                levelEvent.floor = scnEditor.instance.selectedFloors[0].seqID;
                levelEvent["position"] = Vector2.zero;
            }
            else
            {
                levelEvent["position"] = new Vector2(position.x, position.y) / scnEditor.instance.customLevel.GetTileSize();
            }

            return levelEvent;
        }
        public static void AddDecoration(LevelEvent dec, int index = -1)
        {
            int index2 = ((index == -1) ? scnEditor.instance.levelData.decorations.Count : (index + 1));
            scnEditor.instance.levelData.decorations.Insert(index2, dec);
            scrDecorationManager.instance.CreateDecoration(dec, out var _, index);
        }
    }
}
