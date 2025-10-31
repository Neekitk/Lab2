using System;
using System.Collections.Generic;

namespace Players
{
    enum PlayerStatus
    {
        Idle,
        Moving,
        Shooting
    }
    enum PlayerRole
    {
        Sniper,
        Assault,
        Support
    }
    struct PlayerPosition
    {
        public float X;
        public float Y;
        public PlayerPosition(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    class Player
    {
        private static int Id = 1;

        public int PlayerID { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public int Health { get; set; }
        public int Money { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public PlayerStatus Status { get; set; }
        public PlayerRole Role { get; set; }
        public PlayerPosition Position { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public int Experience { get; set; }
        public float Accuracy { get; set; }
        public int Level { get; set; }
        public Player()
        {
            PlayerID = Id++;
            Name = "Unknown";
            Team = "None";
            Health = 100;
            Money = 800;
            Kills = 0;
            Deaths = 0;
            Status = PlayerStatus.Idle;
            Role = PlayerRole.Assault;
            Position = new PlayerPosition(0, 0);
            Country = "Unknown";
            Language = "English";
            Experience = 0;
            Accuracy = 50.0f;
            Level = 1;
        }
        public Player(string name, string team, int health = 100, int money = 800) : this()
        {
            Name = name;
            Team = team;
            Health = health;
            Money = money;
        }
        public void Move() => Position = new PlayerPosition(Position.X + 1, Position.Y + 1);
        public void ShowStats() => Console.WriteLine(ToString());
        public override string ToString()
        {
            return $"ID:{PlayerID} | {Name} [{Team}] HP:{Health} Money:{Money} Kills:{Kills} Deaths:{Deaths} Status:{Status} Role:{Role} Pos:({Position.X},{Position.Y}) " +
                $"Country:{Country} Lang:{Language} Exp:{Experience} Acc:{Accuracy} Lv:{Level}";
        }
        public override bool Equals(object obj) => obj is Player p && p.PlayerID == PlayerID;
        public override int GetHashCode() => PlayerID.GetHashCode();
        public static void EnsureNextId(int Idfile)
        {
            if (Idfile >= Id)
                Id = Idfile + 1;
        }
    }

    // Сортировка по PlayerID (по возрастанию)
    class PlayerIdComparer : IComparer<Player>
    {
        public int Compare(Player x, Player y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            int cmp = x.PlayerID.CompareTo(y.PlayerID);
            if (cmp == 0) // чтобы SortedSet не считала равными разные объекты с одинаковым ID
                return x.GetHashCode().CompareTo(y.GetHashCode());
            return cmp;
        }
    }
}