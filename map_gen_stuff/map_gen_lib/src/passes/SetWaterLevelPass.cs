using System.Collections.Generic;
using System.Linq;

namespace MapGen
{
    public class SetWaterLevelPass : IMapGenPass
    {   
        public IEnumerable<string> getMapDataRead()
        {
            return new string[] { CoreDataKeys.MDATA_HEIGHT_MAP_KEY };
        }

        public IEnumerable<string> getMapDataWritten()
        {
            return new string[] { CoreDataKeys.MDATA_WATER_COVERED_CELLS_KEY, CoreDataKeys.MDATA_SEA_LEVEL_HEIGHT_KEY };
        }

        public string getPassDesc()
        {
            return "Filling with water";
        }

        public string getPassName()
        {
            return "SetWaterLevel";
        }

        public IEnumerable<string> getReqMapParams()
        {
            return new string[] { CoreDataKeys.PARAM_WATER_PERC_KEY };
        }

        public void run(IMapInfo mapData)
        {
            var hMap = (HeightMap)mapData.getMapData(CoreDataKeys.MDATA_HEIGHT_MAP_KEY);
            var map_water_perc = (float)mapData.getParamData(CoreDataKeys.PARAM_WATER_PERC_KEY);

            var num_cells_at_height_table = count_num_cells_with_each_height_level(hMap, hMap.maxHeight);
            var water_level = determine_needed_water_level_to_cover_perc_of_map(num_cells_at_height_table, hMap.dim, map_water_perc);
            var cell_is_water_table = determine_cell_map_covered_by_water(hMap, water_level);
            
            mapData.writeMapData(CoreDataKeys.MDATA_WATER_COVERED_CELLS_KEY, cell_is_water_table);
            mapData.writeMapData(CoreDataKeys.MDATA_SEA_LEVEL_HEIGHT_KEY, water_level);
        }

        int[] count_num_cells_with_each_height_level(HeightMap hMap, int maxHeight)
        {
            var num_cells_at_height_table = new int[maxHeight + 1];

            // Do a pass to determine the number of tiles at each possible height
            for (int x = 0; x < hMap.dim.x; x++)
                for (int y = 0; y < hMap.dim.y; y++)
                {
                    var height = hMap.cells[x, y];
                    num_cells_at_height_table[height]++;
                }

            return num_cells_at_height_table;
        }

        int determine_needed_water_level_to_cover_perc_of_map(int[] num_cells_with_height_table, Vector2 mDim, float map_water_perc)
        {
            int needed_water_level = 0;
            int num_tiles_covered = 0;
            int total_cells = mDim.x * mDim.y;
            int tot_cells_that_need_to_be_covered = (int)(total_cells * map_water_perc);

            int curr_height = 0;
            while (num_tiles_covered < tot_cells_that_need_to_be_covered)
            {
                num_tiles_covered += num_cells_with_height_table[curr_height];
                needed_water_level++;
                curr_height++;
            }

            return needed_water_level;
        }

        bool[,] determine_cell_map_covered_by_water(HeightMap hmap, int water_level)
        {
            var cell_is_water_table = new bool[hmap.dim.x, hmap.dim.y];

            for (int x = 0; x < hmap.dim.x; x++)
                for (int y = 0; y < hmap.dim.y; y++)
                {
                    // Note: Bools default to false
                    if (hmap.cells[x, y] <= water_level)
                    {
                        cell_is_water_table[x, y] = true;
                    }
                }

            return cell_is_water_table;
        }
    }
}