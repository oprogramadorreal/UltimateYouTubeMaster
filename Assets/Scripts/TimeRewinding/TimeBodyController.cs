using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Based on https://youtu.be/eqlHpPzS22U
/// </summary>
public sealed class TimeBodyController : MonoBehaviour
{
    private const float recordTimeInSeconds = 20.0f;

    private bool isRewinding = false;

    private readonly LinkedList<PointInTime> pointsInTime = new LinkedList<PointInTime>();

    private ITimeBody[] allBodies;

    private void Awake()
    {
        allBodies = GetComponentsInChildren<ITimeBody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartRewind();
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            StopRewind();
        }
    }

    private void FixedUpdate()
    {
        if (isRewinding)
        {
            Rewind();
        }
        else if (Physics.autoSimulation)
        {
            Record();
        }
    }

    private void Rewind()
    {
        if (pointsInTime.Count > 0)
        {
            pointsInTime.Last().Restore();
            pointsInTime.RemoveLast();
        }
    }

    private void Record()
    {
        if (pointsInTime.Count > Mathf.Round(recordTimeInSeconds / Time.fixedDeltaTime))
        {
            pointsInTime.RemoveFirst();
        }

        pointsInTime.AddLast(CreatePointInTime());
    }

    private void StartRewind()
    {
        foreach (var b in allBodies)
        {
            b.RewindStarted();
        }

        isRewinding = true;
        Physics.autoSimulation = false;
    }

    private void StopRewind()
    {
        isRewinding = false;

        foreach (var b in allBodies)
        {
            b.RewindStopped();
        }
    }

    private PointInTime CreatePointInTime()
    {
        return new PointInTime(allBodies);
    }

    private sealed class PointInTime
    {
        private readonly ITimeBody[] allBodies;
        private readonly IList<IMemento> mementos = new List<IMemento>();

        public PointInTime(ITimeBody[] allBodies)
        {
            this.allBodies = allBodies;

            foreach (var b in allBodies)
            {
                mementos.Add(b.CreateMemento());
            }
        }

        public void Restore()
        {
            for (var i = 0; i < allBodies.Length; ++i)
            {
                allBodies[i].RestoreMemento(mementos[i]);
            }
        }
    }
}
