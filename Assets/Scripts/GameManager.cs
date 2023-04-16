using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public enum GameState
{
    None,
    Connection,
    WaitingForConnection,
    Game
}

public enum Role
{
    None,
    Upstairs,
    Downstairs
}

public class GameManager : MonoBehaviour
{
    public RectTransform connectionScreen;
    public RectTransform waitingForConnectionScreen;
    public Button hostButton;
    public Button clientButton;
    public ControllerDisplay controllerDisplay;
    public Transform player;

    public NetworkInput networkInput;

    // Start is called before the first frame update
    void Start()
    {
        m_manager = FindObjectOfType<NetworkManager>();

        controllerDisplay.gameObject.SetActive(false);
        connectionScreen.gameObject.SetActive(false);
        waitingForConnectionScreen.gameObject.SetActive(false);

        hostButton.onClick.AddListener(HostButton);
        clientButton.onClick.AddListener(ClientButton);

        SetState(GameState.Connection);
    }

    // Update is called once per frame
    void Update()
    {
        // Update State
        switch (m_state)
        {
            case GameState.Connection:
            {
                if (m_manager.IsHost || m_manager.IsClient)
                {
                    SetState(GameState.WaitingForConnection);
                }
            }
            break;

            case GameState.WaitingForConnection:
            {
                switch(m_role)
                {
                    case Role.Upstairs:
                    {
                        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 2)
                        {
                            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                            {
                                if (NetworkManager.Singleton.LocalClientId == uid)
                                {
                                    m_serverUid = uid;
                                }
                                else
                                {
                                    m_clientUid = uid;
                                }
                            }
                            Debug.Log(string.Format("server:{0}, client:{1}", m_serverUid, m_clientUid));

                            player.GetComponent<NetworkObject>().ChangeOwnership(m_clientUid);
                            player.GetComponent<NetworkObject>().TrySetParent(NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(m_clientUid));

                            SetState(GameState.Game);
                        }
                    }
                    break;

                    case Role.Downstairs:
                    {
                        if (NetworkManager.Singleton.IsConnectedClient)
                        {
                            SetState(GameState.Game);
                        }
                    }
                    break;
                }
                //foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                //        NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
            }
            break;

            case GameState.Game:
            {
                switch (m_role)
                {
                    case Role.Upstairs:
                    {
                        float xAxis = 0.0f;
                        float yAxis = 0.0f;
                        if (Input.GetKey(KeyCode.LeftArrow))
                        {
                            xAxis -= 1.0f;
                        }
                        if (Input.GetKey(KeyCode.RightArrow))
                        {
                            xAxis += 1.0f;
                        }
                        if (Input.GetKey(KeyCode.UpArrow))
                        {
                            yAxis += 1.0f;
                        }
                        if (Input.GetKey(KeyCode.DownArrow))
                        {
                            yAxis -= 1.0f;
                        }

                        networkInput.xAxis.Value = xAxis;
                        networkInput.yAxis.Value = yAxis;
                    }
                    break;

                    case Role.Downstairs:
                    {
                        float xAxis = 0.0f;
                        float yAxis = 0.0f;

                        if (Input.GetKey(KeyCode.LeftArrow))
                        {
                            xAxis -= 1.0f;
                        }
                        if (Input.GetKey(KeyCode.RightArrow))
                        {
                            xAxis += 1.0f;
                        }
                        if (Input.GetKey(KeyCode.UpArrow))
                        {
                            yAxis += 1.0f;
                        }
                        if (Input.GetKey(KeyCode.DownArrow))
                        {
                            yAxis -= 1.0f;
                        }

                        float speed = 0.2f;
                        Vector3 playerPosition = player.transform.position;
                        playerPosition.x += xAxis * speed;
                        playerPosition.z += yAxis * speed;
                        player.transform.position = playerPosition;
                    }
                    break;
                }
            }
            break;
        }
    }

    void HostButton()
    {
        m_manager.StartHost();
    }

    void ClientButton()
    {
        m_manager.StartClient();
    }

    public void SetState(GameState _state)
    {
        if (_state == m_state)
            return;

        // Exit state
        switch (m_state)
        {
            case GameState.Connection:
            {
                connectionScreen.gameObject.SetActive(false);

                m_role = Role.None;
                if (m_manager.IsHost)
                {
                    m_role = Role.Upstairs;
                }
                else if (m_manager.IsClient)
                {
                    m_role = Role.Downstairs;
                    controllerDisplay.gameObject.SetActive(true);
                }
            }
            break;

            case GameState.WaitingForConnection:
            {
                waitingForConnectionScreen.gameObject.SetActive(false);
            }
            break;

            case GameState.Game:
            {

            }
            break;
        }
        m_state = _state;
        // Enter state
        switch (m_state)
        {
            case GameState.Connection:
            {
                connectionScreen.gameObject.SetActive(true);
            }
            break;

            case GameState.WaitingForConnection:
            {
                waitingForConnectionScreen.gameObject.SetActive(true);
            }
            break;

            case GameState.Game:
            {
            }
            break;
        }
    }

    NetworkManager m_manager;
    Role m_role = Role.None;
    GameState m_state = GameState.None;

    ulong m_serverUid;
    ulong m_clientUid;
}
