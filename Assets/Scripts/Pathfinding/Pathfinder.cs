using Nito.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using VoxelPlay;

public class Pathfinder
{
    private SpriteMovement spriteMovement;

    private const int MAX_PATH_LENGTH = 1000;
    private const int MAX_SAVED_NODES = 1000000;

    private PriorityQueue<Node, float> frontier = new PriorityQueue<Node, float>();
    private Dictionary<Vector3Int, Node> positionToNode = new Dictionary<Vector3Int, Node>();
    // keeps track of changed values since we can't update them directly in the priority queue
    private Dictionary<Node, float> changedNodes = new Dictionary<Node, float>();
    private Deque<Vector3Int> path = new Deque<Vector3Int>();
    private Vector3Int? previousStartPoint = null;

    public Pathfinder(SpriteMovement spriteMovement) {
        this.spriteMovement = spriteMovement;
    }

    private void ResetStructures(Node start) {
        if (!previousStartPoint.HasValue) {
            return;
        }
        path.Clear();
        changedNodes.Clear();
        frontier.Clear();

        // reuse old nodes so we can reuse their scores
        if (positionToNode.Count > MAX_SAVED_NODES) {
            positionToNode.Clear();
        }
        else if (previousStartPoint.Value == start.position) {
            foreach (Node node in positionToNode.Values) {
                node.visited = false;
                node.heuristicScore = float.MaxValue;
            }
        }
        else {
            positionToNode.Clear();
        }

        Debug.Log($"Reusing {positionToNode.Count} nodes.");
    }

    public Deque<Vector3Int> FindPath(Vector3Int startPosition, Vector3Int endPosition,
            bool includeFinalPosition) {
        Debug.Log($"Finding path to {endPosition}");
        Node start = new Node(startPosition);
        Node end = new Node(endPosition);

        if (includeFinalPosition && !spriteMovement.IsReachablePosition(end.position, true)) {
            Debug.Log("End position isn't reachable.");
            return path;
        }

        ResetStructures(start);
        previousStartPoint = start.position;

        start.score = 0;
        start.heuristicScore = 0;

        frontier.Enqueue(start, start.heuristicScore);
        positionToNode[start.position] = start;

        Node currNode;
        int numLoops = 0;
        while (true) {
            if (numLoops > 10000) {
                Debug.LogError("Ran into infinite loop");
                return path;
            }
            numLoops += 1;

            currNode = GetLowestHeuristicScoreUnvisited();
            if (currNode == null || currNode.score >= MAX_PATH_LENGTH) {
                Debug.Log("Path would be too long, stopping search.");
                return path;
            }
            currNode.visited = true;

            // get neighbors of current node
            if (currNode.neighbors.Count == 0) {
                foreach (Vector3Int coordinate in Coordinates.GetAdjacentCoordinates(currNode.position)) {
                    Node neighbor;
                    if (!positionToNode.ContainsKey(coordinate)) {
                        neighbor = new Node(coordinate);
                        positionToNode[coordinate] = neighbor;
                    }
                    neighbor = positionToNode[coordinate];
                    currNode.neighbors.Add(neighbor);
                }
            }

            // update neighbor costs
            foreach (Node neighbor in currNode.neighbors) {
                if (!neighbor.visited) {
                    bool includeOccupiedCoordinates = neighbor.position == endPosition && !includeFinalPosition;
                    float newScore = CalculateDistance(currNode, neighbor, includeOccupiedCoordinates) + currNode.score;
                    if (newScore <= neighbor.score) {
                        float oldScore = neighbor.score;
                        neighbor.score = newScore;
                        neighbor.heuristicScore = neighbor.score + CalculateDirectLineLength(neighbor, end);
                        neighbor.prevNode = currNode;

                        // update the value in the priority queue
                        if (oldScore != newScore) {
                            if (!changedNodes.ContainsKey(neighbor)) {
                                changedNodes[neighbor] = float.MaxValue;
                            }
                            changedNodes[neighbor] = neighbor.heuristicScore;
                        }
                    }
                    frontier.Enqueue(neighbor, neighbor.heuristicScore);
                }
            }

            if (currNode.position == end.position) {
                Debug.Log("Finished path");
                if (!includeFinalPosition) {
                    currNode = currNode.prevNode;
                }

                while (currNode != null && currNode.position != start.position) {
                    path.AddToBack(currNode.position);
                    currNode = currNode.prevNode;
                }
                return path;
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

    private Node GetLowestHeuristicScoreUnvisited() {
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
            if (changedNodes.ContainsKey(topNode) && changedNodes[topNode] < topNode.heuristicScore) {
                frontier.Dequeue();
                frontier.Enqueue(topNode, changedNodes[topNode]);
                continue;
            }
            break;
        }

        return frontier.Dequeue();
    }
}
