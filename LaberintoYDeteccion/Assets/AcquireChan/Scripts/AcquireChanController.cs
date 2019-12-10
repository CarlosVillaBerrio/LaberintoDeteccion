using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/*!
 *	----------------------------------------------------------------------
 *	@brief	アクワイアちゃん簡易操作スクリプト
 *	
*/
public class AcquireChanController : MonoBehaviour
{
	// Inspector
	[SerializeField] private float	m_WalkSpeed		= 2.0f;
	[SerializeField] private float	m_RunSpeed		= 3.5f;
	[SerializeField] private float	m_RotateSpeed	= 8.0f;
	[SerializeField] private float	m_JumpForce		= 400.0f;
	[SerializeField] private float	m_RunningStart	= 1.0f;

    [Header("(Mov en X = 1)Horizontal y (Mov en Z = 1)Vertical")] // MODIFICACION
    public float mHorizontal;
    public float mVertical;
    public float mSentido = 1;

	// member
	private Rigidbody	m_RigidBody	= null;
	private Animator	m_Animator	= null;
	private float		m_MoveTime	= 0;
	private float		m_MoveSpeed	= 0.0f;
	private bool		m_IsGround	= true;

    // MODIFICACION
    [SerializeField] private float sightRange = 10;
    [SerializeField] private Transform playerTransform;
    private Transform _enemyTransform;
    [SerializeField] private GameObject warningGameObject;
    [SerializeField] private GameObject detectGameObject;
    AudioSource activarVozUnityChan;

	/*!
	 *	----------------------------------------------------------------------
	 *	@brief	生成
	*/
	private void Awake()
	{
		m_RigidBody = this.GetComponentInChildren<Rigidbody>();
		m_Animator = this.GetComponentInChildren<Animator>();
		m_MoveSpeed = m_WalkSpeed;
        _enemyTransform = transform;
	}

    /*!
	 *	----------------------------------------------------------------------
	 *	@brief	初期化
	*/
    //	private void Start()
    //	{
    //	}

    /*!
	 *	----------------------------------------------------------------------
	 *	@brief	更新
	*/

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Pared")
        {
            mSentido *= -1;
        }
    }

    private void Update()
	{
		if( null == m_RigidBody ) return;
		if( null == m_Animator ) return;

		// check ground
		float rayDistance = 0.3f;
		Vector3 rayOrigin = (this.transform.position + (Vector3.up * rayDistance * 0.5f));
		bool ground = Physics.Raycast( rayOrigin, Vector3.down, rayDistance, LayerMask.GetMask( "Default" ) );
		if( ground != m_IsGround )
		{
			m_IsGround = ground;

			// landing
			if( m_IsGround )
			{
				m_Animator.Play( "landing" );
			}
		}

		// input
		Vector3 vel = m_RigidBody.velocity;
		// float h = Input.GetAxis( "Horizontal" ); MODIFICACION: Para que se mueva sola y vigile
		float h = mHorizontal * mSentido;
		// float v = Input.GetAxis( "Vertical" );
		float v = mVertical * mSentido;
		bool isMove = ((0 != h) || (0 != v));

		m_MoveTime = isMove? (m_MoveTime + Time.deltaTime) : 0;
		// bool isRun = (m_RunningStart <= m_MoveTime); MODIFICACION: NO VA A CORRER!
		bool isRun = false;

		// move speed (walk / run)
		float moveSpeed = isRun? m_RunSpeed : m_WalkSpeed;
		m_MoveSpeed = isMove? Mathf.Lerp( m_MoveSpeed, moveSpeed, (8.0f * Time.deltaTime) ) : m_WalkSpeed;
//		m_MoveSpeed = moveSpeed;

		Vector3 inputDir = new Vector3( h, 0, v );
		if( 1.0f < inputDir.magnitude ) inputDir.Normalize();

		if( 0 != h ) vel.x = (inputDir.x * m_MoveSpeed);
		if( 0 != v ) vel.z = (inputDir.z * m_MoveSpeed);

		m_RigidBody.velocity = vel;

		if( isMove )
		{
			// rotation
			float t = (m_RotateSpeed * Time.deltaTime);
			Vector3 forward = Vector3.Slerp( this.transform.forward, inputDir, t );
			this.transform.rotation = Quaternion.LookRotation( forward );
		}

		m_Animator.SetBool( "isMove", isMove );
		m_Animator.SetBool( "isRun", isRun );


		// jump
		if( Input.GetButtonDown( "Jump" ) && m_IsGround		)
		{
			m_Animator.Play( "jump" );
			m_RigidBody.AddForce( Vector3.up * m_JumpForce );
		}

		// quit
		if( Input.GetKeyDown( KeyCode.Escape ) ) Application.Quit();

        // looking for player
        DetectPlayer(IsLookingThePlayer(playerTransform.position));
	}

    private float _timeOnSight;

    private void DetectPlayer(bool isLookingThePlayer)
    {
        if (isLookingThePlayer)
        {
            warningGameObject.SetActive(true);
            detectGameObject.SetActive(false);
            if(_timeOnSight < 2)
            {
                _timeOnSight += Time.deltaTime;
            }

            if (!(_timeOnSight >= 2)) return;
            detectGameObject.SetActive(true);
            warningGameObject.SetActive(false);

            if (activarVozUnityChan != null && !activarVozUnityChan.isPlaying) // RETROALIMENTACION DE SONIDO
            {
                activarVozUnityChan.Play();
                StartCoroutine(RecargarEscena()); // REINICIO DE LA ESCENA
            }
        }
        else
        {
            if(_timeOnSight > 0)
            {
                _timeOnSight -= Time.deltaTime;
            }

            if (!(_timeOnSight <= 0)) return;
            detectGameObject.SetActive(false);
            warningGameObject.SetActive(false);
        }
    }

    private bool IsLookingThePlayer(Vector3 playerPosition) // CODIGO DE ALEJO PARA LA DETECCION DEL JUGADOR
    {
        var displacement = playerPosition - _enemyTransform.position; // SE CALCULA LA DISTNCIA AL JUGADOR
        var distanceToPlayer = displacement.magnitude;

        if (!(distanceToPlayer <= sightRange)) return false;
        var dot = Vector3.Dot(_enemyTransform.forward, displacement.normalized);

        if (!(dot >= 0.65)) return false; // ANGULO DE VISION

        var layerMask = 1 << 2;

        layerMask = ~layerMask; // INVERSION DE LA OPERACION

        if (Physics.Raycast(_enemyTransform.position + new Vector3(0,1.5f,0), displacement.normalized, out var hit, sightRange, layerMask))
        {

            Debug.DrawRay(_enemyTransform.position + new Vector3(0, 1.5f, 0), displacement.normalized * hit.distance, Color.red); // ROJO SI LOS OBJETOS ESTAN CERCA PERO NO SE VEN
            Debug.Log("Did Hit"); // SE TIRA EL RAYO SI ESTA A LA DISTANCIA SOLICITADA

            if (hit.collider.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>())
            {
                Debug.DrawRay(_enemyTransform.position + new Vector3(0, 1.5f, 0), displacement.normalized * hit.distance, Color.green); // VERDE SI DETECTA AL JUGADOR
                
                activarVozUnityChan = hit.collider.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>().unityChanVoice;                
                return true;
            }
        }
        return false;
    }

    IEnumerator RecargarEscena()
    {
        yield return new WaitForSeconds(1.85f); // REINICIO EN EL TIEMPO QUE TARDA EL DIALOGO
        SceneManager.LoadScene("HouseEscape"); // CARGAR LA ESCENA CON EL NOMBRE ENTRE ""
    }
}
