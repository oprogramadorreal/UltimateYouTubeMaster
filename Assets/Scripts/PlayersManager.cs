using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class PlayersManager : MonoBehaviour
{
    [SerializeField]
    private GameConfig config;

    [SerializeField]
    private GameObject subsPrefab;

    private struct PlayerAndPlanet
    {
        public Player Player;
        public Planet Planet;
    }

    private IEnumerable<PlayerAndPlanet> allPlayers;

    private void Awake()
    {
        allPlayers = FindAllPlayers();

        var subscribers = ReadSubscribers();
        var numberOfSubs = Mathf.Min(subscribers.Count, config.NumberOfSubsPlayers);
        var count = 0;

        foreach (var entry in subscribers)
        {
            if (++count > numberOfSubs)
            {
                break;
            }

            var player = Instantiate(subsPrefab, Vector3.zero, Quaternion.identity, transform.parent);
            player.name = entry.Key;
            player.GetComponentInChildren<Planet>().SetRadius(Mathf.Clamp(entry.Value / 5.0f, 1.0f, 5.0f));
        }
    }

    private IEnumerable<PlayerAndPlanet> FindAllPlayers()
    {
        return GameObject
            .FindGameObjectsWithTag("Player")
            .Select(o => new PlayerAndPlanet
            {
                Player = o.GetComponentInChildren<Player>(),
                Planet = o.GetComponentInChildren<Planet>()
            }).ToList();
    }

    private static Dictionary<string, int> ReadSubscribers()
    {
        var result = new Dictionary<string, int>();

        var subsResource = Resources.Load<TextAsset>("uytm_player_subs");
        var allLines = subsResource.text.Split(Environment.NewLine.ToCharArray());

        foreach (var line in allLines)
        {            
            if (!string.IsNullOrEmpty(line) && !line.StartsWith("{") && !line.StartsWith("}") && !line.StartsWith("//"))
            {
                char[] delimiterChars = { ':', '\t', '\"' };

                var tokens = line
                    .Substring(0, line.Length - 1)
                    .Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries)
                    .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

                var userName = tokens[0];
                var numberOfComments = Convert.ToInt32(tokens[1]);

                result[userName] = numberOfComments;
            }
        }

        return result;
    }

    private void LateUpdate()
    {
        var smallestPlanet = GetSmallestPlanet();
        
        if (smallestPlanet != null)
        {
            smallestPlanet.EnsureCanAbsorbMySubs();
        }
    }

    private Planet GetSmallestPlanet()
    {
        Planet result = null;
        var smallestScore = float.MaxValue;
        
        foreach (var p in GetAlivePlayers())
        {
            var score = p.Player.CalculateScore();

            if (score < smallestScore)
            {
                smallestScore = score;
                result = p.Planet;
            }
        }

        return result;
    }

    private IEnumerable<PlayerAndPlanet> GetAlivePlayers()
    {
        return allPlayers
            .Where(p => !p.Planet.IsDead)
            .ToList();
    }
}
