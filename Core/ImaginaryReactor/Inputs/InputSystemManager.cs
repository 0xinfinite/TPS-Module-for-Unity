using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImaginaryReactor
{
    public class InputSystemManager : MonoBehaviour
    {
        public static InputSystemManager instance;
        public ThirdPersonControlInput input;

        // Start is called before the first frame update
        void Start()
        {
            if(instance == null) 
            { 
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }
}
