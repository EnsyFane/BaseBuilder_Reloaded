using Assets.Scripts.Infrastructure.Exceptions;
using Assets.Scripts.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Controllers
{
    /// <summary>
    /// Controller that creates and keeps track of all object in the <see cref="World"/>. (Keeps track of only the data, not the actual GameObjects).
    /// </summary>
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; private set; }
        public World World { get; private set; }

        private static string worldSaveFile = string.Empty;

        public bool IsDialogOpen { get; set; }
        private bool _isWorldPaused = false;

        public bool IsGamePaused
        {
            get => _isWorldPaused || IsDialogOpen;
            set => _isWorldPaused = value;
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
            // USE FOR DEBUGGING ONLY
            //if (Input.GetMouseButtonUp(0))
            //{
            //    World.RandomizeTiles();
            //}

            // TODO: Add pause, unpause and speed controls.
            if (!IsGamePaused)
            {
                World.Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// Loads a world from a save file. Reloads the current scene.
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