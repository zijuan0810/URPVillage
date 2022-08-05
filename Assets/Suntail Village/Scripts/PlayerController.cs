using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

/*Simple player movement controller, based on character controller component, 
with footstep system based on check the current texture of the component*/
namespace Suntail
{
    public class PlayerController : MonoBehaviour
    {
        //Variables for footstep system list
        [Serializable]
        public class GroundLayer
        {
            public string layerName;
            public Texture2D[] groundTextures;
            public AudioClip[] footstepSounds;
        }

        private const float GRAVITY = -9.81f;

        public TextMeshProUGUI fpsText;
        public FloatingJoystick joystickRight;
        public FloatingJoystick joystickLeft;

        [Header("Movement")]
        [Tooltip("Basic controller speed")]
        [SerializeField]
        private float walkSpeed;

        [Tooltip("Running controller speed")]
        [SerializeField]
        private float runMultiplier;

        [Tooltip("Force of the jump with which the controller rushes upwards")]
        [SerializeField]
        private float jumpForce;

        [Header("Mouse Look")]
        [SerializeField]
        private Camera playerCamera;

        [SerializeField]
        private float mouseSensivity;

        [SerializeField]
        private float mouseVerticalClamp;


        [Header("Footsteps")]
        [Tooltip("Footstep source")]
        [SerializeField]
        private AudioSource footstepSource;

        [Tooltip("Distance for ground texture checker")]
        [SerializeField]
        private float groundCheckDistance = 1.0f;

        [Tooltip("Footsteps playing rate")]
        [SerializeField]
        [Range(1f, 2f)]
        private float footstepRate = 1f;

        [Tooltip("Footstep rate when player running")]
        [SerializeField]
        [Range(1f, 2f)]
        private float runningFootstepRate = 1.5f;

        [Tooltip("Add textures for this layer and add sounds to be played for this texture")]
        public List<GroundLayer> groundLayers = new List<GroundLayer>();

        //Private movement variables
        private float _horizontalMovement;
        private float _verticalMovement;
        private float _currentSpeed;
        private Vector3 _moveDirection;
        private Vector3 _velocity;

        private CharacterController _characterController;
        // private bool _isRunning;

        //Private mouselook variables
        private float _verticalRotation;
        private float _yAxis;
        private float _xAxis;
        private bool _activeRotation;

        //Private footstep system variables
        private Terrain _terrain;
        private TerrainData _terrainData;
        private TerrainLayer[] _terrainLayers;
        private AudioClip _previousClip;
        private Texture2D _currentTexture;
        private RaycastHit _groundHit;
        private float _nextFootstep;

        // for fps calculation.
        private int _frameCount;
        private float _elapsedTime;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            GetTerrainData();
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }

        private void Start()
        {
            fpsText.text = "0";
        }

        private void LateUpdate()
        {
            _frameCount++;
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > 0.5f)
            {
                var frameRate = Math.Round(_frameCount / _elapsedTime, 1, MidpointRounding.AwayFromZero);
                _frameCount = 0;
                _elapsedTime = 0;
                fpsText.text = $"{frameRate}";
            }
        }

        //Getting all terrain data for footstep system
        private void GetTerrainData()
        {
            if (Terrain.activeTerrain)
            {
                _terrain = Terrain.activeTerrain;
                var terrainData = _terrain.terrainData;
                _terrainData = terrainData;
                _terrainLayers = terrainData.terrainLayers;
            }
        }

        private void Update()
        {
            Movement();
            MouseLook();
            GroundChecker();
        }

        //Character controller movement
        private void Movement()
        {
            if (_characterController.isGrounded && _velocity.y < 0)
                _velocity.y = -2f;

            _horizontalMovement = joystickRight.Horizontal; //Input.GetAxis("Horizontal");
            _verticalMovement = joystickRight.Vertical; //Input.GetAxis("Vertical");

            _moveDirection = transform.forward * _verticalMovement + transform.right * _horizontalMovement;

            _currentSpeed = walkSpeed * (Global.IsRunning ? runMultiplier : 1f);
            _characterController.Move(_moveDirection * _currentSpeed * Time.deltaTime);

            _velocity.y += GRAVITY * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        public void Jump()
        {
            if (!_characterController.isGrounded)
                return;

            _velocity.y = Mathf.Sqrt(jumpForce * -2f * GRAVITY);
            _velocity.y += GRAVITY * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void MouseLook()
        {
            _xAxis = joystickLeft.Horizontal * 0.5f; // Input.GetAxis("Mouse X");
            _yAxis = joystickLeft.Vertical * 0.5f; // Input.GetAxis("Mouse Y");

            _verticalRotation += -_yAxis * mouseSensivity;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -mouseVerticalClamp, mouseVerticalClamp);
            playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
            transform.rotation *= Quaternion.Euler(0, _xAxis * mouseSensivity, 0);
        }

        //Playing footstep sound when controller moves and grounded
        private void FixedUpdate()
        {
            if (_characterController.isGrounded && (_horizontalMovement != 0 || _verticalMovement != 0))
            {
                float currentFootstepRate = (Global.IsRunning ? runningFootstepRate : footstepRate);
                if (_nextFootstep >= 100f)
                {
                    {
                        PlayFootstep();
                        _nextFootstep = 0;
                    }
                }

                _nextFootstep += (currentFootstepRate * walkSpeed);
            }
        }


        //Check where the controller is now and identify the texture of the component
        private void GroundChecker()
        {
            Ray checkerRay = new Ray(transform.position + (Vector3.up * 0.1f), Vector3.down);
            if (Physics.Raycast(checkerRay, out _groundHit, groundCheckDistance))
            {
                if (_groundHit.collider.GetComponent<Terrain>())
                    _currentTexture = _terrainLayers[GetTerrainTexture(transform.position)].diffuseTexture;

                if (_groundHit.collider.GetComponent<Renderer>())
                    _currentTexture = GetRendererTexture();
            }
        }

        //Play a footstep sound depending on the specific texture
        private void PlayFootstep()
        {
            for (int i = 0; i < groundLayers.Count; i++)
            {
                for (int k = 0; k < groundLayers[i].groundTextures.Length; k++)
                {
                    if (_currentTexture == groundLayers[i].groundTextures[k])
                        footstepSource.PlayOneShot(RandomClip(groundLayers[i].footstepSounds));
                }
            }
        }

        //Return an array of textures depending on location of the controller on terrain
        private float[] GetTerrainTexturesArray(Vector3 controllerPosition)
        {
            _terrain = Terrain.activeTerrain;
            _terrainData = _terrain.terrainData;
            Vector3 terrainPosition = _terrain.transform.position;

            int positionX = (int)(((controllerPosition.x - terrainPosition.x) / _terrainData.size.x) *
                                  _terrainData.alphamapWidth);
            int positionZ = (int)(((controllerPosition.z - terrainPosition.z) / _terrainData.size.z) *
                                  _terrainData.alphamapHeight);

            float[,,] layerData = _terrainData.GetAlphamaps(positionX, positionZ, 1, 1);
            float[] texturesArray = new float[layerData.GetUpperBound(2) + 1];
            for (int n = 0; n < texturesArray.Length; ++n)
                texturesArray[n] = layerData[0, 0, n];

            return texturesArray;
        }

        //Returns the zero index of the prevailing texture based on the controller location on terrain
        private int GetTerrainTexture(Vector3 controllerPosition)
        {
            float[] array = GetTerrainTexturesArray(controllerPosition);
            float maxArray = 0;
            int maxArrayIndex = 0;

            for (int n = 0; n < array.Length; ++n)
            {
                if (array[n] > maxArray)
                {
                    maxArrayIndex = n;
                    maxArray = array[n];
                }
            }

            return maxArrayIndex;
        }

        //Returns the current main texture of renderer where the controller is located now
        private Texture2D GetRendererTexture()
        {
            return _groundHit.collider.gameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
        }

        //Returns an audio clip from an array, prevents a newly played clip from being repeated and randomize pitch
        private AudioClip RandomClip(AudioClip[] clips)
        {
            footstepSource.pitch = Random.Range(0.9f, 1.1f);

            int attempts = 2;
            AudioClip selectedClip = clips[Random.Range(0, clips.Length)];
            while (selectedClip == _previousClip && attempts > 0)
            {
                selectedClip = clips[Random.Range(0, clips.Length)];
                attempts--;
            }

            _previousClip = selectedClip;

            return selectedClip;
        }
    }
}