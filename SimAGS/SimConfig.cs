namespace SimAGS
{
    public class SimConfig
    {
        #region singl

        private static SimConfig _config;
        public static SimConfig GetConfig()
        {
            if (_config == null)
            {
                _config = new SimConfig();
            }
            return _config;
        }

        private SimConfig() { }

        #endregion


    }
}
