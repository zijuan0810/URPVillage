using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Interacting with objects and doors
namespace Suntail
{
    public class PlayerInteractions : MonoBehaviour
    {
        private const string TAG_DOOR = "Door";
        private const string TAG_ITEM = "Item";

        [Header("Interaction variables")]
        [Tooltip("Layer mask for interactive objects")]
        [SerializeField]
        private LayerMask interactionLayer;

        [Tooltip("Maximum distance from player to object of interaction")]
        [SerializeField]
        private float interactionDistance = 3f;

        [Tooltip("The player's main camera")]
        [SerializeField]
        private Camera mainCamera;

        [Tooltip("Parent object where the object to be lifted becomes")]
        [SerializeField]
        private Transform m_PickupParent;

        [Header("Object Following")]
        [Tooltip("Minimum speed of the lifted object")]
        [SerializeField]
        private float minSpeed = 0;

        [Tooltip("Maximum speed of the lifted object")]
        [SerializeField]
        private float maxSpeed = 3000f;

        [Header("UI")]
        [SerializeField]
        private Image m_AimWhite;

        [SerializeField]
        private Image m_AimGreen;

        [SerializeField]
        private Button m_OperateButton;

        [SerializeField]
        private TextMeshProUGUI m_OperateText;

        [SerializeField]
        private PointerDownHandler m_RunHandler;

        [SerializeField]
        private Button m_JumpButton;

        //Private variables.
        private PhysicsObject _physicsObject;
        private PhysicsObject _currentlyPickedUpObject;
        private PhysicsObject m_LookObject;
        private Quaternion _lookRotation;
        private Vector3 _raycastPosition;
        private Rigidbody _pickupRigidBody;
        private Door m_LookDoor;
        private float _currentSpeed = 0f;
        private float _currentDistance = 0f;
        private PlayerController m_Player;
        private CharacterController m_Character;


        private void Start()
        {
            mainCamera = Camera.main;
            m_Player = GetComponent<PlayerController>();
            m_Character = GetComponent<CharacterController>();

            m_JumpButton.onClick.AddListener(OnClickJump);
            m_RunHandler.PointerDownHander = OnPointerDown;
            m_RunHandler.PointerUpHander = OnPointerUp;
        }

        private void Update()
        {
            Interactions();
            LegCheck();
        }

        //Determine which object we are now looking at, depending on the tag and component
        private void Interactions()
        {
            _raycastPosition = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(_raycastPosition, mainCamera.transform.forward, out RaycastHit interactionHit, interactionDistance,
                    interactionLayer))
            {
                if (interactionHit.collider.CompareTag(TAG_ITEM))
                {
                    m_LookObject = interactionHit.collider.GetComponentInChildren<PhysicsObject>();
                    ShowItemUI();
                }
                else if (interactionHit.collider.CompareTag(TAG_DOOR))
                {
                    m_LookDoor = interactionHit.collider.gameObject.GetComponentInChildren<Door>();
                    ShowDoorUI();
                }
            }
            else
            {
                m_LookDoor = null;
                m_LookObject = null;
                m_AimGreen.gameObject.SetActive(false);
                m_AimWhite.gameObject.SetActive(true);
                m_OperateButton.gameObject.SetActive(false);
            }
        }

        //Disconnects from the object when the player attempts to step on the object, prevents flight on the object
        private void LegCheck()
        {
            Vector3 spherePosition = m_Character.center + transform.position;
            RaycastHit legCheck;
            if (Physics.SphereCast(spherePosition, 0.3f, Vector3.down, out legCheck, 2.0f))
            {
                if (legCheck.collider.CompareTag(TAG_ITEM))
                    BreakConnection();
            }
        }

        //Velocity movement toward pickup parent
        private void FixedUpdate()
        {
            if (_currentlyPickedUpObject != null)
            {
                _currentDistance = Vector3.Distance(m_PickupParent.position, _pickupRigidBody.position);
                _currentSpeed = Mathf.SmoothStep(minSpeed, maxSpeed, _currentDistance / interactionDistance);
                _currentSpeed *= Time.fixedDeltaTime;
                Vector3 direction = m_PickupParent.position - _pickupRigidBody.position;
                _pickupRigidBody.velocity = direction.normalized * _currentSpeed;
            }
        }

        //Picking up an looking object
        public void PickUpObject()
        {
            _physicsObject = m_LookObject.GetComponentInChildren<PhysicsObject>();
            _currentlyPickedUpObject = m_LookObject;
            _lookRotation = _currentlyPickedUpObject.transform.rotation;
            _pickupRigidBody = _currentlyPickedUpObject.GetComponent<Rigidbody>();
            _pickupRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            _pickupRigidBody.transform.rotation = _lookRotation;
            _physicsObject.playerInteraction = this;
            StartCoroutine(_physicsObject.PickUp());
        }

        //Release the object
        public void BreakConnection()
        {
            if (_currentlyPickedUpObject)
            {
                _pickupRigidBody.constraints = RigidbodyConstraints.None;
                _currentlyPickedUpObject = null;
                _physicsObject.pickedUp = false;
                _currentDistance = 0;
            }
        }

        //Show interface elements when hovering over an object
        private void ShowDoorUI()
        {
            m_AimGreen.gameObject.SetActive(true);
            m_AimWhite.gameObject.SetActive(false);

            m_OperateText.text = m_LookDoor.doorOpen ? "Close" : "Open";
            m_OperateButton.onClick.RemoveAllListeners();
            m_OperateButton.onClick.AddListener(OperateDoor);
            m_OperateButton.gameObject.SetActive(true);
        }

        private void ShowItemUI()
        {
            m_AimGreen.gameObject.SetActive(true);
            m_AimWhite.gameObject.SetActive(false);

            m_OperateText.text = _currentlyPickedUpObject == null ? "Pickup" : "Drop";
            m_OperateButton.onClick.RemoveAllListeners();
            m_OperateButton.onClick.AddListener(OperateItem);
            m_OperateButton.gameObject.SetActive(true);
        }

        private void OperateDoor()
        {
            if (m_LookDoor != null)
                m_LookDoor.PlayDoorAnimation();
        }

        private void OperateItem()
        {
            if (_currentlyPickedUpObject == null)
            {
                if (m_LookObject != null)
                    PickUpObject();
            }
            else
                BreakConnection();
        }
        
        private void OnClickJump()
        {
            m_Player.Jump();
        }
        
        private void OnPointerDown(PointerEventData eventData)
        {
            Global.IsRunning = true;
        }

        private void OnPointerUp(PointerEventData eventData)
        {
            Global.IsRunning = false;
        }
    }
}