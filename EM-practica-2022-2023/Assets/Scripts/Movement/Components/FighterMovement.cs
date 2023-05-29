using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
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
        public NetworkVariable<float> health = new NetworkVariable<float>();        //Creamos la variable de vida como una NetworkVariable. De este modo, la variable se comparte entre cliente y servidor
        public float damage = 10.0f;                                                //Creamos el daño que hacen los jugadores. Siempre se harán golpes de 10 de daño
        public NetworkVariable<bool> dead = new NetworkVariable<bool>();            //Creamos un booleano para saber si el jugador esta muerto o no. También la compartimos entre cliente y servidor para
                                                                                    //facilicar futura funcionalidad.

        private Rigidbody2D _rigidbody2D;           //RigidBody del personaje
        private Animator _animator;                 //Definimos un animador
        private NetworkAnimator _networkAnimator;   //Definimos un network animator
        private Transform _feet;                    //Definimos la parte inferior del personaje
        private LayerMask _floor;                   //Definimos el suelo

        private Vector3 _direction = Vector3.zero;  //Dirección del personaje
        private bool _grounded = true;              //Si el personaje está en el suelo o no

        public healthBar healthbar;                 //Barra de vida del personaje. La actualizaremos cuando cambie la vida

        //Animaciones del personaje
        private static readonly int AnimatorSpeed = Animator.StringToHash("speed");
        private static readonly int AnimatorVSpeed = Animator.StringToHash("vspeed");
        private static readonly int AnimatorGrounded = Animator.StringToHash("grounded");
        private static readonly int AnimatorAttack1 = Animator.StringToHash("attack1");
        private static readonly int AnimatorAttack2 = Animator.StringToHash("attack2");
        private static readonly int AnimatorHit = Animator.StringToHash("hit");
        private static readonly int AnimatorDie = Animator.StringToHash("die");


        void Awake()
        {
            health.Value = 100f;                            //Establecemos en 100 la vida inicial de los personajes.
            health.OnValueChanged += HealthChange;          //Cuando cambie dicha vida, se llamará a la funcion HealthChange. OnValueChanged es un delegado
            GameManager.onGameRestart += RestartHealth;     //Cuando el GameManager ordena reiniciar el juego, llamamos a la funcion que reestablece la vida
            dead.Value = false;                             //Establecemos que los jugadores empiecen vivos.
        }

        private void HealthChange(float previousValue, float newValue)  //Si health cambia, llamamos a esta funcion
        {
            healthbar.setHealth(newValue);                  //Introducimos en la barra de vida la vida resultante despues de recibir daño. Actualizamos la barra de vida
        }

        private void RestartHealth()                        //Cuando reiniciamos la partida, reestablecemos todas las variables.
        {
            health.Value = healthbar.slider.maxValue;       //Volvemos a rellenar la vida (y por tanto la barra de vida)

            if (dead.Value == true)                         //Si el jugador está muerto...
            {
                dead.Value = false;                         //Establecemos que deje de estar muerto
                _animator.SetTrigger("die");                //Pasamos a la animacion Idle con este trigger
            }
        }

        void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();         //Tomamos el rigidBody del personaje
            _animator = GetComponent<Animator>();               //Cogemos su componente animador
            _networkAnimator = GetComponent<NetworkAnimator>(); //Cogemos el network animator

            _feet = transform.Find("Feet");                     //Cogemos el objeto de tipo Feet
            _floor = LayerMask.GetMask("Floor");                //Cogemos la parte de la escena correspondiente al suelo


            healthbar.SetMaxHealth(health.Value);               //Establecemos la vida maxima de los jugadores en el Start. Fijamos el valor maximo de la vida 
        }

        void Update()
        {
            if (!IsServer) return;                              //Queremos que el servidor sea autoritativo. Calculamos todo en el servidor para verterlo en los clientes posteriormente
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
            if (dead.Value) return;         //Si el jugador está muerto, hacemos que no se pueda mover
            MoveServerRpc(direction);       //Llamamos a MoveServerRpc para que los calculos del movimiento se hagan solo en el servidor.
        }

        [ServerRpc]
        public void MoveServerRpc(IMoveableReceiver.Direction direction)        //Calculo de movimiento en el servidor
        {
            if (direction == IMoveableReceiver.Direction.None)                  //Si estamos quietos el personaje no hace ningun movimiento
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
            if (dead.Value) return;         //De forma análoga al movimiento, si estamos muertos no podemos saltar
            JumpServerRpc(stage);           //y hacemos los cálculos de todo en el servidor en caso de estar vivos
        }

        [ServerRpc]
        public void JumpServerRpc(IJumperReceiver.JumpStage stage)
        {
            switch (stage)
            {
                case IJumperReceiver.JumpStage.Jumping:     //Si el personaje hace la accion de saltar...
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
            if (dead.Value) return;         //Si estamos muertos, no podemos atacar. Salimos del metodo.
            _networkAnimator.SetTrigger(AnimatorAttack1);
        }

        public void Attack2()
        {
            if (dead.Value) return;         //Si estamos muertos, no podemos atacar. Salimos del metodo.
            _networkAnimator.SetTrigger(AnimatorAttack2);
        }

        public void TakeHit()
        {
            if(!IsServer) return;           //Calculamos los golpes en el servidor.
            health.Value -= damage;         //Restamos la vida al jugador en funcion del daño
            Debug.Log($"Other player's healt: {health}");

            if(health.Value <= 0)           //Si la vida resultante es 0 o menos, el jugador muere
            {
                Die();
            }
            else                            //Si no, el jugador hace la animacion de recibir daño
            {
                _networkAnimator.SetTrigger(AnimatorHit);
            }
        }

        public void Die()
        {
            if (dead.Value == true) return; //Si el jugador esta ya muerto, salimos del metodo.
            dead.Value = true;              //En caso de no estarlo, ponemos a true la variable de estar muerto
            _networkAnimator.SetTrigger(AnimatorDie);       //Hacemos la animacion de morir
            GameManager.RemoveDeadPlayer(this.gameObject);  //Sacamos al jugador de la lista de jugadores vivos. Se explica en mas detalle en el GameManager
        }

    }
}