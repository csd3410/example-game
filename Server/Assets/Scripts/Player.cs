using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;

        list.Add(id, player);
    }

    #region Messages
    private void SendSpawned()
    {
        Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerSpawned);
        msg.AddUShort(Id);
        msg.AddString(Username);
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message msg)
    {
        Spawn(fromClientId, msg.GetString());
    }
    #endregion
}
