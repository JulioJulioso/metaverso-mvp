using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace Metaverso
{
    /// <summary>
    /// Secuencia de muros. El boton del HUD dispara el Timeline; la senal llama a Rise().
    /// </summary>
    public class WallAssembly : MonoBehaviour
    {
        public float RiseDuration = 4.5f;
        public float ExplodeDuration = 1.4f;
        public float ExplodeDistance = 1.15f;
        public float PromptRadius = 5.5f;
        public string RiseStepId = "walls_rise";
        public string ExplodeStepId = "walls_explode";
        public CircuitTracker Circuit;
        public MetaversoHud Hud;
        public PlayableDirector Director;

        Transform[] _panels;
        Vector3[] _restLocal;
        bool _rising;
        bool _risen;
        bool _exploded;

        public bool IsRisen => _risen;
        public bool IsExploded => _exploded;

        void Awake()
        {
            var count = transform.childCount;
            _panels = new Transform[count];
            _restLocal = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                _panels[i] = transform.GetChild(i);
                _restLocal[i] = _panels[i].localPosition;
                var hidden = _restLocal[i];
                hidden.y = -Mathf.Abs(_restLocal[i].y);
                _panels[i].localPosition = hidden;
            }
        }

        public bool IsPlayerNear(Vector3 playerPosition)
        {
            var flat = playerPosition - transform.position;
            flat.y = 0f;
            return flat.magnitude <= PromptRadius;
        }

        public void PlayRise()
        {
            if (Director != null)
            {
                Director.time = 0d;
                Director.Play();
                return;
            }

            Rise();
        }

        public void Rise()
        {
            if (_rising || _risen)
            {
                Hud?.ShowMessage(_risen ? "Los muros ya estan levantados." : "Levantamiento en curso.");
                return;
            }

            StartCoroutine(RiseRoutine());
        }

        public void ToggleExplode()
        {
            if (!_risen)
            {
                Hud?.ShowMessage("Primero levanta los muros.");
                return;
            }

            StartCoroutine(ExplodeRoutine(!_exploded));
        }

        IEnumerator RiseRoutine()
        {
            _rising = true;
            Hud?.ShowMessage("Levantamiento de muros en curso.");
            var start = new Vector3[_panels.Length];
            for (var i = 0; i < _panels.Length; i++)
                start[i] = _panels[i].localPosition;

            var t = 0f;
            while (t < RiseDuration)
            {
                t += Time.deltaTime;
                var u = Mathf.Clamp01(t / RiseDuration);
                for (var i = 0; i < _panels.Length; i++)
                    _panels[i].localPosition = Vector3.Lerp(start[i], _restLocal[i], u);
                yield return null;
            }

            _rising = false;
            _risen = true;
            Circuit?.Complete(RiseStepId);
            Hud?.ShowMessage("Muros levantados.");
        }

        IEnumerator ExplodeRoutine(bool opening)
        {
            var from = new Vector3[_panels.Length];
            var to = new Vector3[_panels.Length];
            for (var i = 0; i < _panels.Length; i++)
            {
                from[i] = _panels[i].localPosition;
                var sign = i - (_panels.Length - 1) * 0.5f;
                var offset = opening ? Vector3.right * sign * ExplodeDistance : Vector3.zero;
                to[i] = _restLocal[i] + offset;
            }

            var t = 0f;
            while (t < ExplodeDuration)
            {
                t += Time.deltaTime;
                var u = Mathf.Clamp01(t / ExplodeDuration);
                for (var i = 0; i < _panels.Length; i++)
                    _panels[i].localPosition = Vector3.Lerp(from[i], to[i], u);
                yield return null;
            }

            _exploded = opening;
            if (opening)
                Circuit?.Complete(ExplodeStepId);
            Hud?.ShowMessage(opening ? "Vision explotada." : "Muros reensamblados.");
        }
    }
}
