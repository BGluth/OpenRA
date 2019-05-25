namespace MapGen
{
    public static class CoreDataKeys
    {
        public static string PARAM_MAP_NAME_KEY = "map_name";
        public static string PARAM_DIM_KEY = "dimensions";
        public static string PARAM_MHEIGHT_KEY = "max_height";
        public static string PARAM_WATER_PERC_KEY = "water_percentage";
        public static string PARAM_CONTINENTIFY_PERC_INFL_ON_CELL = "continentify_infl_perc_on_cell";
        public static string PARAM_SEED_KEY = "seed";

        public static string PARAM_CONTINENT_RAD_KEY = "continent_radius";

        public static string MDATA_HEIGHT_MAP_KEY = "height_map";
        public static string MDATA_WATER_COVERED_CELLS_KEY = "water_covered_cells";
        public static string MDATA_SEA_LEVEL_HEIGHT_KEY = "sea_level_height";
        

        public static string getMapName(IMapInfo info)
        {
            return (string)info.getParamData(PARAM_MAP_NAME_KEY);
        }

        public static HeightMap getHeightMap(IMapInfo info)
        {
            return (HeightMap)info.getMapData(MDATA_HEIGHT_MAP_KEY);
        }
    }

}
