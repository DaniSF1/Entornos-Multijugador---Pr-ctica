using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace Movement.Components
{
    [RequireComponent(typeof(Rigidbody2D)), 
     RequireComponent(typeof(Animator)),
     RequireComponent(typeof(NetworkObject))]
    public sealed class FighterMovement : NetworkBehaviour, IMoveableReceiver, IJumperReceiver, IFighterReceiver
    {
        public float speed = 1.0f;
        public float jumpAmount = 1.0f;
        public float health = 100.0f;
        public float damage = 10.0f;
        public bool dead = false;

        private Rigidbody2D _rigidbody2D;           //RigidBody del personaje
        private Animator _animator;                 //Definimos un animador
        private NetworkAnimator _networkAnimator;   //Definimos un network animator
        private Transform _feet;                    //Definimos la parte inferior del personaje
        private LayerMask _floor;                   //Definimos el suelo

        private Vector3 _direction = Vector3.zero;  //Dirección del personaje
        private bool _grounded = true;              //Si el personaje está en el suelo o no
        
        //Animaciones del personaje
        private static readonly int AnimatorSpeed = Animator.StringToHash("speed");
        private static readonly int AnimatorVSpeed = Animator.StringToHash("vspeed");
        private static readonly int AnimatorGrounded = Animator.StringToHash("grounded");
        private static readonly int AnimatorAttack1 = Animator.StringToHash("attack1");
        private static readonly int AnimatorAttack2 = Animator.StringToHash("attack2");
        private static readonly int AnimatorHit = Animator.StringToHash("hit");
        private static readonly int AnimatorDie = Animator.StringToHash("die");

        void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();         //Tomamos el rigidBody del personaje
            _animator = GetComponent<Animator>();               //Cogemos su componente animador
            _networkAnimator = GetComponent<NetworkAnimator>(); //Cogemos el network animator

            _feet = transform.Find("Feet");                     //Cogemos el objeto de tipo Feet
            _floor = LayerMask.GetMask("Floor");                //Cogemos la parte de la escena correspondiente al suelo
        }

        void Update()
        {
            if (!IsServer) return;
            _grounded = Physics2D.OverlapCircle(_feet.position, 0.1f, _floor);
            _animator.SetFloat(AnimatorSpeed, this._direction.magnitude);
            _animator.SetFloat(AnimatorVSpeed, this._rigidbody2D.velocity.y);
            _animator.SetBool(AnimatorGrounded, this._grounded);

        }

        void FixedUpdate()
        {
            _rigidbody2D.velocity = new Vector2(_direction.x, _rigidbody2D.velocity.y);
        }

        public void Move(IMoveableReceiver.Direction direction)
        {
            if (dead) return;
            MoveServerRpc(direction);
            /*
            //if (direction == IMoveableReceiver.Direction.None)  //Si estamos quietos no nos movemos
            //{
            //    this._direction = Vector3.zero;
            //    return;
            //}

            //bool lookingRight = direction == IMoveableReceiver.Direction.Right; //Vemos la dirección a la que mira el personaje
            //_direction = (lookingRight ? 1f : -1f) * speed * Vector3.right;     //En funcion de dicha direccion, hacemos que el personaje se mueva
            //transform.localScale = new Vector3(lookingRight ? 1 : -1, 1, 1);    //Cambiamos la direccion del sprite en función de la direccion de movimiento
            */
        }

        [ServerRpc]
        public void MoveServerRpc(IMoveableReceiver.Direction direction)
        {
            if (dead) return;
            if (direction == IMoveableReceiver.Direction.None)  //Si estamos quietos no nos movemos
            {
                this._direction = Vector3.zero;
                return;
            }

            bool lookingRight = direction == IMoveableReceiver.Direction.Right; //Vemos la dirección a la que mira el personaje
            _direction = (lookingRight ? 1f : -1f) * speed * Vector3.right;     //En funcion de dicha direccion, hacemos que el personaje se mueva
            transform.localScale = new Vector3(lookingRight ? 1 : -1, 1, 1);    //Cambiamos la direccion del sprite en función de la direccion de movimiento
        }

        public void Jump(IJumperReceiver.JumpStage stage)
        {
            if (dead) return;
            JumpServerRpc(stage);
            /*
            switch (stage)
            {
                case IJumperReceiver.JumpStage.Jumping:     //Si el personaje esta saltando...
                    if (_grounded)                          //y esta en el suelo...
                    {
                        float jumpForce = Mathf.Sqrt(jumpAmount * -2.0f * (Physics2D.gravity.y * _rigidbody2D.gravityScale));
                        _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);     //Hacemos que el personaje salte
                    }
                    break;
                case IJumperReceiver.JumpStage.Landing:     //Si está cayendo, salimos del método
                    break;
            }
            */
        }

        [ServerRpc]
        public void JumpServerRpc(IJumperReceiver.JumpStage stage)
        {
            if (dead) return;
            switch (stage)
            {
                case IJumperReceiver.JumpStage.Jumping:     //Si el personaje esta saltando...
                    if (_grounded)                          //y esta en el suelo...
                    {
                        float jumpForce = Mathf.Sqrt(jumpAmount * -2.0f * (Physics2D.gravity.y * _rigidbody2D.gravityScale));
                        _rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);     //Hacemos que el personaje salte
                    }
                    break;
                case IJumperReceiver.JumpStage.Landing:     //Si está cayendo, salimos del método
                    break;
            }
        }

        public void Attack1()
        {
            //Attack1ServerRpc();
            _networkAnimator.SetTrigger(AnimatorAttack1);
        }

        public void Attack2()
        {
            //Attack2ServerRpc();
            _networkAnimator.SetTrigger(AnimatorAttack2);
        }

        public void TakeHit()
        {
            //TakeHitServerRpc();
            if (dead) return;
            health -= damage;
            Debug.Log($"Other player's healt: {health}");
            _networkAnimator.SetTrigger(AnimatorHit);
            if(health <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            dead = true;
            _networkAnimator.SetTrigger(AnimatorDie);
        }
    }
}