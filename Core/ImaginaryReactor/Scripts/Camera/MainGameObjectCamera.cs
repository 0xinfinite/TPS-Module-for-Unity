

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImaginaryReactor
{
    public class MainGameObjectCamera : MonoBehaviour
    {
        public static Camera Instance;

        void Awake()
        {
            Instance = GetComponent<UnityEngine.Camera>();
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}