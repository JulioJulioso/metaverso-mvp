using System;
using System.Collections.Generic;

namespace Metaverso
{
    /// <summary>
    /// Pasos del circuito y cuales estan hechos. No depende de Unity para poder testearlo.
    /// </summary>
    public sealed class CircuitTracker
    {
        readonly List<string> _steps = new List<string>();
        readonly Dictionary<string, string> _labels = new Dictionary<string, string>();
        readonly HashSet<string> _done = new HashSet<string>();

        public event Action Changed;

        public IReadOnlyList<string> Steps => _steps;
        public int CompletedCount => _done.Count;
        public bool AllDone => _steps.Count > 0 && _done.Count == _steps.Count;

        public void SetSteps(IEnumerable<string> steps)
        {
            _steps.Clear();
            _labels.Clear();
            _done.Clear();
            if (steps != null)
                _steps.AddRange(steps);
            Changed?.Invoke();
        }

        public void SetSteps(IEnumerable<KeyValuePair<string, string>> stepsWithLabels)
        {
            _steps.Clear();
            _labels.Clear();
            _done.Clear();
            if (stepsWithLabels != null)
            {
                foreach (var pair in stepsWithLabels)
                {
                    _steps.Add(pair.Key);
                    _labels[pair.Key] = pair.Value;
                }
            }
            Changed?.Invoke();
        }

        public string LabelFor(string id)
        {
            return id != null && _labels.TryGetValue(id, out var label) ? label : id;
        }

        public bool IsDone(string id) => id != null && _done.Contains(id);

        public bool Complete(string id)
        {
            if (string.IsNullOrEmpty(id) || !_steps.Contains(id) || !_done.Add(id))
                return false;
            Changed?.Invoke();
            return true;
        }
    }
}
