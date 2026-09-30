using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Raiz de todo lo que sobrevive al viaje entre mundos: Player de PC, XR Origin, camaras,
    /// HUD, EventSystem y sistemas. Vive en DontDestroyOnLoad; los mundos solo traen contenido.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class PersistentRig : MonoBehaviour
    {
        public Transform DesktopBody;
        public Transform XrRig;
        public Transform XrHead;
        public Transform HoldingPad;

        public static PersistentRig Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Espera de viaje: una losa lejos de todo mundo para que nadie caiga al vacio.</summary>
        public void Hold()
        {
            var spot = HoldingPad != null ? HoldingPad.position + Vector3.up * 0.05f : new Vector3(0f, -500f, 0f);
            PlaceAt(spot, Quaternion.identity);
        }

        /// <summary>
        /// Deja al jugador parado en el punto, mirando hacia su +Z. En VR alinea la cabeza,
        /// no el origen del rig, porque el usuario puede estar corrido dentro de su espacio.
        /// </summary>
        public void PlaceAt(Vector3 position, Quaternion rotation)
        {
            var yaw = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
            if (DesktopBody != null)
                Move(DesktopBody, position, yaw);

            if (XrRig != null)
            {
                if (ClientModeState.Current == ClientMode.Vr && XrHead != null)
                {
                    var headYaw = XrHead.eulerAngles.y - XrRig.eulerAngles.y;
                    var rigRotation = Quaternion.Euler(0f, yaw.eulerAngles.y - headYaw, 0f);
                    Move(XrRig, XrRig.position, rigRotation);
                    var offset = XrHead.position - XrRig.position;
                    offset.y = 0f;
                    Move(XrRig, position - offset, rigRotation);
                }
                else
                {
                    Move(XrRig, position, yaw);
                }
            }

            Physics.SyncTransforms();
        }

        // Con el CharacterController prendido, Unity pisa el cambio de posicion en el frame siguiente.
        static void Move(Transform target, Vector3 position, Quaternion rotation)
        {
            var body = target.GetComponent<CharacterController>();
            var wasEnabled = body != null && body.enabled;
            if (wasEnabled)
                body.enabled = false;
            target.SetPositionAndRotation(position, rotation);
            if (wasEnabled)
                body.enabled = true;
        }
    }
}
