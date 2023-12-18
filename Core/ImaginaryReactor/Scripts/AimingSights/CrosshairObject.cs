using UnityEngine;

namespace ImaginaryReactor
{
    public class CrosshairObject : MonoBehaviour
    {
        public static Transform Target;

        // Start is called before the first frame update
        void Awake()
        {
            Target = this.transform;
        }

    }
}