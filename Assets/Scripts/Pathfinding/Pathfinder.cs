using Instantiated;
using Nito.Collections;
using NonVoxel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using VoxelPlay;
using static UnityEngine.EventSystems.EventTrigger;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] SpriteMovement spriteMovement;
    [SerializeField] NonVoxelWorld nonVoxelWorld;

    private const int MAX_PATH_LENGTH = 1000;
    private const float MAX_SEARCH_TIME = 0.1f;

    PriorityQueue<Node, float> frontier1 = new();
    Dictionary<Vector3Int, Node> positionToNode1 = new();
    // keeps track of changed values since we can't update them directly in the priority queue
    Dictionary<Node, KeyValuePair<float, float>> changedNodes1 = new();
    PriorityQueue<Node, float> frontier2 = new();
    Dictionary<Vector3Int, Node> positionToNode2 = new();
    Dictionary<Node, KeyValuePair<float, float>> changedNodes2 = new();

    private Deque<Vector3Int> EMPTY_RESULTS = new(0);

    private void ClearDataStructures() {
        frontier1.Clear();
        positionToNode1.Clear();
        changedNodes1.Clear();
        frontier2.Clear();
        positionToNode2.Clear();
        changedNodes2.Clear();
    }

    public IEnumerator FindPath(Traveller traveller, Vector3Int endPosition,
            int maxSearchDepth = MAX_PATH_LENGTH) {
        // when the traveller is large or greater, we need to allow movement onto tiles occupied by itself
        List<TangibleEntity> ignoredCreatures = new() { traveller };

        // check if any position in the target is occupied by someone else
        HashSet<Vector3Int> endPositions = traveller.GetPositionsIfOriginAtPosition(endPosition);
        InstantiatedEntity occupyingEntity = null;
        foreach (Vector3Int position in endPositions) {
            if (nonVoxelWorld.IsPositionOccupied(position, ignoredCreatures)) {
                occupyingEntity = nonVoxelWorld.GetEntityFromPosition(position);
                break;
            }
        }

        if (occupyingEntity == null) {
            CoroutineWithData<Deque<Vector3Int>> coroutineWithData = new(this,
                FindPathInternal(traveller.origin, endPosition, traveller, ignoredCreatures, maxSearchDepth));
            yield return coroutineWithData.coroutine;
            yield return coroutineWithData.GetResult();
            yield break;
        }

        // TODO - make this possible (need to allow non-square types)
        if (!TypeUtils.IsSameTypeOrIsSubclass(occupyingEntity, typeof(Traveller))) {
            Debug.Log("Tried to move into tile with a non-traveller object.");
            yield return EMPTY_RESULTS;
            yield break;
        }

        // find reachable origins
        Traveller target = (Traveller)occupyingEntity;
        List<Vector3Int> reachableOriginsNextToTarget = Coordinates
            .GetOriginPositionsWhereXIsNextToY(traveller, target)
            .Where((Vector3Int position) => {
                return spriteMovement.IsReachablePosition(position, traveller, ignoredCreatures);
        }).ToList();
        reachableOriginsNextToTarget.Sort((x, y) => {
            return Coordinates.GetDirectLineLength(x, traveller.origin)
            .CompareTo(Coordinates.GetDirectLineLength(y, traveller.origin));
        });
        if (reachableOriginsNextToTarget.Count == 0) {
            Debug.Log("Found nowhere to put traveller next to selected target.");
            yield return EMPTY_RESULTS;
            yield break;
        }

        // find smallest path out of reachable origins
        PriorityQueue<Deque<Vector3Int>, int> pathLengths = new();
        foreach (Vector3Int targetOrigin in reachableOriginsNextToTarget) {
            int newMaxSearch = pathLengths.Count > 0 ? pathLengths.Peek().Count : maxSearchDepth;
            CoroutineWithData<Deque<Vector3Int>> coroutineWithData = new(this,
                FindPathInternal(traveller.origin, targetOrigin, traveller, ignoredCreatures,
                newMaxSearch));
            yield return coroutineWithData.coroutine;
            Deque<Vector3Int> result = coroutineWithData.GetResult();
            if (result.Count != 0) {
                pathLengths.Enqueue(result, result.Count);
            }
        }
        if (pathLengths.Count != 0) {
            yield return pathLengths.Dequeue();
            yield break;
        }

        Debug.Log($"Found no path to {reachableOriginsNextToTarget.Count}");
        yield return EMPTY_RESULTS;
    }

    private IEnumerator FindPathInternal(Vector3Int startPosition, 
            Vector3Int endPosition, Traveller traveller, List<TangibleEntity> ignoredCreatures,
            int maxSearchDepth) {
        ClearDataStructures();
        Deque<Vector3Int> result = new();

        Node start1 = new(startPosition);
        Node end1 = new(endPosition);
        Node start2 = new(endPosition);
        Node end2 = new(startPosition);

        if (!spriteMovement.IsReachablePosition(end1.origin, traveller, ignoredCreatures)) {
            Debug.Log("End position isn't reachable.");
            yield return result;
            yield break;
        }

        start1.score = 0;
        start1.heuristicScore = 0;
        start2.score = 0;
        start2.heuristicScore = 0;

        frontier1.Enqueue(start1, start1.heuristicScore);
        positionToNode1[start1.origin] = start1;
        frontier2.Enqueue(start2, start2.heuristicScore);
        positionToNode2[start2.origin] = start2;

        float searchStartTime = Time.realtimeSinceStartup;

        Node currNode1;
        Node currNode2;
        int numLoops = 0;
        while (true) {
            if (Time.realtimeSinceStartup - searchStartTime > MAX_SEARCH_TIME) {
                // need to take a break to allow frames to be drawn
                yield return null;
                searchStartTime = Time.realtimeSinceStartup;
            }

            if (numLoops > 10000) {
                Debug.LogError("Ran into infinite loop");
                yield return result;
                yield break;
            }
            numLoops += 1;

            currNode1 = GetLowestHeuristicScoreUnvisited(frontier1, changedNodes1);
            currNode2 = GetLowestHeuristicScoreUnvisited(frontier2, changedNodes2);
            if (currNode1.score >= float.MaxValue || currNode2.score >= float.MaxValue) {
                Debug.Log("No possible path.");
                yield return result;
                yield break;
            }
            if (currNode1.score > maxSearchDepth || currNode2.score > maxSearchDepth) {
                Debug.Log($"Path of score {currNode1.score} would be too long, stopping search.");
                yield return result;
                yield break;
            }
            currNode1.visited = true;
            currNode2.visited = true;

            // get neighbors of current node
            foreach (Vector3Int coordinate in Coordinates.GetAdjacentCoordinates(currNode1.origin)) {
                Node neighbor;
                if (!positionToNode1.ContainsKey(coordinate)) {
                    neighbor = new(coordinate);
                    positionToNode1[coordinate] = neighbor;
                }
                neighbor = positionToNode1[coordinate];
                currNode1.neighbors.Add(neighbor);
            }
            foreach (Vector3Int coordinate in Coordinates.GetAdjacentCoordinates(currNode2.origin)) {
                Node neighbor;
                if (!positionToNode2.ContainsKey(coordinate)) {
                    neighbor = new(coordinate);
                    positionToNode2[coordinate] = neighbor;
                }
                neighbor = positionToNode2[coordinate];
                currNode2.neighbors.Add(neighbor);
            }

            // update neighbor costs
            foreach (Node neighbor in currNode1.neighbors) {
                if (!neighbor.visited) {
                    float newScore = CalculateDistance(currNode1, neighbor, traveller, ignoredCreatures) 
                        + currNode1.score;
                    if (newScore <= neighbor.score) {
                        float oldScore = neighbor.score;
                        neighbor.score = newScore;
                        neighbor.heuristicScore = neighbor.score 
                            + Coordinates.GetDirectLineLength(neighbor.origin, end1.origin);
                        neighbor.prevNode = currNode1;

                        if (oldScore != newScore) {
                            changedNodes1[neighbor] = new KeyValuePair<float, float>(
                                newScore, neighbor.heuristicScore);
                        }
                    }
                    frontier1.Enqueue(neighbor, neighbor.heuristicScore);
                }
            }
            foreach (Node neighbor in currNode2.neighbors) {
                if (!neighbor.visited) {
                    float newScore = CalculateDistance(currNode2, neighbor, traveller, ignoredCreatures) 
                        + currNode2.score;
                    if (newScore <= neighbor.score) {
                        float oldScore = neighbor.score;
                        neighbor.score = newScore;
                        neighbor.heuristicScore = neighbor.score 
                            + Coordinates.GetDirectLineLength(neighbor.origin, end2.origin);
                        neighbor.prevNode = currNode2;

                        if (oldScore != newScore) {
                            changedNodes1[neighbor] = new KeyValuePair<float, float>(
                                newScore, neighbor.heuristicScore);
                        }
                    }
                    frontier2.Enqueue(neighbor, neighbor.heuristicScore);
                }
            }

            if (currNode1.origin == end1.origin) {
                while (currNode1 != start1) {
                    result.AddToBack(currNode1.origin);
                    currNode1 = currNode1.prevNode;
                }
                yield return result;
                yield break;
            }
            else if (currNode2.origin == end2.origin) {
                while (currNode2 != null) {
                    result.AddToFront(currNode2.origin);
                    currNode2 = currNode2.prevNode;
                }
                yield return result;
                yield break;
            }
        }
    }

    private float CalculateDistance(Node start, Node destination, Traveller traveller,
            List<TangibleEntity> ignoredCreatures) {
        Vector3Int? terrainAdjustedCoordinate = spriteMovement.GetTerrainAdjustedCoordinate(
            destination.origin, traveller, ignoredCreatures);
        if (!terrainAdjustedCoordinate.HasValue || terrainAdjustedCoordinate.Value != destination.origin) {
            return float.MaxValue;
        }

        return Coordinates.GetDirectLineLength(destination.origin, start.origin);
    }

    private Node GetLowestHeuristicScoreUnvisited(PriorityQueue<Node, float> frontier,
            Dictionary<Node, KeyValuePair<float, float>> changedNodes) {
        while (true) {
            if (frontier.Count == 0) {
                return null;
            }

            // get rid of visited or outdated nodes
            Node topNode = frontier.Peek();
            if (topNode.visited) {
                frontier.Dequeue();
                continue;
            }
            if (changedNodes.ContainsKey(topNode) && changedNodes[topNode].Value < topNode.heuristicScore) {
                frontier.Dequeue();
                topNode.score = changedNodes[topNode].Key;
                topNode.heuristicScore = changedNodes[topNode].Value;
                changedNodes.Remove(topNode);
                frontier.Enqueue(topNode, topNode.heuristicScore);
                continue;
            }
            break;
        }

        return frontier.Dequeue();
    }
}
