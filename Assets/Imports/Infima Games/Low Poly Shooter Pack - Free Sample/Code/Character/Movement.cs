

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(AudioSource))]
    public class Movement : MovementBehaviour
    {
        [SerializeField] AudioClip audioClipWalking;
        [SerializeField] AudioClip audioClipRunning;
        [SerializeField] float speedWalking = 5f;
        [SerializeField] float speedRunning = 9f;
        [SerializeField] float jumpForce = 7f;
        [SerializeField] float groundCheckDistance = 0.2f;

        Rigidbody rb;
        CapsuleCollider capsule;
        AudioSource source;
        CharacterBehaviour playerCharacter;
        bool grounded;
        bool jumpQueued;

        protected override void Awake()
        {
            playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        }

        protected override void Start()
        {
            rb = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            source = GetComponent<AudioSource>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            source.loop = true;
        }

        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                jumpQueued = true;
        }

        protected override void FixedUpdate()
        {
            GroundCheck();

            Vector2 input = playerCharacter.GetInputMovement();
            Vector3 move = transform.TransformDirection(new Vector3(input.x, 0, input.y));
            float speed = playerCharacter.IsRunning() ? speedRunning : speedWalking;
            Vector3 v = rb.linearVelocity;
            v.x = move.x * speed;
            v.z = move.z * speed;
            rb.linearVelocity = v;

            if (grounded && jumpQueued)
            {
                v = rb.linearVelocity;
                v.y = 0;
                rb.linearVelocity = v;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            jumpQueued = false;

            if (grounded && new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).sqrMagnitude > .05f)
            {
                source.clip = playerCharacter.IsRunning() ? audioClipRunning : audioClipWalking;
                if (!source.isPlaying) source.Play();
            }
            else if (source.isPlaying) source.Pause();
        }

        void GroundCheck()
        {
            Bounds b = capsule.bounds;
            float radius = capsule.radius * 0.95f;
            grounded = Physics.SphereCast(b.center, radius, Vector3.down, out _, b.extents.y + groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);
        }
    }
}
