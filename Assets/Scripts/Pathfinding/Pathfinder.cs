using Nito.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using VoxelPlay;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] SpriteMovement spriteMovement;

    private const int MAX_PATH_LENGTH = 1000;
    private const float MAX_SEARCH_TIME = 0.1f;

    PriorityQueue<Node, float> frontier1 = new PriorityQueue<Node, float>();
    Dictionary<Vector3Int, Node> positionToNode1 = new Dictionary<Vector3Int, Node>();
    // keeps track of changed values since we can't update them directly in the priority queue
    Dictionary<Node, KeyValuePair<float, float>> changedNodes1
        = new Dictionary<Node, KeyValuePair<float, float>>();
    PriorityQueue<Node, float> frontier2 = new PriorityQueue<Node, float>();
    Dictionary<Vector3Int, Node> positionToNode2 = new Dictionary<Vector3Int, Node>();
    Dictionary<Node, KeyValuePair<float, float>> changedNodes2
        = new Dictionary<Node, KeyValuePair<float, float>>();

    private void ClearDataStructures() {
        frontier1.Clear();
        positionToNode1.Clear();
        changedNodes1.Clear();
        frontier2.Clear();
        positionToNode2.Clear();
        changedNodes2.Clear();
    }

    public IEnumerator FindPath(Vector3Int startPosition, Vector3Int endPosition,
            bool includeFinalPosition) {
        ClearDataStructures();
        Deque<Vector3Int> result = new Deque<Vector3Int>();

        bool includeFinalPosition1 = includeFinalPosition;
        bool includeFinalPosition2 = false;
        Node start1 = new Node(startPosition);
        Node end1 = new Node(endPosition);
        Node start2 = new Node(endPosition);
        Node end2 = new Node(startPosition);

        if (includeFinalPosition && !spriteMovement.IsReachablePosition(end1.position, true)) {
            Debug.Log("End position isn't reachable.");
            yield break;
        }

        start1.score = 0;
        start1.heuristicScore = 0;
        start2.score = 0;
        start2.heuristicScore = 0;

        frontier1.Enqueue(start1, start1.heuristicScore);
        positionToNode1[start1.position] = start1;
        frontier2.Enqueue(start2, start2.heuristicScore);
        positionToNode2[start2.position] = start2;

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
            if (currNode1.score >= MAX_PATH_LENGTH || currNode2.score >= MAX_PATH_LENGTH) {
                Debug.Log("Path would be too long, stopping search.");
                yield return result;
                yield break;
            }
            currNode1.visited = true;
            currNode2.visited = true;

            // get neighbors of current node
            foreach (Vector3Int coordinate in Coordinates.GetAdjacentCoordinates(currNode1.position)) {
                Node neighbor;
                if (!positionToNode1.ContainsKey(coordinate)) {
                    neighbor = new Node(coordinate);
                    positionToNode1[coordinate] = neighbor;
                }
                neighbor = positionToNode1[coordinate];
                currNode1.neighbors.Add(neighbor);
            }
            foreach (Vector3Int coordinate in Coordinates.GetAdjacentCoordinates(currNode2.position)) {
                Node neighbor;
                if (!positionToNode2.ContainsKey(coordinate)) {
                    neighbor = new Node(coordinate);
                    positionToNode2[coordinate] = neighbor;
                }
                neighbor = positionToNode2[coordinate];
                currNode2.neighbors.Add(neighbor);
            }

            // update neighbor costs
            foreach (Node neighbor in currNode1.neighbors) {
                if (!neighbor.visited) {
                    bool includeOccupiedCoordinates = neighbor.position == end1.position 
                        && !includeFinalPosition1;
                    float newScore = CalculateDistance(currNode1, neighbor, includeOccupiedCoordinates) 
                        + currNode1.score;
                    if (newScore <= neighbor.score) {
                        float oldScore = neighbor.score;
                        neighbor.score = newScore;
                        neighbor.heuristicScore = neighbor.score + CalculateDirectLineLength(neighbor, end1);
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
                    bool includeOccupiedCoordinates = neighbor.position == end2.position 
                        && !includeFinalPosition2;
                    float newScore = CalculateDistance(currNode2, neighbor, includeOccupiedCoordinates) 
                        + currNode2.score;
                    if (newScore <= neighbor.score) {
                        float oldScore = neighbor.score;
                        neighbor.score = newScore;
                        neighbor.heuristicScore = neighbor.score + CalculateDirectLineLength(neighbor, end2);
                        neighbor.prevNode = currNode2;

                        if (oldScore != newScore) {
                            changedNodes1[neighbor] = new KeyValuePair<float, float>(
                                newScore, neighbor.heuristicScore);
                        }
                    }
                    frontier2.Enqueue(neighbor, neighbor.heuristicScore);
                }
            }

            if (currNode1.position == end1.position) {
                if (!includeFinalPosition1) {
                    currNode1 = currNode1.prevNode;
                }

                while (currNode1 != start1) {
                    result.AddToBack(currNode1.position);
                    currNode1 = currNode1.prevNode;
                }
                yield return result;
                yield break;
            }
            else if (currNode2.position == end2.position) {
                if (!includeFinalPosition2) {
                    currNode2 = currNode2.prevNode;
                }

                while (currNode2 != null) {
                    result.AddToFront(currNode2.position);
                    currNode2 = currNode2.prevNode;
                }
                yield return result;
                yield break;
            }
        }
    }

    private float CalculateDistance(Node node1, Node node2, bool includeOccupiedCoordinates) {
        if (spriteMovement.IsATraversibleFromB(node2.position, node1.position, includeOccupiedCoordinates)) {
            return CalculateDirectLineLength(node2, node1);
        }
        return float.MaxValue;
    }

    private float CalculateDirectLineLength(Node curr, Node end) {
        return Mathf.Abs((end.position - curr.position).magnitude);
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
