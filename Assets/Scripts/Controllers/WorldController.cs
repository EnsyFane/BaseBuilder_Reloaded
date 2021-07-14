using BaseBuilder_Reloaded.Scripts.Infrastructure.Exceptions;
using BaseBuilder_Reloaded.Scripts.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BaseBuilder_Reloaded.Scripts.Controllers
{
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; private set; }
        public World World { get; private set; }
        private static string worldSaveFile = string.Empty;

        public bool IsDialogOpen { get; set; }
        private bool _isPaused = false;

        public bool IsPaused
        {
            get => _isPaused || IsDialogOpen;
            set => _isPaused = value;
        }

        public void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("An instance of WorldController already exists.");
                throw new InstanceAlreadyExists<WorldController>("An instance of WorldController already exists.", Instance);
            }

            Instance = this;

            if (string.IsNullOrEmpty(worldSaveFile))
            {
                CreateEmptyWorld();
            }
            else
            {
                CreateWorldFromSaveFile();
                worldSaveFile = string.Empty;
            }
        }

        public void Update()
        {
            // TODO: Add pause, unpause and speed controls.
            if (!IsPaused)
            {
                World.Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// Loads a world from a save file. Reloads the current scene on click.
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadWorldFromFile(string fileName)
        {
            worldSaveFile = fileName;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void CreateEmptyWorld()
        {
            // TODO: Read those values from somewhere (config file for defaults or inputs from user).
            World = new World(100, 100);

            Camera.main.transform.position = new Vector3(
                    World.Width / 2,
                    World.Height / 2,
                    Camera.main.transform.position.z
                );
        }

        private void CreateWorldFromSaveFile()
        {
        }
    }
}