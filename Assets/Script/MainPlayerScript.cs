using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class MainPlayerScript : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    Rigidbody rb;
    public TMP_Text namePrefab;
    private TMP_Text nameLabel;

    private LoginManagerScript loginManagerScript;

    private NetworkVariable<int> posX = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct NetworkString : INetworkSerializable
    {
        public FixedString32Bytes info;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref info);
        }
        public override string ToString()
        {
            return info.ToString();
        }
        public static implicit operator NetworkString(string v) =>
            new NetworkString() { info = new FixedString32Bytes(v)};
    }

    private NetworkVariable<NetworkString> playerNameA = new NetworkVariable<NetworkString>(
        new NetworkString { info = "player" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(
        new NetworkString { info = "player" },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public override void OnNetworkSpawn()
    {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero,Quaternion.identity) as TMP_Text;
        nameLabel.transform.SetParent(canvas.transform);
        posX.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log("Owner ID = " + OwnerClientId + " : pos X = " + posX.Value);
        };
        /*if (IsServer)
        {
            playerNameA.Value = new NetworkString() { info = new FixedString32Bytes(name.text) };
            playerNameB.Value = new NetworkString() { info = new FixedString32Bytes(name.text) };
        }*/

        if (IsOwner)
        {
            loginManagerScript = GameObject.FindAnyObjectByType<LoginManagerScript>();
            if(loginManagerScript != null)
            {
                string name = loginManagerScript.userNameInputField.text;
                if (IsOwnedByServer) { playerNameA.Value = name; }
                else{ playerNameB.Value = name; }
            }
        }
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
        UpdatePlayerInfo();
    }

    private void UpdatePlayerInfo()
    {
        if (IsOwnedByServer) { nameLabel.text = playerNameA.Value.ToString(); }
        else { nameLabel.text = playerNameB.Value.ToString(); }
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
