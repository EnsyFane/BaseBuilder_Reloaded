using Assets.Scripts.Models;
using Assets.Scripts.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Controllers
{
    public class MouseController : MonoBehaviour
    {
        public enum MouseMode
        {
            Select,
            Build
        }

        public GameObject cursorPrefab;

        [Min(1f)]
        public float edgeScrollSpeed;

        [Range(0.0001f, 0.9999f)]
        public float edgeScrollThreshold;

        public Tile TileUnderCursor
        {
            get => World.Instance.GetTileAt(Mathf.FloorToInt(CurrentMousePosition.x), Mathf.FloorToInt(CurrentMousePosition.y));
        }

        private Vector3 lastFrameMousePosition;
        private Vector3 currentFrameMousePosition;
        public Vector3 CurrentMousePosition { get => currentFrameMousePosition; }

        private Vector3 dragStartPosition;
        private List<GameObject> dragPreviewGameObjects;
        private bool isDragging;

        private MouseMode currentMouseMode = MouseMode.Select;

        private FurnitureSpriteController furnitureSpriteController;

        public void Start()
        {
            furnitureSpriteController = FindObjectOfType<FurnitureSpriteController>();

            dragPreviewGameObjects = new List<GameObject>();
        }

        public void Update()
        {
            if (WorldController.Instance.IsDialogOpen)
            {
                return;
            }

            currentFrameMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentFrameMousePosition.z = 0;

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (currentMouseMode != MouseMode.Select)
                {
                    currentMouseMode = MouseMode.Select;
                }
                else
                {
                    // TODO: Show game menu
                }
            }

            UpdateDragging();
            UpdateCamera();
            UpdateSelection();

            // A new positio is grabbed in case the user moved the camera.
            lastFrameMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lastFrameMousePosition.z = 0;
        }

        private void UpdateDragging()
        {
            // If the user is over an UI element don't continue.
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            while (dragPreviewGameObjects.Count > 0)
            {
                var gameObject = dragPreviewGameObjects[0];
                dragPreviewGameObjects.RemoveAt(0);
                SimplePool.Despawn(gameObject);
            }

            if (currentMouseMode == MouseMode.Build)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    dragStartPosition = CurrentMousePosition;
                    isDragging = true;
                }
                else if (isDragging == false)
                {
                    dragStartPosition = CurrentMousePosition;
                }

                if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape))
                {
                    isDragging = false;
                }

                // TODO: add further mouse logic here. (line 198)
            }
        }

        private void UpdateCamera()
        {
            var camera = Camera.main;

            if (Input.GetMouseButton(2))
            {
                var cameraDelta = lastFrameMousePosition - currentFrameMousePosition;
                camera.transform.Translate(cameraDelta);
            }
            else
            {
                var mousePosition = camera.ScreenToViewportPoint(Input.mousePosition);
                var toMove = Vector2.zero;
                if (mousePosition.x < edgeScrollThreshold)
                {
                    toMove.x = -1f;
                }
                else if (mousePosition.x > 1 - edgeScrollThreshold)
                {
                    toMove.x = 1f;
                }

                if (mousePosition.y < edgeScrollThreshold)
                {
                    toMove.y = -1f;
                }
                else if (mousePosition.y > 1 - edgeScrollThreshold)
                {
                    toMove.y = 1f;
                }

                camera.transform.Translate(toMove * edgeScrollSpeed * Time.deltaTime);
            }

            camera.orthographicSize -= camera.orthographicSize * Input.GetAxis("Mouse ScrollWheel");

            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, 3f, 25f);
        }

        private void UpdateSelection()
        {
        }
    }
}