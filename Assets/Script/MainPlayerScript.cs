using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MainPlayerScript : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    Rigidbody rb;
    public TMP_Text namePrefab;
    private TMP_Text nameLabel;

    private NetworkVariable<int> posX = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero,Quaternion.identity) as TMP_Text;
        nameLabel.transform.SetParent(canvas.transform);
        posX.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : pos X = " + posX.Value);
        };
    }
    // Start is called before the first frame update

    private void Update()
    {
        Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3.5f, 0));
        nameLabel.text = gameObject.name;
        nameLabel.transform.position = nameLabelPos;
        if (IsOwner)
        {
            posX.Value = (int)System.Math.Ceiling(transform.position.x);
        }
    }

    private void OnDestroy()
    {
        if (nameLabel != null) Destroy(nameLabel.gameObject);
        base.OnDestroy();
    }

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (IsOwner)
        {
            float translation = Input.GetAxis("Vertical") * speed;
            translation *= Time.deltaTime;
            rb.MovePosition(rb.position + this.transform.forward * translation);

            float rotation = Input.GetAxis("Horizontal");
            if (rotation != 0)
            {
                rotation *= rotationSpeed;
                Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
                rb.MoveRotation(rb.rotation * turn);
            }
            else
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}