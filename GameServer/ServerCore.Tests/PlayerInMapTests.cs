using NUnit.Framework;
using MapHandler;
using Storage;
using Storage.TestDataBuilder;
using ServerCore;
using ServerCore.Tests.TestUtilities;
using Common.Networking.Packets;
using Storage.Players;
using ServerCore.GameServer.Players;
using ServerCore.GameServer.Entities;

namespace MapTests
{
    [TestFixture]
    public class PlayerInMapTests
    {
        private StoredPlayer _player;
        private Server _server = new Server(123);

        [SetUp]
        public void Start()
        {
            Redis redis = new Redis();
            redis.Start();
            TestDb.Create();
            _server.StartGameThread();
            _player = new StoredPlayer()
            {
                UserId = "wololo",
                Login = "login",
                Password = "password",
                MoveSpeed = 1,
                X = 0,
                Y = 0
            };
        }

        [Test]
        public void TestPlayerMoving()
        {
            var client = ServerMocker.GetClient();
            client.FullLoginSequence(_player);

            client.SendToServer(new EntityMovePacket()
            {
                From = new Position(_player.X, _player.Y),
                To = new Position(_player.X - 1, _player.Y),
                UID = _player.UserId
            });

            var player = RedisHash<StoredPlayer>.Get(_player.UserId);

            Assert.That(player.X == _player.X - 1, 
                "Server did not store that the player moved");

            var onlinePlayer = Server.GetPlayer(_player.UserId);

            Assert.That(onlinePlayer.Position.X == _player.X - 1, 
                "Server did not update online player position");

            var chunk = onlinePlayer.GetChunk();

            Assert.That(chunk.x == -1, 
                "Player should have moved to other chunk");

            Assert.That(chunk.PlayersInChunk.Contains(onlinePlayer),
                "New chunk did not contain the player reference");

        }

    }
}

