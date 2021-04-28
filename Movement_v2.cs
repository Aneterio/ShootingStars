using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_v2 : MonoBehaviour {

    [Header("1")]
    public Camera main_cam;
    [Header("2")]
    private Rigidbody rb;
    [Header("3")]
    /*
    The can_^^ variables are literal for movement
    Use can_^^_show to connect to the icons on the screen 
    */
    public bool can_jump;
    public GameObject can_jump_show;
    public bool can_dash;
    public GameObject can_dash_show;
    /*
    is_locked lets the player move when false 
    */
    public bool is_locked;
    [Header("4")]
    public float speed = 8.0f;
    public float jump_speed = 10.0f;
    public float gravity = 20.0f;
    public Vector3 move_direct = Vector3.zero;
    /* 
    x & y are variables directly related to the player position
    */
    public float x; //Sidewards
    public float y; //Forward-Backward
    /* 
    mx & my are variables directly related to the player-camera rotation
    */
    public float mx;  //Horizontal
    public float my;  //Vertical

    
    [Header("5")]
    public Vector3 spawn_trans;
    public Transform spawn_rot;
    /*
    The Spawn variable take the position-rotation of checkpoint objects
    so that it can respawn at them at death.
    */
    public GameObject blank_parent;

    
    void Start ()
    {
        rb = GetComponent<Rigidbody>();
        blank_parent = GameObject.Find("Start");

        Application.targetFrameRate = 60;
    }
	
	
	void Update ()
    {
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");

        if (!is_locked)
        {
            CharacterController cont = GetComponent<CharacterController>();
            if (cont.isGrounded)
            {
                move_direct = new Vector3(x, 0, y);
                move_direct = transform.TransformDirection(move_direct);
                move_direct *= speed;

                can_jump = true;
                can_dash = true;
            }
            else
            {
                if (can_dash)
                {
                    move_direct = new Vector3(x * 1.4f, move_direct.y, y * 1.4f);
                    move_direct = transform.TransformDirection(move_direct);
                    move_direct.x *= speed;
                    move_direct.z *= speed;
                }
                else
                {
                    move_direct = new Vector3(x * 0.7f, move_direct.y, (y + 2) * 1.4f);
                    move_direct = transform.TransformDirection(move_direct);
                    move_direct.x *= speed;
                    move_direct.z *= speed;
                }
            }

            if (Input.GetButtonDown("Jump") && can_jump || Input.GetButtonDown("Fire1") && can_jump)
            {
                move_direct.y = jump_speed;
                can_jump = false;
            }
            if (Input.GetButtonDown("Fire2") && can_dash || Input.GetButtonDown("Fire3") && can_dash)
            {
                move_direct = transform.TransformDirection(new Vector3(0, 0.35f, 1) * (speed * 2.4f));
                can_dash = false;
            }

            move_direct.y -= gravity * Time.deltaTime;
            cont.Move(move_direct * Time.deltaTime);
        }
	}

    void FixedUpdate()
    {
        LockThatMove(is_locked);
        if (!is_locked)
        {
            rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -4, 4), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -4, 4));

            mx = Input.GetAxis("Mouse X");
            my += Input.GetAxis("Mouse Y"); my = Mathf.Clamp(my, -80, 21);

            transform.Rotate(new Vector3(0, mx * 3, 0), Space.World);

            if (main_cam.transform.rotation.x <= 360)
            {
                main_cam.transform.localEulerAngles = new Vector3(-my * 1.2F, main_cam.transform.localEulerAngles.y, main_cam.transform.localEulerAngles.z);
            }
            if (-my * 1.2F > 70)
            {
                my += Mathf.Abs(Input.GetAxis("Mouse Y"));
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit coll)
    {
        string tag = coll.gameObject.tag;

        if (coll.gameObject.tag != "No")
        {
            can_jump = true;
            can_dash = true;

            transform.SetParent(coll.transform);
        }

        switch (tag)
        {
            case "Check":
                spawn_trans = coll.gameObject.transform.position;
                spawn_rot = coll.gameObject.transform;
                break;
            case "Reset_Jump":
                can_jump = true;
                break;
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        string tag = coll.gameObject.tag;

        switch (tag)
        {
            case "Reset":
                Token.Teleport(this.gameObject, spawn_trans, spawn_rot);
                break;
            case "Jumper":
                if (can_dash) {move_direct.y = jump_speed * 2; } else { move_direct.y = jump_speed * 1.25f; }
                
                break;
            case "Reset_Dash":
                can_dash = true;
                break;
            case "Reset_Jump":
                can_jump = true;
                break;
            case "Reset_Both":
                can_dash = true;
                can_jump = true;
                break;

            case "Level":
                GameObject fader = coll.GetComponent<multiTool>().fader;
                string lvl = coll.GetComponent<multiTool>().level;
                if (fader != null) { Level_Load.Ping_Level(fader, lvl); } else { Debug.Log("Please Insert Fader"); }
                can_jump = false; can_dash = false;
                break;
        }
    }

    void OnTriggerExit(Collider coll)
    {
        string tag = coll.gameObject.tag;
        switch (tag)
        {
            case "Move":
                transform.SetParent(blank_parent.transform);
                break;
        }
    }

    void LockThatMove(bool unp)
    {
        Rigidbody temp = gameObject.GetComponent<Rigidbody>();
        if (unp)
        {
            temp.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            temp.constraints = RigidbodyConstraints.FreezeRotation;
            temp.constraints = ~RigidbodyConstraints.FreezePosition;
        }
    }
}
