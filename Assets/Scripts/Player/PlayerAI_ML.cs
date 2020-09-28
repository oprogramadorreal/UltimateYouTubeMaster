using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Planet))]
[RequireComponent(typeof(Player))]
public sealed class PlayerAI_ML : Agent
{
    private Planet planet;
    private Player player;
    private int swallowedCount = 0;

    private LearningState previousState = null;

    public override void Initialize()
    {
        planet = GetComponent<Planet>();
        player = GetComponent<Player>();

        player.SwallowedOther += (o, e) =>
        {
            AddReward(100.0f * ++swallowedCount);

            if (swallowedCount >= 10)
            {
                AddReward(1000.0f);
                Debug.Log("Swallowed 10!!! " + GetCumulativeReward());

                //player.EndOfTheArena();
            }
            else
            {
                Debug.Log("Swallowed other! " + GetCumulativeReward());
            }
        };

        player.WasSwallowed += (o, e) =>
        {
            AddReward(-100.0f);

            //if (o is BlackHolePlanet)
            //{
            //    AddReward(-100.0f);
            //    Debug.Log("Was swallowed by the black hole! " + GetCumulativeReward());
            //}
            //else
            {
                Debug.Log("Was swallowed! " + GetCumulativeReward());
            }

            Reset();
        };
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    private void Reset()
    {
        player.Respawn(player.GetValidRandomPositionInsideArena(), 7);
        swallowedCount = 0;
        previousState = null;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        MovePlayer(new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]));
        AddReward();
    }

    private void AddReward()
    {
        var currentState = new LearningState(player, planet);

        if (previousState != null)
        {
            var rewardStep = currentState.CalculateRewardFrom(previousState);
            AddReward(rewardStep);

            //Debug.Log("Reward: " + rewardStep + " || Total: " + GetCumulativeReward());

        }

        previousState = currentState;
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
        actionsOut[2] = 0.0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var closestPlayer = planet.GetClosestPlanet();

        sensor.AddObservation(planet.Center);
        sensor.AddObservation(closestPlayer.Center);
        sensor.AddObservation(planet.Radius);
        sensor.AddObservation(closestPlayer.Radius);
        sensor.AddObservation(planet.GetDistanceToTheBlackHole());
    }

    private void MovePlayer(Vector3 direction)
    {
        player.Move(2 * direction);
    }

    private sealed class LearningState
    {
        private readonly ClosePlayerInfo closestPlayer;
        private readonly float distanceToClosestPlayer;
        private readonly bool playerCanSwallowClosestPlayer;
        private readonly float distanceToTheBlackHole;

        public LearningState(Player player, Planet playerPlanet)
        {
            closestPlayer = player.GetClosestPlayer();
            distanceToClosestPlayer = closestPlayer.Planet.DistanceTo(playerPlanet);
            playerCanSwallowClosestPlayer = player.CanSwallow(closestPlayer);
            distanceToTheBlackHole = playerPlanet.GetDistanceToTheBlackHole();
        }

        public float CalculateRewardFrom(LearningState previousState)
        {
            var reward = 0.0f;

            if (SameAction(previousState))
            {
                if (IsBetterThanOtherConsideringPlanetSize(previousState))
                {
                    // No penalty! Good!
                }
                else
                {
                    reward -= 10.0f * Mathf.Abs(previousState.distanceToClosestPlayer - distanceToClosestPlayer);
                }
            }

            // Try to minimize the distance to the black hole.
            reward += 0.01f * (distanceToTheBlackHole - previousState.distanceToTheBlackHole);

            return reward;
        }

        private bool IsBetterThanOtherConsideringPlanetSize(LearningState previousState)
        {
            if (playerCanSwallowClosestPlayer)
            {
                return distanceToClosestPlayer < previousState.distanceToClosestPlayer;
            }

            return distanceToClosestPlayer > previousState.distanceToClosestPlayer;
        }

        private bool SameAction(LearningState other)
        {
            return ReferenceEquals(closestPlayer, other.closestPlayer)
                && playerCanSwallowClosestPlayer == other.playerCanSwallowClosestPlayer;
        }
    }
}
