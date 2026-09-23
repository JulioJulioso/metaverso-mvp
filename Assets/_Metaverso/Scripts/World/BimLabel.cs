using UnityEngine;

namespace Metaverso
{
    /// <summary>
    /// Ficha que se muestra al hacer click sobre un elemento. Reemplaza al bim-index del MVP web.
    /// </summary>
    public class BimLabel : MonoBehaviour
    {
        public string ElementName;
        public string Category;
        public string Note;

        public string Describe()
        {
            var text = ElementName;
            if (!string.IsNullOrEmpty(Category))
                text += " · " + Category;
            if (!string.IsNullOrEmpty(Note))
                text += " · " + Note;
            return text;
        }
    }
}
