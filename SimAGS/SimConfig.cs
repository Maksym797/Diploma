namespace SimAGS
{
    public class SimConfig
    {
        #region singl

        private static SimConfig _config;
        public static SimConfig GetConfig()
        {
            return _config ?? new SimConfig();
        }

        private SimConfig() { }

        #endregion
        

    }
}
