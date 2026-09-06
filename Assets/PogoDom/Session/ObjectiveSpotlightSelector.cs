using PogoDom.Meta;

namespace PogoDom.Session
{
    public sealed class ObjectiveSpotlight
    {
        public string ObjectiveId { get; }
        public ObjectiveMetric Metric { get; }
        public int Current { get; }
        public int Target { get; }

        public ObjectiveSpotlight(ObjectiveState objective)
        {
            ObjectiveId = objective.Id;
            Metric = objective.Metric;
            Current = objective.Current;
            Target = objective.Target;
        }
    }

    public static class ObjectiveSpotlightSelector
    {
        public static ObjectiveSpotlight Select(WorldState world, CityId cityId, NationId nationId)
        {
            ObjectiveState best = null;
            double bestScore = double.NegativeInfinity;

            for (var i = 0; i < world.Objectives.Count; i++)
            {
                var objective = world.Objectives[i];
                if (objective.IsComplete || !Matches(objective, cityId, nationId)) continue;

                var ratio = objective.Current / (double)objective.Target;
                var startedBonus = objective.Current > 0 ? 0.2 : 0.0;
                var cityBonus = objective.Scope == ObjectiveScope.City ? 0.03 : 0.0;
                var score = ratio + startedBonus + cityBonus;

                if (best == null || score > bestScore)
                {
                    best = objective;
                    bestScore = score;
                }
            }

            return best == null ? null : new ObjectiveSpotlight(best);
        }

        private static bool Matches(ObjectiveState objective, CityId cityId, NationId nationId)
        {
            return objective.Scope == ObjectiveScope.City
                ? objective.ScopeId == cityId.Value
                : objective.ScopeId == nationId.Value;
        }
    }
}
