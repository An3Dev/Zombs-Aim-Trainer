using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace An3Apps
{
    public class SceneManager : MonoBehaviour
    {
        public static An3Apps.SceneManager Instance;

        private void Start()
        {
            Instance = this;
        }
        public void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
