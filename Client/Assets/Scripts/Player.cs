using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    private string username;

    [SerializeField] private Transform camTranform;
    [SerializeField] private PlayerAnimationManager animationManager;

  private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;
        if (!IsLocal)
        {
            camTranform.forward = forward;
            animationManager.AnimateBasedOnSpeed();
        }
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;

        list.Add(id, player);
    }

    #region Messages
    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message msg)
    {
        Spawn(msg.GetUShort(), msg.GetString(), msg.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message msg)
    {
        if (list.TryGetValue(msg.GetUShort(), out Player player))
            player.Move(msg.GetVector3(), msg.GetVector3());
    }
    #endregion
}
