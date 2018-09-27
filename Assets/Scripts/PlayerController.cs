using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float sensetivity = 3f;

    [SerializeField]
    private float enduranceBurnSpeed = 2f;
    [SerializeField]
    private float enduranceRegenSpeed = 0.5f;
    private float enduranceMaxAmount = 10f;
    private float enduranceAmount = 10f;
    private float sprintMultiplier = 2f;

    [SerializeField]
    private float thrusterForce = 1000f;

    [Header("Spring settings:")]
    [SerializeField]
    private JointDriveMode jointMode = JointDriveMode.Position;
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;

    private PlayerMovement movement;
    private ConfigurableJoint joint;
    private Animator animator;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();

        SetJointSettings(jointSpring);
    }

    private void Update()
    {
        if (PauseMenu.IsOn)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            movement.Move(Vector3.zero);
            movement.Rotate(Vector3.zero);
            movement.RotateCamera(0f);
            animator.SetFloat("Forward", 0, 0.1f, Time.deltaTime);
            animator.SetFloat("Right", 0, 0.1f, Time.deltaTime);

            enduranceAmount += enduranceRegenSpeed * Time.deltaTime;
            enduranceAmount = Mathf.Clamp(enduranceAmount, 0f, enduranceMaxAmount);

            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        RaycastHit _hit;
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, 100f))
        {
            joint.targetPosition = new Vector3(0, -_hit.point.y, 0);
        }
        else
        {
            joint.targetPosition = new Vector3(0, 1, 0);
        }

        //Вычисление направления движения
        float _xMov = Input.GetAxis("Horizontal");
        float _zMov = Input.GetAxis("Vertical");

        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;

        //Расчет скорости движения с учетом спринта
        float _sprintMultiplier = 1f;
        if (Input.GetKey(KeyCode.LeftShift) && _zMov > 0 && _xMov == 0)
        {
            if (enduranceAmount > 0f)
            {
                enduranceAmount -= enduranceBurnSpeed * Time.deltaTime;
                _sprintMultiplier = sprintMultiplier;
            }
        }

        Vector3 _thrusterForce = Vector3.zero;
        if (Input.GetKey(KeyCode.Space) && enduranceAmount >= 0.1f)
        {
            enduranceAmount -= enduranceBurnSpeed * Time.deltaTime;
            _thrusterForce = Vector3.up * thrusterForce;
            SetJointSettings(0f);
        }
        else
        {
            SetJointSettings(jointSpring);
        }

        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.Space))
        {
            enduranceAmount += enduranceRegenSpeed * Time.deltaTime;
        }

        movement.ApplyThruster(_thrusterForce);

        enduranceAmount = Mathf.Clamp(enduranceAmount, 0f, enduranceMaxAmount);

        //Конечный вектор движения
        Vector3 _velocity = (_movHorizontal + _movVertical) * speed * _sprintMultiplier;

        //Анимирование движения
        animator.SetFloat("Forward", _zMov, 0.1f, Time.deltaTime);
        animator.SetFloat("Right", _xMov, 0.1f, Time.deltaTime);

        //Применение изменения движения 
        movement.Move(_velocity);

        //Вычисление поворота по горизонтали
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * sensetivity;

        //Применение изменения поворотов по горизонтали
        movement.Rotate(_rotation);

        //Вычисление поворотов для камеры по вертикали
        float _xRot = Input.GetAxisRaw("Mouse Y");
        

        float _cameraRotationX = _xRot * sensetivity;

        //Применение изменения поворотов для камеры по вертикали
        movement.RotateCamera(_cameraRotationX);

    }

    private void SetJointSettings(float _jointSpring)
    {
        joint.yDrive = new JointDrive
        {
            mode = jointMode,
            positionSpring = _jointSpring,
            maximumForce = jointMaxForce
        };
    }

    public float GetEndurancePct()
    {
        return enduranceAmount / enduranceMaxAmount;
    }
}
