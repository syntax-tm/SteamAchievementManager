using SAM.API.Types;

namespace SAM.API.Stats
{
    public class AvgRateStatDefinition : FloatStatDefinition
    {
        public AvgRateStatDefinition()
        {

        }

        public AvgRateStatDefinition(KeyValue stat, string currentLanguage) : base(stat, currentLanguage)
        {

        }
    }
}
