using UnityEngine;

namespace PixelArtCamera
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class PixelArtCamera : MonoBehaviour
    {
        [Header("Dependencies")]

        [Tooltip("The camera that will render the game.")]
        public Camera cam;

        [Tooltip("The shader used to upscale the game.")]
        public Shader shader;

        [Header("Parameters")]

        [Tooltip("The target resolution at which the game should be rendered.")]
        public Vector2Int gameRes = new Vector2Int(320, 180);

        [Tooltip("How many pixels in the sprite correspond to a unit in the world.\nShould be equals to the one use for the sprites.")]
        public float pixelPerUnit = 32;

        [Tooltip("The color used by the shader to fill the remaining space on the screen (if any).")]
        public Color outerColor;

        #region Internal Logic

        /// <summary>
        /// The actual resolution on which the game is being displayed.
        /// </summary>
        private Vector2Int _screenRes;

        /// <summary>
        /// How much the game will be upscaled to fit the screen as much as possible.
        /// </summary>
        private int _scale;

        /// <summary>
        /// The resolution of the game after being upscaled.
        /// </summary>
        private Vector2Int _scaledGameRes;

        /// <summary>
        /// The portion of the screen where the game will be rendered.
        /// </summary>
        private RectInt _gameRect;

        /// <summary>
        /// Material used to apply the shader.
        /// </summary>
        private Material _material;

        /// <summary>
        /// RenderTexture used to render the game at the target resolution.
        /// </summary>
        private RenderTexture _rt;

        // Shader Properties
        private static readonly int RectProperty = Shader.PropertyToID("_Rect");
        private static readonly int ScaleProperty = Shader.PropertyToID("_Scale");
        private static readonly int GameResProperty = Shader.PropertyToID("_GameRes");
        private static readonly int BackgroundColorProperty = Shader.PropertyToID("_BackgroundColor");

        #endregion

        private void Awake()
        {
            // Grab Camera
            if (cam is null) cam = GetComponent<Camera>();

            // Grab Shader
            if (shader is null) shader = Shader.Find("Pole/PixelArtCamera");

            if (!CheckDependencies())
            {
                enabled = false;
                Debug.LogWarning("Pixel Art Camera is missing some dependencies");
            }
            else
            {
                SetUp();
            }
        }

        private void OnValidate()
        {
            // Validate parameters
            gameRes.x = Mathf.Max(2, gameRes.x);
            gameRes.y = Mathf.Max(2, gameRes.y);
            pixelPerUnit = Mathf.Max(0.0001f, pixelPerUnit);

            if (!CheckDependencies())
            {
                enabled = false;
                Debug.LogWarning("Pixel Art Camera is missing some dependencies");
            }
            else
            {
                SetUp();
            }
        }

        private void Update()
        {
            if (!CheckDependencies())
            {
                enabled = false;
                Debug.LogWarning("Pixel Art Camera is missing some dependencies");
            }
            else if (CheckForChanges()) SetUp();
        }

        private bool CheckDependencies()
        {
            // Check that both Camera and Shader are assigned
            return !(cam is null || shader is null);
        }

        private bool CheckForChanges()
        {
            // Check for screen changes
            return _screenRes.x != cam.pixelWidth || _screenRes.y != cam.pixelHeight;
        }

        private void OnPreRender()
        {
            // Request temporary Render Texture
            _rt = RenderTexture.GetTemporary(gameRes.x, gameRes.y, 16);
            // Set FilterMode to Point to correctly render every pixel
            _rt.filterMode = FilterMode.Point;

            // Set Camera to target the RenderTexture
            cam.targetTexture = _rt;

            // Correct Camera aspect 
            cam.aspect = _rt.width * 1f / _rt.height;
        }

        private void OnPostRender()
        {
            // Settin targetTexture to null will render what the camera sees directly to screen
            cam.targetTexture = null;

            // Copies source texture into destination render texture using submitted material
            Graphics.Blit(_rt, null, _material);

            // Release the RenderTexture
            _rt.Release();
            _rt = null;
        }

        private void SetUp()
        {
            // Perform some calculations in order to set up both Camera and Shader

            // Read actual screen size
            _screenRes.x = cam.pixelWidth;
            _screenRes.y = cam.pixelHeight;

            // Set Up Camera
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;

            // Set Camera ortographic size
            cam.orthographicSize = gameRes.y / 2f / pixelPerUnit;

            // Compute the maximun valid scale which the gameRes can be upscaled to while 
            // still fitting the actual resolution of the Game Window
            _scale = Mathf.Min(
                (int)(1f * cam.pixelWidth / gameRes.x),
                (int)(1f * cam.pixelHeight / gameRes.y)
            );

            if (_scale == 0)
            {
                Debug.LogWarning("Target GameRes will not fit at the current resolution");
                // Set scale to 1 so at least something it's rendered
                _scale = 1;
            }

            // Compute the resolution at which our games will be upscaled
            _scaledGameRes = gameRes * _scale;

            // Compute the area in which the upscaled game screen will be placed into the actual screen
            _gameRect.x = (_screenRes.x - _scaledGameRes.x) / 2;
            _gameRect.y = (_screenRes.y - _scaledGameRes.y) / 2;
            _gameRect.width = _scaledGameRes.x;
            _gameRect.height = _scaledGameRes.y;

            // Generate a Material to use the Shader
            if (_material is null)
            {
                _material = new Material(shader);
                _material.shader = shader;
            }

            // Set up Shader with computed values 
            _material.SetFloat(ScaleProperty, _scale);
            _material.SetVector(RectProperty, new Vector4(_gameRect.x, _gameRect.y, _gameRect.max.x, _gameRect.max.y));
            _material.SetVector(GameResProperty, new Vector4(gameRes.x, gameRes.y));
            _material.SetColor(BackgroundColorProperty, outerColor);
        }
    }
}
