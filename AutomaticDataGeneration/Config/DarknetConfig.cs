using System;

namespace AutomaticDataGeneration.Config
{
    public class DarknetConfig
    {
        public DarknetConfig(string configPath, string weightPath, string namesPath)
        {
            ConfigPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
            WeightPath = weightPath ?? throw new ArgumentNullException(nameof(weightPath));
            NamesPath = namesPath ?? throw new ArgumentNullException(nameof(namesPath));
        }

        public string ConfigPath { get; }
        public string WeightPath { get; }
        public string NamesPath { get; }
    }
}